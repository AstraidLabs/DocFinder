using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DocFinder.Services;
using DocFinder.Domain;
using Microsoft.EntityFrameworkCore;
using DocFinder.App.ViewModels.Entities;
using Wpf.Ui.Controls;

namespace DocFinder.App.Views.Controls;

public partial class ProtocolListControl : UserControl
{
    private readonly DocumentDbContext _context;
    private readonly ObservableCollection<ProtocolViewModel> _protocols = new();
    private readonly ICollectionView _view;
    private string? _filePathFilter;

    public ProtocolListControl()
    {
        InitializeComponent();
        _context = new DocumentDbContext();
        protocolsGrid.ItemsSource = _protocols;
        _view = CollectionViewSource.GetDefaultView(_protocols);
        _view.Filter = Filter;
        LoadProtocols();
    }

    private void LoadProtocols()
    {
        var query = _context.Protocols.Include(p => p.File).AsQueryable();
        if (!string.IsNullOrEmpty(_filePathFilter))
            query = query.Where(p => p.File.FilePath == _filePathFilter);
        _protocols.Clear();
        foreach (var p in query.AsEnumerable().Select(p => new ProtocolViewModel(p)))
            _protocols.Add(p);
    }

    public void FilterByPath(string? filePath)
    {
        _filePathFilter = filePath;
        LoadProtocols();
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
            if (System.IO.File.Exists(vm.FilePath))
            {
                Process.Start(new ProcessStartInfo(vm.FilePath) { UseShellExecute = true });
            }
            else
            {
                var messageBox = new MessageBox
                {
                    Title = "DocFinder",
                    Content = "Soubor neexistuje.",
                    CloseButtonText = "OK",
                };
                messageBox.ShowDialogAsync().GetAwaiter().GetResult();
            }
        }
    }
}
