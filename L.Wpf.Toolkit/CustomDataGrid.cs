using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace System.Windows
{
    public class CustomDataGrid:DataGrid
    {
        static CustomDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomDataGrid), new FrameworkPropertyMetadata(typeof(CustomDataGrid)));
        }
    }
}
