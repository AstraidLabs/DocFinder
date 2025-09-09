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

        private readonly ISnackbarService _snackbarService;
        private IServiceProvider? _serviceProvider;

        public MainWindow()
        {
            InitializeComponent();
            SystemThemeWatcher.Watch(this);
            _snackbarService = null!;
        }

        public MainWindow(
            MainWindowViewModel viewModel,
            INavigationViewPageProvider navigationViewPageProvider,
            INavigationService navigationService,
            ISnackbarService snackbarService
        ) : this()
        {
            _snackbarService = snackbarService;
            snackbarService.SetSnackbarPresenter(SnackbarPresenter);

            ViewModel = viewModel;
            DataContext = ViewModel;

            SetPageService(navigationViewPageProvider);
            navigationService.SetNavigationControl(RootNavigation);
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider)
            => RootNavigation.SetPageProviderService(navigationViewPageProvider);

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

        public void SetServiceProvider(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;
    }
}
