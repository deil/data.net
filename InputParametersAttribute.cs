using System;

namespace Data.Net
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class InputParametersAttribute : Attribute
    {
        public InputParametersAttribute(params int[] parameters)
        {
            Parameters = parameters;
        }

        public readonly int[] Parameters;
    }
}
