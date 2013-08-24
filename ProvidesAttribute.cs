using System;

namespace Data.Net
{
    public class ProvidesAttribute : Attribute
    {
        public ProvidesAttribute(params int[] parameters)
        {
            Parameters = parameters;
        }

        public readonly int[] Parameters;
    }
}
