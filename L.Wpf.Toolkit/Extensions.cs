using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace System.Windows
{
    public static class Extensions
    {
        public static IEnumerable<TElement> FindChildren<TElement>(this DependencyObject obj)
           where TElement : DependencyObject
        {
            var childCount = VisualTreeHelper.GetChildrenCount(obj);
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(obj, i);
                    if (child is TElement)
                        yield return (TElement)child;

                    foreach (var item in FindChildren<TElement>(child))
                    {
                        yield return item;
                    }
                }
            }
        }
        public static TParent FindParent<TParent>(this DependencyObject child)
            where TParent : DependencyObject
        {
            DependencyObject parent;
            if (child is Visual || child is Visual3D)
            {
                parent = VisualTreeHelper.GetParent(child);
            }
            else
            {
                parent = LogicalTreeHelper.GetParent(child);
            }
            if (parent == null)
                return null;
            if (parent is TParent)
                return (TParent)parent;
            return FindParent<TParent>(child);
        }
    }
}
