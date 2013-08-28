namespace Data.Net
{
    public interface ILoadingContext<in TParam> where TParam : struct
    {
        void ProvideParameter(TParam parameterType, object value);

        object State { get; }
    }
}
