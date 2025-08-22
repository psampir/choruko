namespace ChorukoAPI.Mappers;

public interface IMapper<TSource, TResult> where TSource : class
{
    Task<TResult> Map(TSource source);
}
