using System.Windows;
using System.Windows.Controls;

namespace DocFinder.App.Views.Layout
{
    public partial class HeaderBodyFooterLayout : UserControl
    {
        public HeaderBodyFooterLayout()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header), typeof(object), typeof(HeaderBodyFooterLayout), new PropertyMetadata(null));

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register(
                nameof(Body), typeof(object), typeof(HeaderBodyFooterLayout), new PropertyMetadata(null));

        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register(
                nameof(Footer), typeof(object), typeof(HeaderBodyFooterLayout), new PropertyMetadata(null));

        public object? Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
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
