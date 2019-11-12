using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace System.Windows.Charts
{
    public class LocalLegendDataTemplateSelector:DataTemplateSelector
    {
        public DataTemplate LineGraphTemplate { get; set; }
        public DataTemplate MarkerGraphTemplate { get; set; }
        public DataTemplate HeatmapGraphTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null) return null;
            if (item is LineGraph) return LineGraphTemplate;
            if (item is MarkerGraph) return MarkerGraphTemplate;
            if (item is HeatmapGraph) return HeatmapGraphTemplate;
            return null;
        }
    }
}
