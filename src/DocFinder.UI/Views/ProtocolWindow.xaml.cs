using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DocFinder.Services;
using DocFinder.Domain;
using Microsoft.EntityFrameworkCore;
using Wpf.Ui.Controls;
using DocFinder.UI.ViewModels.Entities;

namespace DocFinder.UI.Views;

public partial class ProtocolWindow : FluentWindow
{
    private readonly DocumentDbContext _context;
    private readonly ObservableCollection<ProtocolViewModel> _protocols;
    private readonly ICollectionView _view;
    private readonly string? _filePathFilter;

    public ProtocolWindow(string? filePathFilter = null, DbContextOptions<DocumentDbContext>? dbOptions = null)
    {
        InitializeComponent();
        _filePathFilter = filePathFilter;
        _context = dbOptions != null ? new DocumentDbContext(dbOptions) : new DocumentDbContext();

        var query = _context.Protocols.Include(p => p.File).AsQueryable();
        if (!string.IsNullOrEmpty(_filePathFilter))
            query = query.Where(p => p.File.FilePath == _filePathFilter);

        _protocols = new ObservableCollection<ProtocolViewModel>(
            query.AsEnumerable().Select(p => new ProtocolViewModel(p)).ToList());
        protocolsGrid.ItemsSource = _protocols;

        _view = CollectionViewSource.GetDefaultView(_protocols);
        _view.Filter = Filter;
    }

    private bool Filter(object obj)
    {
        if (obj is not ProtocolViewModel p) return false;
        return Match(p.Title, TitleFilter.Text)
            && Match(p.ReferenceNumber, RefFilter.Text)
            && Match(p.ResponsiblePerson, PersonFilter.Text);
    }

    private static bool Match(string? value, string filter)
        => string.IsNullOrEmpty(filter) || (value?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);

    private void OnFilterChanged(object sender, TextChangedEventArgs e) => _view.Refresh();

    private void ProtocolsGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit) return;
        if (e.Row.Item is ProtocolViewModel vm)
        {
            var protocol = _context.Protocols.Include(p => p.File).FirstOrDefault(p => p.Id == vm.Id);
            if (protocol != null)
            {
                protocol.SetTitle(vm.Title);
                protocol.SetReferenceNumber(vm.ReferenceNumber);
                protocol.SetIssuedBy(vm.IssuedBy);
                protocol.SetResponsiblePerson(vm.ResponsiblePerson);
                _context.SaveChanges();
            }
        }
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        if (protocolsGrid.SelectedItem is ProtocolViewModel vm && !string.IsNullOrEmpty(vm.FilePath))
        {
            if (File.Exists(vm.FilePath))
            {
                Process.Start(new ProcessStartInfo(vm.FilePath) { UseShellExecute = true });
            }
            else
            {
                var messageBox = new MessageBox
                {
                    Title = "DocFinder",
                    Content = "Soubor neexistuje.",
                    CloseButtonText = "OK"
                };
                messageBox.ShowDialogAsync().GetAwaiter().GetResult();
            }
        }
    }
}
