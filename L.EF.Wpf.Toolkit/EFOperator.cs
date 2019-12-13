using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows
{
    public class EFOperator : EFEditorBase
    {
        public static readonly DependencyProperty CanDeleteProperty =
           DependencyProperty.Register(nameof(CanDelete), typeof(bool), typeof(EFOperator), new PropertyMetadata(true));
        static EFOperator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EFOperator), new FrameworkPropertyMetadata(typeof(EFOperator)));
        }
        public bool CanDelete
        {
            get { return (bool)GetValue(CanDeleteProperty); }
            set { SetValue(CanDeleteProperty, value); }
        }
    }
}
