using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Data.Net
{
    /// <summary>
    /// Loads different data types from external source
    /// </summary>
    /// <typeparam name="TParam">User-defined type (enumeration) of input parameters</typeparam>
    /// <typeparam name="TData">User-defined type (enumeration) of data</typeparam>
    public class DataProvider<TParam, TData>
        where TData : struct
        where TParam : struct
    {
        /// <summary>
        /// Initializes a new instance of the DataProvider class, specifying a data source to load data from 
        /// </summary>
        /// <param name="dataSource">Data source to load data from</param>
        public DataProvider(IDataSource<TParam, TData> dataSource)
        {
            _dataSource = dataSource;
        }

        /// <summary>
        /// Loads requested data from associated IDataSource
        /// </summary>
        /// <param name="inputParameters">Input parameters for IDataSource</param>
        /// <param name="dataToLoad">List of types of data that should be loaded</param>
        /// <returns>Requested data, grouped by type</returns>
        public IDictionary<TData, object> LoadData(IDictionary<TParam, object> inputParameters, params TData[] dataToLoad)
        {
            return LoadData(inputParameters, dataToLoad, null);
        }

        /// <summary>
        /// Loads requested data from associated IDataSource and supplies an object containing data to be used by associated IDataSource 
        /// </summary>
        /// <param name="inputParameters">Input parameters for IDataSource</param>
        /// <param name="dataToLoad">List of types of data that should be loaded</param>
        /// <param name="callerContext">Object containing data to be used by associated IDataSource</param>
        /// <returns>Requested data, grouped by type</returns>
        public IDictionary<TData, object> LoadData(IDictionary<TParam, object> inputParameters, TData[] dataToLoad, object callerContext)
        {
            var startTime = DateTime.Now;
            Debug.WriteLine("Loading {0}", String.Join(", ", dataToLoad));

            var parameters = new Dictionary<TParam, object>(inputParameters);
            var data = new Dictionary<TData, object>();

            foreach (var dataType in dataToLoad)
            {
                var type = dataType;
                ThreadPool.QueueUserWorkItem(state => LoadData(type, parameters, data, state), callerContext);
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

        private TParam[] GetInputParameters(TData dataType)
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

        private TParam[] GetOptionalParameters(TData dataType)
        {
            var type = dataType.GetType();
            var memberInfo = type.GetMember(dataType.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(OptionalInputParameter), false).Cast<OptionalInputParameter>();
            var result = new HashSet<TParam>();
            attributes.Select(attr => attr.Parameter)
                .Cast<TParam>()
                .ToList()
                .ForEach(p => result.Add(p));
            return result.ToArray();            
        }

        private void LoadData(TData data, IDictionary<TParam, object> parameters, IDictionary<TData, object> dataContainer, object callerContext)
        {
            var startTime = DateTime.Now;
            Debug.WriteLine("Will load {0}", data);

            var inputParameters = GetInputParameters(data);
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

            foreach (var paramType in GetOptionalParameters(data))
            {
                if (parameterValues.ContainsKey(paramType))
                    continue;

                object paramVal;
                if (parameters.TryGetValue(paramType, out paramVal))
                {
                    Debug.WriteLine("[{0}] Optional parameter {1} is supplied", data, paramType);
                    parameterValues.Add(paramType, paramVal);
                }
            }

            object dataValue = null;

            try
            {
                dataValue = _dataSource.LoadData(data, parameterValues, new LoadingContext<TParam>(parameters) {State = callerContext});
            }
            catch (Exception ex)
            {
                dataValue = ex;
            }
            finally
            {
                lock (dataContainer)
                {
                    dataContainer.Add(data, dataValue);
                    Monitor.Pulse(dataContainer);
                }
            }

            Debug.WriteLine("[{0}] Loading finished in {1}", data, DateTime.Now - startTime);
        }
        #endregion
    }
}
