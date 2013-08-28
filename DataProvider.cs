using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Data.Net
{
    public class DataProvider<TParam, TData>
        where TData : struct
        where TParam : struct
    {
        public DataProvider(IDataSource<TParam, TData> dataSource)
        {
            _dataSource = dataSource;
        }

        public IDictionary<TData, object> LoadData(IDictionary<TParam, object> inputParameters, params TData[] dataToLoad)
        {
            var startTime = DateTime.Now;
            Debug.WriteLine("Loading {0}", String.Join(", ", dataToLoad));

            var parameters = new Dictionary<TParam, object>(inputParameters);
            var data = new Dictionary<TData, object>();

            foreach (var dataType in dataToLoad)
            {
                var type = dataType;
                ThreadPool.QueueUserWorkItem(state => LoadData(type, parameters, data));
            }

            lock (data)
            {
                while (data.Count < dataToLoad.Length)
                    Monitor.Wait(data);
            }

            Debug.WriteLine("Loading finished in {0}", DateTime.Now - startTime);
            return data;
        }

        #region private
        private readonly IDataSource<TParam, TData> _dataSource;

        private TParam[] GetInputParameters<TParam, TData>(TData dataType)
            where TParam : struct
            where TData : struct
        {
            var type = dataType.GetType();
            var memberInfo = type.GetMember(dataType.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(InputParametersAttribute), false).Cast<InputParametersAttribute>();
            var result = new HashSet<TParam>();
            attributes.SelectMany(attr => attr.Parameters)
                .Cast<TParam>()
                .ToList()
                .ForEach(p => result.Add(p));
            return result.ToArray();
        }

        private void LoadData(TData data, IDictionary<TParam, object> parameters, IDictionary<TData, object> dataContainer)
        {
            var startTime = DateTime.Now;
            Debug.WriteLine("Will load {0}", data);

            var inputParameters = GetInputParameters<TParam, TData>(data);
            var parameterValues = new Dictionary<TParam, object>();

            lock (parameters)
            {
                bool allParametersExist;

                do
                {
                    allParametersExist = true;
                    parameterValues.Clear();

                    foreach (var paramType in inputParameters)
                    {
                        if (parameterValues.ContainsKey(paramType))
                            continue;

                        object paramVal;
                        if (parameters.TryGetValue(paramType, out paramVal))
                        {
                            parameterValues.Add(paramType, paramVal);
                        }
                        else
                        {
                            Debug.WriteLine("[{0}] Required parameter {1} does not exist", data, paramType);
                            allParametersExist = false;
                        }
                    }

                    if (!allParametersExist)
                    {
                        Monitor.Wait(parameters);
                    }
                } while (!allParametersExist);
            }

            Debug.WriteLine("[{0}] All required parameters do exist", data);

            try
            {
                object dataValue = _dataSource.LoadData(data, parameterValues, new LoadingContext<TParam>(parameters));

                lock (dataContainer)
                {
                    dataContainer.Add(data, dataValue);
                    Monitor.Pulse(dataContainer);
                }
            }
            catch (Exception ex)
            {
                
            }

            Debug.WriteLine("[{0}] Loading finished in {1}", data, DateTime.Now - startTime);
        }
        #endregion
    }
}
