using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace DocFinder.App.Views.Layout
{
    public partial class BodyView : UserControl
    {
        public BodyView()
        {
            InitializeComponent();
        }

        public NavigationView Navigation => RootNavigation;
    }
}
