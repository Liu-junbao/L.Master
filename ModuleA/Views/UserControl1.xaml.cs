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

namespace ModuleA.Views
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }



        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(UserControl1), new PropertyMetadata(null));

        public override string ToString()
        {
            return Value.ToString();
        }

        public static readonly RoutedEvent TestEvent =
                    EventManager.RegisterRoutedEvent(nameof(Test), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UserControl1));
        public event RoutedEventHandler Test
        {
            add { this.AddHandler(TestEvent, value); }
            remove { this.RemoveHandler(TestEvent, value); }
        }
        private void RaiseTest()
        {
            var e = new RoutedEventArgs(TestEvent);
            OnTest(e);
        }
        protected virtual void OnTest(RoutedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.RaiseTest();
        }
    }
}
