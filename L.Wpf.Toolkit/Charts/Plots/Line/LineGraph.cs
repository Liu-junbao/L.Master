// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Collections;
using System.Globalization;
using System.ComponentModel;

namespace System.Windows.Charts
{
    /// <summary>
    /// A plot to draw simple line.
    /// </summary>
    [Description("Plots line graph")]
    public class LineGraph : Plot
    {
        public static readonly DependencyProperty StrokeProperty = 
            DependencyProperty.Register("Stroke",typeof(Brush), typeof(LineGraph), new PropertyMetadata(new SolidColorBrush(Colors.Black), OnStrokeChanged));
        public static readonly DependencyProperty StrokeDashArrayProperty =
            DependencyProperty.Register("StrokeDashArray",typeof(DoubleCollection),typeof(LineGraph),new PropertyMetadata(EmptyDoubleCollection, OnStrokeDashArrayChanged));
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness",typeof(double),typeof(LineGraph),new PropertyMetadata(1.0));
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(LineGraph),new PropertyMetadata(null,OnDesriptionChanged));
        private static void OnStrokeChanged(object target, DependencyPropertyChangedEventArgs e)
        {
            LineGraph lineGraph = (LineGraph)target;
            lineGraph.polyline.Stroke = e.NewValue as Brush;
        }
        private static void OnDesriptionChanged(object target, DependencyPropertyChangedEventArgs e)
        {
            var lg = (LineGraph)target;
            ToolTipService.SetToolTip(lg, e.NewValue);
        }
        private static void PointsPropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LineGraph linePlot = (LineGraph)d;
            if (linePlot != null)
            {
                SetPoints(linePlot.polyline, (PointCollection)e.NewValue);
            }
        }
        static LineGraph()
        {
            PointsProperty.OverrideMetadata(typeof(LineGraph), new PropertyMetadata(new PointCollection(), PointsPropertyChangedHandler));
        }


        private Polyline polyline;
        public LineGraph()
        {
            polyline = new Polyline 
            { 
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeLineJoin = PenLineJoin.Round
            };

            BindingOperations.SetBinding(polyline, Polyline.StrokeThicknessProperty, new Binding("StrokeThickness") { Source = this });
            BindingOperations.SetBinding(this, PlotBase.PaddingProperty, new Binding("StrokeThickness") { Source = this, Converter = new LineGraphThicknessConverter() });

            Children.Add(polyline);
        }

        /// <summary>
        /// Gets or sets line graph points.
        /// </summary>
        [Category("InteractiveDataDisplay")]
        [Description("Line graph points")]
        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }
        /// <summary>
        /// Updates data in <see cref="Points"/> and causes a redrawing of line graph.
        /// </summary>
        /// <param name="x">A set of x coordinates of new points.</param>
        /// <param name="y">A set of y coordinates of new points.</param>
        public void Plot(IEnumerable x, IEnumerable y)
        {
            if (x == null)
                throw new ArgumentNullException("x");
            if (y == null)
                throw new ArgumentNullException("y");

            var points = new PointCollection();
            var enx = x.GetEnumerator();
            var eny = y.GetEnumerator();
            while (true)
            {
                var nx = enx.MoveNext();
                var ny = eny.MoveNext();
                if (nx && ny)
                    points.Add(new Point(Convert.ToDouble(enx.Current, CultureInfo.InvariantCulture), 
                        Convert.ToDouble(eny.Current, CultureInfo.InvariantCulture)));
                else if (!nx && !ny)
                    break;
                else
                    throw new ArgumentException("x and y have different lengthes");
            }

            Points = points;
        }

        /// <summary>
        /// Updates data in <see cref="Points"/> and causes a redrawing of line graph.
        /// In this version a set of x coordinates is a sequence of integers starting with zero.
        /// </summary>
        /// <param name="y">A set of y coordinates of new points.</param>
        public void PlotY(IEnumerable y)
        {
            if (y == null)
                throw new ArgumentNullException("y");
            int x = 0;
            var en = y.GetEnumerator();
            var points = new PointCollection();
            while (en.MoveNext())
                points.Add(new Point(x++, Convert.ToDouble(en.Current, CultureInfo.InvariantCulture)));

            Points = points;
        }

        /// <summary>
        /// Gets or sets the line thickness.
        /// </summary>
        /// <remarks>
        /// The default stroke thickness is 1.0
        /// </remarks>
        [Category("Appearance")]
        public double StrokeThickness
        {
            get
            {
                return (double)GetValue(StrokeThicknessProperty);
            }
            set
            {
                SetValue(StrokeThicknessProperty, value);
            }
        }
        /// <summary>
        /// Gets or sets description text for line graph. Description text appears in default
        /// legend and tooltip.
        /// </summary>
        [Category("InteractiveDataDisplay")]
        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }
            set
            {
                SetValue(DescriptionProperty, value);
            }
        }
        /// <summary>
        /// Gets or sets the brush to draw the line.
        /// </summary>
        /// <remarks>
        /// The default color of stroke is black
        /// </remarks>
        [Category("Appearance")]
        public Brush Stroke
        {
            get
            {
                return (Brush)GetValue(StrokeProperty);
            }
            set
            {
                SetValue(StrokeProperty, value);
            }
        }
        private static DoubleCollection EmptyDoubleCollection
        {
            get
            {
                var result = new DoubleCollection(0);
                result.Freeze();
                return result;
            }
        }
        private static void OnStrokeDashArrayChanged(object target, DependencyPropertyChangedEventArgs e)
        {
            LineGraph lineGraph = (LineGraph)target;
            lineGraph.polyline.StrokeDashArray = e.NewValue as DoubleCollection;
        }
        /// <summary>
        /// Gets or sets a collection of <see cref="Double"/> values that indicate the pattern of dashes and gaps that is used to draw the line.
        /// </summary>
        [Category("Appearance")]
        public DoubleCollection StrokeDashArray
        {
            get
            {
                return (DoubleCollection)GetValue(StrokeDashArrayProperty);
            }
            set
            {
                SetValue(StrokeDashArrayProperty, value);
            }
        }
    }

    internal class LineGraphThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double thickness = (double)value;
            return new Thickness(thickness / 2.0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

