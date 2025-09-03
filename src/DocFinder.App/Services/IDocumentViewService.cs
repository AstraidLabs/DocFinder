using System.Windows;
using DocFinder.Domain;

namespace DocFinder.App.Services;

public interface IDocumentViewService
{
    UIElement GetViewer(SearchHit hit);
}
