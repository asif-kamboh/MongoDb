using System.ComponentModel;
using System.Text.Json;
using Newtonsoft.Json;

namespace ScientificBit.MongoDb.Utils;

public static class JsonHelper
{
    public static TVal? DeserializeObject<TVal>(object val)
    {
        var output = DeserializeObject(val, typeof(TVal));
        return (TVal?)output;
    }

    public static object? DeserializeObject(object? val, Type type)
    {
        if (val?.GetType() == type)
        {
            return val;
        }

        var json = GetJsonString(val);
        if (json is null) return null;

        try
        {
            return JsonConvert.DeserializeObject(json, type);
        }
        catch (NotSupportedException)
        {
            return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(json);
        }
    }

    private static string? GetJsonString(object? val)
    {
        if (val is null) return null;
        if (val is string s) return s;
        try
        {
            return ((JsonElement)val).GetRawText();
        }
        catch (InvalidCastException)
        {
            return val.ToString();
        }
    }
}