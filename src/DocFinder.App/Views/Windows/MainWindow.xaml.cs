using DocFinder.App.ViewModels.Windows;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DocFinder.App.Views.Windows
{
    public partial class MainWindow : FluentWindow, INavigationWindow
    {
        public MainWindowViewModel? ViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            SystemThemeWatcher.Watch(this);
        }

        public MainWindow(
            MainWindowViewModel viewModel,
            INavigationViewPageProvider navigationViewPageProvider,
            INavigationService navigationService
        ) : this()
        {
            ViewModel = viewModel;
            DataContext = this;
            SetPageService(navigationViewPageProvider);

            navigationService.SetNavigationControl(BodyView.Navigation);
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => BodyView.Navigation;

        public bool Navigate(Type pageType) => BodyView.Navigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => BodyView.Navigation.SetPageProviderService(navigationViewPageProvider);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            System.Windows.Application.Current.Shutdown();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}
