public static class Utilities
{
    public static TEnum ToEnum<TEnum>(int value) where TEnum : struct, Enum
    {
        var result = Unsafe.As<int, TEnum>(ref value);
        if (!Enum.IsDefined(result)) { throw new($"Value {value} is not defined in enum {typeof(TEnum)}"); }
        return result;
    }

    public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicates<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keyProjection,
        Func<TSource, TValue> valueProjection
    ) where TKey : notnull =>
        source.Select(item => new { Key = keyProjection(item), Value = valueProjection(item) })
            .DistinctBy(pair => pair.Key)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
}
