using System.Windows;
using DocFinder.Domain;

namespace DocFinder.UI.Services;

public interface IDocumentViewService
{
    UIElement GetViewer(SearchHit hit);
}
