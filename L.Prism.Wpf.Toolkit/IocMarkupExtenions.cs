﻿using Prism.Ioc;
using System;
using System.Windows.Markup;

namespace Prism
{
    public class IocMarkupExtenions<TValue> : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var value = this.GetInstance<TValue>();
            if (value != null)
                OnInitialize(value);
            return value;
        }
        protected virtual void OnInitialize(TValue value) { }
    }
}
