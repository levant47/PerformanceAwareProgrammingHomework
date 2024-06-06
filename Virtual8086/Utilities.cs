public static class Utilities
{
    public static TEnum ToEnum<TEnum>(int value) where TEnum : struct, Enum
    {
        var result = Unsafe.As<int, TEnum>(ref value);
        if (!Enum.IsDefined(result)) { throw new($"Value {value} is not defined in enum {typeof(TEnum)}"); }
        return result;
    }
}
