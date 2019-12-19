using Prism.Ioc;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Prism
{
    public class RegionNavigationItem : Button
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(RegionNavigationItem), new PropertyMetadata(null));
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(RegionNavigationItem), new PropertyMetadata(null));
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(Geometry), typeof(RegionNavigationItem), new PropertyMetadata(null));
        public static readonly DependencyProperty DataFillProperty =
            DependencyProperty.Register(nameof(DataFill), typeof(Brush), typeof(RegionNavigationItem), new PropertyMetadata(null));
        public static readonly DependencyProperty DataStrokeProperty =
            DependencyProperty.Register(nameof(DataStroke), typeof(Brush), typeof(RegionNavigationItem), new PropertyMetadata(null));
        public static readonly DependencyProperty DataStrokeThicknessProperty =
            DependencyProperty.Register(nameof(DataStrokeThickness), typeof(double), typeof(RegionNavigationItem), new PropertyMetadata(0.0));
        static RegionNavigationItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RegionNavigationItem), new FrameworkPropertyMetadata(typeof(RegionNavigationItem)));
        }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public Geometry Data
        {
            get { return (Geometry)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        public Brush DataFill
        {
            get { return (Brush)GetValue(DataFillProperty); }
            set { SetValue(DataFillProperty, value); }
        }
        public Brush DataStroke
        {
            get { return (Brush)GetValue(DataStrokeProperty); }
            set { SetValue(DataStrokeProperty, value); }
        }
        public double DataStrokeThickness
        {
            get { return (double)GetValue(DataStrokeThicknessProperty); }
            set { SetValue(DataStrokeThicknessProperty, value); }
        }
    }
    public class RegionNavigationItem<TView> : RegionNavigationItem
    {
        public static readonly DependencyProperty TargetRegionNameProperty =
          DependencyProperty.Register(nameof(TargetRegionName), typeof(string), typeof(RegionNavigationItem<TView>), new PropertyMetadata(null));
        public string TargetRegionName
        {
            get { return (string)GetValue(TargetRegionNameProperty); }
            set { SetValue(TargetRegionNameProperty, value); }
        }
        protected override void OnClick()
        {
            base.OnClick();
            this.NavigateToView<TView>(TargetRegionName);
        }
    }
}
