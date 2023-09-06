using System.Numerics;

namespace Tetris;

public static class LinqExtensions
{
    public static TSource Median<TSource>(this IEnumerable<TSource> source) where TSource : struct, INumber<TSource>
    {
        return Median<TSource, TSource>(source);
    }

    public static TResult Median<TSource, TResult>(this IEnumerable<TSource> source)
        where TSource : struct, INumber<TSource>
        where TResult : struct, INumber<TResult>
    {
        TSource[] array = source.ToArray();
        int length = array.Length;
        if (length == 0)
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }
        Array.Sort(array);
        int index = length / 2;
        TResult value = TResult.CreateChecked(array[index]);
        if (length % 2 == 1)
        {
            return value;
        }
        TResult sum = value + TResult.CreateChecked(array[index - 1]);
        return sum / TResult.CreateChecked(2);
    }
}
