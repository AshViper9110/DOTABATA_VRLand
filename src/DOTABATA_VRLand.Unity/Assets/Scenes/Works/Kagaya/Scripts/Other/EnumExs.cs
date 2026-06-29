using System;

/// <summary>
/// intとstringからEnumに変換する
/// </summary>
public static class EnumExs {
    /// <summary>
    /// Int -> Enum
    /// </summary>
    public static T ParseFromInt<T>(int num) where T : Enum {
        return (T)Enum.ToObject(typeof(T), num);
    }
    /// <summary>
    /// Int -> Enum
    /// </summary>
    public static bool TryParseFromInt<T>(int num, out T resultEnum) where T : Enum {
        try {
            resultEnum = (T)Enum.ToObject(typeof(T), num);
            return true;
        }
        catch {
            resultEnum = default(T);
            return false;
        }
    }

    /// <summary>
    /// String -> Enum
    /// </summary>
    public static T ParseFromString<T>(string enumName, bool ignoreCase) where T : Enum{
        return (T)Enum.Parse(typeof(T), enumName, ignoreCase);
    }
    /// <summary>
    /// String -> Enum
    /// </summary>
    public static bool TryParseFromString<T>(string enumName, bool ignoreCase, out T resultEnum) where T : Enum {
        try {
            resultEnum = (T)Enum.Parse(typeof(T), enumName, ignoreCase);
            return true;
        }
        catch {
            resultEnum = default(T);
            return false;
        }
    }
}