using System;

namespace Data.Net
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class OptionalInputParameter : Attribute
    {
        public OptionalInputParameter(int parameter)
        {
            Parameter = parameter;
        }

        public readonly int Parameter;
    }
}
