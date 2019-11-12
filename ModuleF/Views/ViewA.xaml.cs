using Prism;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Charts;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModuleF.Views
{
    /// <summary>
    /// Interaction logic for ViewA.xaml
    /// </summary>
    public partial class ViewA : UserControl,IActiveAware
    {
        public ViewA()
        {
            InitializeComponent();

            InitializeLines();
        }

        public bool IsActive { get; set; }
        public event EventHandler IsActiveChanged;
        private async void InitializeLines()
        {
            var cpu = await Task.Run(()=> new PerformanceCounter("Processor", "% Processor Time", "_Total"));
            var lg = new LineGraph();
            lines.Children.Add(lg);
            lg.Stroke = new SolidColorBrush(Colors.YellowGreen);
            lg.Description = "CPU";
            lg.StrokeThickness = 2;
            RunLineAsync(lg,cpu,0,new List<double>(),new List<double>());
        }
        private async void RunLineAsync(LineGraph line, PerformanceCounter cpu, int index, List<double> x, List<double> y)
        {
            index++;
            x.Add(index);
            y.Add(cpu.NextValue());
            line.Plot(x, y);
            await Task.Delay(100);
            RunLineAsync(line, cpu, index, x, y);
        }
    }

    public class VisibilityToCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
