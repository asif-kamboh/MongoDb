using ScientificBit.MongoDb.Enums;

namespace ScientificBit.MongoDb.Utils;

internal static class PropertyNamingStylesHelper
{
    public static PropertyNamingStyles CurrentNamingStyle = PropertyNamingStyles.CamelCase;

    public static string ToCurrentNamingStyle(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) throw new ArgumentException("Field name can't be empty");

        return CurrentNamingStyle == PropertyNamingStyles.TitleCase
            ? $"{fieldName[0].ToString().ToUpperInvariant()}{fieldName[1..]}"
            : $"{fieldName[0].ToString().ToLowerInvariant()}{fieldName[1..]}";
    }
}