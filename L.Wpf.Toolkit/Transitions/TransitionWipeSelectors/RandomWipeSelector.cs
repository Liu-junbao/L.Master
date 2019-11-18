using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Transitions
{
    [ContentProperty(nameof(Wipes))]
    public class RandomWipeSelector : ITransitionWipeSelector
    {
        private List<ITransitionWipe> _wipes;
        private Random _random;
        public RandomWipeSelector()
        {
            _wipes = new List<ITransitionWipe>()
            {
                new CircleWipe(),
                new FadeWipe(),
                new SlideOutWipe(),
                new SlideWipe(),
            };
            _wipes.AddRange(CustomWipe.GetLocalCustomWipes());
            Wipes = new List<ITransitionWipe>();
            _random = new Random();
        }
        public List<ITransitionWipe> Wipes { get; }
        public ITransitionWipe ProviderTransitionWipeFrom(FrameworkElement oldPresenter, FrameworkElement newPresenter, ITransitionContainer container)
        {
            if (Wipes.Count > 0)
                return Wipes[_random.Next(0, Wipes.Count - 1)];
            else
                return _wipes[_random.Next(0, _wipes.Count - 1)];
        }
    }
}
