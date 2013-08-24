using System.Collections.Generic;

namespace Data.Net
{
    public interface IDataSource<TParam, TData> 
        where TParam : struct
        where TData : struct
    {
        object LoadData(TData dataType, IDictionary<TParam, object> parameterValues, ILoadingContext<TParam> context);
    }
}
