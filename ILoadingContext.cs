namespace Data.Net
{
    public interface ILoadingContext<TParam> where TParam : struct
    {
        void ProvideParameter(TParam parameterType, object value);
    }
}
