using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace System.Windows.Transitions
{
    [TemplatePart(Name = PART_FontPresenter, Type = typeof(ContentControl))]
    [TemplatePart(Name = PART_BackPresenter, Type = typeof(ContentControl))]
    public class TransitionerBox : ContentControl, ITransitionContainer
    {
        public const string PART_FontPresenter = nameof(PART_FontPresenter);
        public const string PART_BackPresenter = nameof(PART_BackPresenter);
        public static readonly DependencyProperty TransitionWipeProperty =
            DependencyProperty.Register(nameof(TransitionWipe), typeof(ITransitionWipe), typeof(TransitionerBox), new PropertyMetadata(null));
        public static readonly DependencyProperty TransitionWipeSelectorProperty =
            DependencyProperty.Register(nameof(TransitionWipeSelector), typeof(ITransitionWipeSelector), typeof(TransitionerBox), new PropertyMetadata(null));
        public static readonly DependencyProperty TransitionOriginProperty =
            DependencyProperty.Register(nameof(TransitionOrigin), typeof(Point?), typeof(TransitionerBox), new PropertyMetadata(null));
        public static readonly DependencyPropertyKey OldContentPropertyKey =
           DependencyProperty.RegisterReadOnly(nameof(OldContent), typeof(object), typeof(TransitionerBox), new PropertyMetadata(null));
        public static readonly DependencyProperty OldContentProperty = OldContentPropertyKey.DependencyProperty;
        static TransitionerBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitionerBox), new FrameworkPropertyMetadata(typeof(TransitionerBox)));
        }

        private ContentControl _fontPresenter;
        private ContentControl _backPresenter;
        private DispatcherOperation _activeFrameOperation;
        public ITransitionWipe TransitionWipe
        {
            get { return (ITransitionWipe)GetValue(TransitionWipeProperty); }
            set { SetValue(TransitionWipeProperty, value); }
        }
        public ITransitionWipeSelector TransitionWipeSelector
        {
            get { return (ITransitionWipeSelector)GetValue(TransitionWipeSelectorProperty); }
            set { SetValue(TransitionWipeSelectorProperty, value); }
        }
        public Point? TransitionOrigin
        {
            get { return (Point?)GetValue(TransitionOriginProperty); }
            set { SetValue(TransitionOriginProperty, value); }
        }
        public object OldContent
        {
            get { return (object)GetValue(OldContentProperty); }
            private set { SetValue(OldContentPropertyKey, value); }
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _fontPresenter = this.Template.FindName(PART_FontPresenter, this) as ContentControl;
            _backPresenter = this.Template.FindName(PART_BackPresenter, this) as ContentControl;
        }
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            if (this.IsLoaded == false) return;
            TransitionChanged?.Invoke(this, null);
            if (oldContent != null)
                OldContent = oldContent;
            if (_activeFrameOperation == null)
                _activeFrameOperation = this.Dispatcher.BeginInvoke(new Action(OnContenChanged));
        }
        private void OnContenChanged()
        {
            this.ActivateFrame();
            _activeFrameOperation = null;
        }
        private void ActivateFrame()
        {
            var oldPresenter = _backPresenter;
            var newPresenter = _fontPresenter;
            if (!IsLoaded) return;
            if (oldPresenter != null && newPresenter != null)
            {
                ITransitionWipe wipe = TransitionWipe;
                if (wipe == null)
                {
                    wipe = this.TransitionWipeSelector.ProviderTransitionWipeFrom(oldPresenter, newPresenter, this);
                }
                if (wipe != null)
                {
                    var origin = TransitionOrigin ?? new Point();
                    wipe.Wipe(oldPresenter, newPresenter, origin, this);
                }
                else
                {
                    DoStack(newPresenter, oldPresenter);
                    if (oldPresenter != null) 
                        oldPresenter.Visibility = Visibility.Hidden;
                }
            }
            else if (oldPresenter != null || newPresenter != null)
            {
                DoStack(oldPresenter ?? newPresenter);
                if (oldPresenter != null) 
                    oldPresenter.Visibility = Visibility.Hidden;
            }
        }
        void ITransitionContainer.SetZIndexOrderBy(params FrameworkElement[] highestToLowest)
        {
            DoStack(highestToLowest);
        }
        void ITransitionContainer.OnCompletedTransition()
        {
            OldContent = null;
        }
        private static void DoStack(params FrameworkElement[] highestToLowest)
        {
            if (highestToLowest == null) return;

            var pos = highestToLowest.Length;
            foreach (var slide in highestToLowest.Where(s => s != null))
            {
                Panel.SetZIndex(slide, pos--);
            }
        }
        public int Indexof(FrameworkElement presenter)
        {
            return 0;
        }
        public object ItemFromContainer(FrameworkElement presenter)
        {
            return (presenter as ContentPresenter).Content;
        }
        public event EventHandler TransitionChanged;
    }
}
