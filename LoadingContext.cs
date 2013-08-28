using System;
using System.Collections.Generic;
using System.Threading;

namespace Data.Net
{
    public class LoadingContext<TParam> : ILoadingContext<TParam> where TParam : struct
    {
        public LoadingContext(IDictionary<TParam, object> parameters)
        {
            _parameters = parameters;
        }

        public object State { get; set; }

        public void ProvideParameter(TParam parameterType, object value)
        {
            lock (_parameters)
            {
                if (!_parameters.ContainsKey(parameterType))
                {
                    _parameters.Add(parameterType, value);
                    Monitor.PulseAll(_parameters);
                }
            }
        }

        private readonly IDictionary<TParam, object> _parameters;
    }
}
