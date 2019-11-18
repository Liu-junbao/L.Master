using System.Windows.Media.Animation;

namespace System.Windows.Transitions
{
    public abstract class TransitionWipeBase : ITransitionWipe
    {
        private Storyboard _oldPresenterStoryboard;
        private Storyboard _newPresenterStoryboard;
        public ZIndexOrder OriginZIndexOrder { get; set; }
        public TransitionOrder TransitionOrder { get; set; }
        protected virtual Storyboard CreateStoryboardForOldPresenter(ITransitionContainer container) => null;
        protected virtual Storyboard CreateStoryboardForNewPresenter(ITransitionContainer container) => null;
        protected virtual Storyboard PrepareStoryboardForOldPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            if (_oldPresenterStoryboard == null)
                _oldPresenterStoryboard = CreateStoryboardForOldPresenter(container);
            return _oldPresenterStoryboard;
        }
        protected virtual Storyboard PrepareStoryboardForNewPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            if (_newPresenterStoryboard == null)
                _newPresenterStoryboard = CreateStoryboardForNewPresenter(container);
            return _newPresenterStoryboard;
        }
        public virtual void Wipe(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            switch (OriginZIndexOrder)
            {
                case ZIndexOrder.NewPresenterAbove:
                    container.SetZIndexOrderBy(newPresenter, oldPresenter);
                    break;
                case ZIndexOrder.OldPresenterAbove:
                    container.SetZIndexOrderBy(oldPresenter, newPresenter);
                    break;
                default:
                    break;
            }
            if (oldPresenter == null && newPresenter == null)
            {
                OnCompletedTransition(null, null, null, null, origin, container);
                return;
            }
            var oldStoryboard = PrepareStoryboardForOldPresenter(oldPresenter, newPresenter, origin, container);
            var newStoryboard = PrepareStoryboardForNewPresenter(oldPresenter, newPresenter, origin, container);
            if (oldStoryboard == null && newStoryboard == null)
            {
                OnCompletedTransition(null, oldPresenter, null, newPresenter, origin, container);
                return;
            }
            oldPresenter.Visibility = Visibility.Visible;
            newPresenter.Visibility = Visibility.Visible;
            var order = this.TransitionOrder;
            switch (order)
            {
                case TransitionOrder.Both:
                    int complatedCount = 0, transitionCount = 0;
                    if (oldStoryboard != null && oldPresenter != null)
                    {
                        transitionCount++;
                        this.BeginTransition(oldStoryboard, oldPresenter, container, (s, e) =>
                        {
                            complatedCount++;
                            OnEndedTransitionOfOldPresenter(oldPresenter, newPresenter, origin, container);
                            if (complatedCount >= transitionCount)
                            {
                                OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
                            }
                        });
                    }
                    if (newStoryboard != null && newPresenter != null)
                    {
                        transitionCount++;
                        this.BeginTransition(newStoryboard, newPresenter, container, (s, e) =>
                        {
                            complatedCount++;
                            OnEndedTransitionOfNewPresenter(oldPresenter, newPresenter, origin, container);
                            if (complatedCount >= transitionCount)
                            {
                                OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
                            }
                        });
                    }
                    break;
                case TransitionOrder.OldFirst:
                    if (oldStoryboard != null && oldPresenter != null)
                    {
                        this.BeginTransition(oldStoryboard, oldPresenter, container, (s, e) =>
                        {
                            OnEndedTransitionOfOldPresenter(oldPresenter, newPresenter, origin, container);
                            if (newStoryboard != null && newPresenter != null)
                            {
                                this.BeginTransition(newStoryboard, newPresenter, container, (s1, e1) =>
                                {
                                    OnEndedTransitionOfNewPresenter(oldPresenter, newPresenter, origin, container);
                                    OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
                                });
                            }
                            else
                            {
                                OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
                            }

                        });
                    }
                    else if (newStoryboard != null && newPresenter != null)
                    {
                        this.BeginTransition(newStoryboard, newPresenter, container, (s, e) =>
                        {
                            OnEndedTransitionOfNewPresenter(oldPresenter, newPresenter, origin, container);
                            OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
                        });
                    }
                    else
                    {
                        OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
                    }
                    break;
                case TransitionOrder.NewFirst:
                    if (newStoryboard != null && newPresenter != null)
                    {
                        this.BeginTransition(newStoryboard, newPresenter, container, (s, e) =>
                    {
                        //s.Seek(e, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
                        s.Remove(e);
                        OnEndedTransitionOfNewPresenter(oldPresenter, newPresenter, origin, container);
                        if (oldStoryboard != null && oldPresenter != null)
                        {
                            this.BeginTransition(oldStoryboard, oldPresenter, container, (s1, e1) =>
                            {
                                OnEndedTransitionOfOldPresenter(oldPresenter, newPresenter, origin, container);
                                OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
                            });
                        }
                        else
                        {
                            OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
                        }
                    });
                    }
                    else if (oldStoryboard != null && oldPresenter != null)
                    {
                        this.BeginTransition(oldStoryboard, oldPresenter, container, (s, e) =>
                        {
                            OnEndedTransitionOfOldPresenter(oldPresenter, newPresenter, origin, container);
                            OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
                        });
                    }
                    else
                    {
                        OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
                    }
                    break;
                default:
                    break;
            }
        }
        protected virtual void OnEndedTransitionOfOldPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container) { }
        protected virtual void OnEndedTransitionOfNewPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container) { }
        protected virtual void OnCompletedTransition(Storyboard oldStoryboard, FrameworkElement oldPresenter, Storyboard newStoryboard, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            if (oldPresenter != null)
                oldPresenter.Visibility = Visibility.Hidden;
            if (newPresenter != null)
                newPresenter.Visibility = Visibility.Visible;
            container.SetZIndexOrderBy(newPresenter, oldPresenter);
            container.OnCompletedTransition();
            oldStoryboard?.Remove(oldPresenter);
            newStoryboard?.Remove(newPresenter);
        }
        private void BeginTransition(Storyboard storyboard, FrameworkElement element, ITransitionContainer container, Action<Storyboard, FrameworkElement> onEndAction)
        {
            if (storyboard == null || element == null) throw new Exception();
            EventHandler endHandler = null;
            endHandler = (s, e) =>
            {
                onEndAction.Invoke(storyboard, element);
                storyboard.Completed -= endHandler;
                container.TransitionChanged -= endHandler;
            };
            storyboard.Completed += endHandler;
            container.TransitionChanged += endHandler;
            storyboard.Begin(element, true);
        }
    }

    public enum ZIndexOrder
    {
        NewPresenterAbove,
        OldPresenterAbove,
    }
    public enum TransitionOrder
    {
        /// <summary>
        /// 同时执行过渡动画
        /// </summary>
        Both,
        /// <summary>
        /// 先执行隐藏过渡动画
        /// </summary>
        OldFirst,
        /// <summary>
        /// 先执行呈现过渡动画
        /// </summary>
        NewFirst,
    }
}
