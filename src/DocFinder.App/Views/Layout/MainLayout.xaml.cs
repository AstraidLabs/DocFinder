using System.Windows;
using System.Windows.Controls;

namespace DocFinder.App.Views.Layout
{
    public partial class MainLayout : UserControl
    {
        public MainLayout()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header), typeof(object), typeof(MainLayout), new PropertyMetadata(null));

        public static readonly DependencyProperty MenuProperty =
            DependencyProperty.Register(
                nameof(Menu), typeof(object), typeof(MainLayout), new PropertyMetadata(null));

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register(
                nameof(Body), typeof(object), typeof(MainLayout), new PropertyMetadata(null));

        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register(
                nameof(Footer), typeof(object), typeof(MainLayout), new PropertyMetadata(null));

        public object? Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public object? Menu
        {
            get => GetValue(MenuProperty);
            set => SetValue(MenuProperty, value);
        }

        public object? Body
        {
            get => GetValue(BodyProperty);
            set => SetValue(BodyProperty, value);
        }

        public object? Footer
        {
            get => GetValue(FooterProperty);
            set => SetValue(FooterProperty, value);
        }
    }
}
