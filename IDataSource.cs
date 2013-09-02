using System.Collections.Generic;

namespace Data.Net
{
    /// <summary>
    /// A delegate that implements loading of different data types
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public interface IDataSource<TParam, in TData> 
        where TParam : struct
        where TData : struct
    {
        /// <summary>
        /// Loads specified data type from this source
        /// </summary>
        /// <param name="dataType">Type of data</param>
        /// <param name="parameterValues">Values of input parameters</param>
        /// <param name="context">Operation context</param>
        /// <returns>Requested data</returns>
        object LoadData(TData dataType, IDictionary<TParam, object> parameterValues, ILoadingContext<TParam> context);
    }
}
