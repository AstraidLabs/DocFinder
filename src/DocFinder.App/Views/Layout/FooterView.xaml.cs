using System;
using System.Windows.Controls;

namespace DocFinder.App.Views.Layout
{
    public partial class FooterView : UserControl
    {
        public FooterView()
        {
            InitializeComponent();
            DataContext = this;
            FooterText = $"© {DateTime.Now.Year} DocFinder";
        }

        public string FooterText { get; set; }
    }
}
