namespace Data.Net
{
    /// <summary>
    /// Provides access to the context of data-loading process
    /// </summary>
    /// <typeparam name="TParam">User-defined parameter type</typeparam>
    public interface ILoadingContext<in TParam> where TParam : struct
    {
        /// <summary>
        /// Add runtime (loaded from external source) parameter value to this data-loading process
        /// </summary>
        /// <param name="parameterType">Type of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        void ProvideParameter(TParam parameterType, object value);

        /// <summary>
        /// User-defined state object, passed by DataProvider.LoadData() method
        /// </summary>
        object State { get; }
    }
}
