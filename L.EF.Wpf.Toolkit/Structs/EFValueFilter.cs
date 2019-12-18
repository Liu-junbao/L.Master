using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public struct EFValueFilter
    {
        public EFValueFilter(EFComparison comparison, object comparisonValue, EFOperation operation)
        {
            Comparison = comparison;
            ComparisonValue = comparisonValue;
            Operation = operation;
        }
        public EFComparison Comparison { get; }
        public object ComparisonValue { get; }
        public EFOperation Operation { get; }
    }
}
