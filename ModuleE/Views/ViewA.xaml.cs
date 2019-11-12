using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ModuleE.Views
{
    /// <summary>
    /// Interaction logic for ViewA.xaml
    /// </summary>
    public partial class ViewA : UserControl
    {
        private DispatcherTimer _timer;
        public ViewA()
        {
            InitializeComponent();
            tran.SelectedIndex = 0;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += Timer_Tick;
            _timer.IsEnabled = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (tran.SelectedIndex < tran.Items.Count - 1)
            {
                tran.SelectedIndex++;
            }
            else tran.SelectedIndex = 0;
        }
    }
}
