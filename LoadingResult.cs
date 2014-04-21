using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Data.Net
{
    /// <summary>
    /// Holds loaded data.
    /// </summary>
    public class LoadingResult<TData> where TData : struct
    {
        public LoadingResult()
        {
            _loadedData = new Dictionary<TData, object>();
            _exceptions = new List<Exception>();
        }

        /// <summary>
        /// Tells if all requested data was loaded successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets loaded data
        /// </summary>
        /// <param name="key">type of data</param>
        /// <returns>loaded value</returns>
        public object this[TData key]
        {
            get { return _loadedData[key]; }
        }

        /// <summary>
        /// Checks whether data with specified key has been loaded successfully
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsLoaded(TData key)
        {
            return _loadedData.ContainsKey(key) && this[key] != null;
        }

        /// <summary>
        /// All catched exceptions while loading data
        /// </summary>
        public Exception[] Exceptions
        {
            get { return _exceptions.ToArray(); }
        }

        internal void AddData(TData key, object value)
        {
            _loadedData.Add(key, value);
        }

        internal void AddException(Exception ex)
        {
            _exceptions.Add(ex);
        }

        internal int Count { get { return _loadedData.Count; } }

        private readonly IDictionary<TData, object> _loadedData;
        private readonly List<Exception> _exceptions;
    }
}
