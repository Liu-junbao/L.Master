// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace System.Windows.Charts
{
    /// <summary>This panel automatically builds legend for given <see cref="Plot"/></summary>
    [Description("Presents legend items for a plot")]    
    public class LegendItemsPanel : Panel
    {
        private IDisposable unsubsrciber;
        private PlotBase masterPlot = null;
        public static readonly DependencyProperty LegendTemplateProperty =
          DependencyProperty.Register(nameof(LegendTemplate), typeof(DataTemplate), typeof(LegendItemsPanel), new PropertyMetadata(null));
        public static readonly DependencyProperty LegendTemplateSelectorProperty =
            DependencyProperty.Register(nameof(LegendTemplateSelector), typeof(DataTemplateSelector), typeof(LegendItemsPanel), new PropertyMetadata(null));


        /// <summary>
        /// Identifies <see cref="MasterPlot"/> property
        /// </summary>
        public static readonly DependencyProperty MasterPlotProperty =
            DependencyProperty.Register("MasterPlot", typeof(PlotBase), typeof(LegendItemsPanel), new PropertyMetadata(null,
                (o, e) =>
                {
                    LegendItemsPanel panel = (LegendItemsPanel)o;
                    if (panel != null)
                    {
                        panel.OnMasterPlotChanged(e);
                    }
                }));


        /// <summary>
        /// Gets of sets plot to build legend for
        /// </summary>
        [Category("InteractiveDataDisplay")]
        [Description("Plot to build legend for")]
        public PlotBase MasterPlot
        {
            get { return (PlotBase)GetValue(MasterPlotProperty); }
            set { SetValue(MasterPlotProperty, value); }
        }
        public DataTemplate LegendTemplate
        {
            get { return (DataTemplate)GetValue(LegendTemplateProperty); }
            set { SetValue(LegendTemplateProperty, value); }
        }


        public DataTemplateSelector LegendTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(LegendTemplateSelectorProperty); }
            set { SetValue(LegendTemplateSelectorProperty, value); }
        }

       
        private void LegendItemsPanelUnloaded(object sender, RoutedEventArgs e)
        {
            if (MasterPlot == null)
                masterPlot = null;
        }

        private void LegendItemsPanelLoaded(object sender, RoutedEventArgs e)
        {
            if (MasterPlot == null)
                masterPlot = PlotBase.FindMaster(this);
            else
                masterPlot = MasterPlot;

            Resubscribe();

        }

        private void OnMasterPlotChanged(DependencyPropertyChangedEventArgs e)
        {
            PlotBase newPlot = (PlotBase)e.NewValue;

            if (newPlot != null)
                masterPlot = newPlot;

            Resubscribe();
        }

        private void Resubscribe()
        {
            if (unsubsrciber != null)
            {
                unsubsrciber.Dispose();
                unsubsrciber = null;
            }

            if (masterPlot != null)
            {
                unsubsrciber = masterPlot.CompositionChange.Subscribe(change => RefreshContents());
                RefreshContents();
            }
        }

        /// <summary>
        /// Measures the size in layout required for child elements and determines a size for parent. 
        /// </summary>
        /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            Size result = new Size();
            foreach (UIElement c in Children)
            {
                c.Measure(availableSize);
                result.Width = Math.Max(result.Width, c.DesiredSize.Width);
                if (!Double.IsInfinity(availableSize.Height))
                    availableSize.Height = Math.Max(0, availableSize.Height - c.DesiredSize.Height);
                result.Height += c.DesiredSize.Height;
            }
            return result;
        }

        /// <summary>
        /// Positions child elements and determines a size for parent.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that Figure should use to arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            double y = 0;
            foreach (UIElement c in Children)
            {
                c.Arrange(new Rect(new Point(0, y), c.DesiredSize));
                y += c.DesiredSize.Height;
            }
            return finalSize;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LegendItemsPanel"/> class.
        /// </summary>
        public LegendItemsPanel()
        {
            Loaded += LegendItemsPanelLoaded;
            Unloaded += LegendItemsPanelUnloaded;
        }

        /// <summary>
        /// Constructs legend UI for current plot
        /// </summary>
        protected virtual void RefreshContents()
        {
            Children.Clear();

            if (masterPlot != null)
            {
                foreach (var elt in masterPlot.RelatedPlots)
                {
                    var c = CreateLegendContent(elt);
                    if (c != null)
                        Children.Add(c);
                }
            }

            InvalidateMeasure();
        }

        /// <summary>
        /// Creates legend content from given UIElement
        /// </summary>
        /// <param name="element">UIElement for legend content construction</param>
        /// <returns>UIElement which reperesents input UIElement in legend control</returns>
        protected virtual UIElement CreateLegendContent(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (!Legend.GetIsVisible(element))
                return null;

            if (element is Plot == false)
                return null;

            var legendTemplate = LegendTemplate;
            if (legendTemplate==null)
                legendTemplate = LegendTemplateSelector?.SelectTemplate(element, this);
            if (legendTemplate != null)
            {
                var templateView = legendTemplate.LoadContent() as FrameworkElement;
                templateView.DataContext = element;
                return templateView;
            }
            return null;
        }
        
    }
}

