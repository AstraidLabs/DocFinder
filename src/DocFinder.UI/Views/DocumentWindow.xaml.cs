using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DocFinder.Domain;
using DocFinder.Services;
using DocFinder.Search;
using Microsoft.EntityFrameworkCore;
using Wpf.Ui.Controls;

namespace DocFinder.UI.Views;

public partial class DocumentWindow : FluentWindow
{
    private readonly DocumentDbContext _context;
    private readonly ObservableCollection<Document> _documents;
    private readonly ICollectionView _view;

    public DocumentWindow() : this(null)
    {
    }

    public DocumentWindow(DbContextOptions<DocumentDbContext>? dbOptions)
    {
        InitializeComponent();
        _context = dbOptions != null
            ? new DocumentDbContext(dbOptions)
            : new DocumentDbContext();
        var dbPath = _context.Database.GetDbConnection().DataSource;
        if (!Path.IsPathRooted(dbPath))
            dbPath = Path.GetFullPath(dbPath);
        if (!File.Exists(dbPath))
        {
            var result = MessageBox.Show(
                "Databáze nebyla nalezena. Vytvořit novou?",
                "Chybějící databáze",
                MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _context.Database.EnsureCreated();
            }
            else
            {
                Close();
                return;
            }
        }
        else
        {
            _context.Database.EnsureCreated();
        }
        _documents = new ObservableCollection<Document>(_context.Documents.ToList());
        documentsGrid.ItemsSource = _documents;
        _view = CollectionViewSource.GetDefaultView(_documents);
        _view.Filter = Filter;
    }

    private bool Filter(object obj)
    {
        if (obj is not Document d) return false;
        return Match(d.BuildingName, BuildingFilter.Text)
            && Match(d.Name, NameFilter.Text)
            && Match(d.Author, AuthorFilter.Text)
            && Match(d.ModifiedAt.ToString(), ModifiedFilter.Text)
            && Match(d.Version, VersionFilter.Text)
            && Match(d.Type, TypeFilter.Text)
            && Match(d.IssuedAt?.ToString() ?? string.Empty, IssuedFilter.Text)
            && Match(d.ValidUntil?.ToString() ?? string.Empty, ValidFilter.Text)
            && MatchBool(d.CanPrint, (CanPrintFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty)
            && MatchBool(d.IsElectronic, (ElectronicFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty);
    }

    private static bool Match(string? value, string filter)
        => string.IsNullOrEmpty(filter) || (value?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);

    private static bool MatchBool(bool value, string filter)
    {
        if (string.IsNullOrEmpty(filter)) return true;
        return bool.TryParse(filter, out var b) && b == value;
    }

    private void OnTextFilterChanged(object sender, TextChangedEventArgs e) => _view.Refresh();
    private void OnBoolFilterChanged(object sender, SelectionChangedEventArgs e) => _view.Refresh();

    private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit) return;
        if (e.Row.Item is Document doc)
        {
            if (doc.Id == 0)
                _context.Documents.Add(doc);
            else
                _context.Documents.Update(doc);
            _context.SaveChanges();
        }
    }

    private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && documentsGrid.SelectedItem is Document doc)
        {
            _context.Documents.Remove(doc);
            _context.SaveChanges();
        }
    }

    private void DocumentsGrid_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths) return;
        foreach (var path in paths)
        {
            if (!File.Exists(path))
                continue;
            if (_documents.Any(d => string.Equals(d.FileLink, path, StringComparison.OrdinalIgnoreCase)))
                continue;
            var info = new FileInfo(path);
            var doc = new Document(
                id: 0,
                buildingName: Path.GetFileName(Path.GetDirectoryName(path) ?? string.Empty),
                name: Path.GetFileNameWithoutExtension(path),
                author: string.Empty,
                modifiedAt: info.LastWriteTime,
                version: string.Empty,
                type: info.Extension.Trim('.'),
                issuedAt: null,
                validUntil: null,
                canPrint: false,
                isElectronic: false,
                fileLink: path);
            _context.Documents.Add(doc);
            _documents.Add(doc);
        }
        _context.SaveChanges();
    }

    private void FileLink_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBlock tb && tb.DataContext is Document doc)
        {
            if (File.Exists(doc.FileLink))
            {
                Process.Start(new ProcessStartInfo(doc.FileLink) { UseShellExecute = true });
            }
            else
            {
                System.Windows.MessageBox.Show("Soubor neexistuje.");
            }
        }
    }
}
