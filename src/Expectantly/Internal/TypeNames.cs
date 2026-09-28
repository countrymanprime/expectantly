using System.Text;

namespace Expectantly.Internal;

/// <summary>Writes type names the way C# source spells them: <c>int</c>, <c>List&lt;string&gt;</c>, <c>int?</c>.</summary>
internal static class TypeNames
{
    private static readonly Dictionary<Type, string> Aliases = new()
    {
        [typeof(bool)] = "bool",
        [typeof(byte)] = "byte",
        [typeof(sbyte)] = "sbyte",
        [typeof(char)] = "char",
        [typeof(short)] = "short",
        [typeof(ushort)] = "ushort",
        [typeof(int)] = "int",
        [typeof(uint)] = "uint",
        [typeof(long)] = "long",
        [typeof(ulong)] = "ulong",
        [typeof(float)] = "float",
        [typeof(double)] = "double",
        [typeof(decimal)] = "decimal",
        [typeof(string)] = "string",
        [typeof(object)] = "object",
        [typeof(void)] = "void",
    };

    public static string Of(Type type)
    {
        if (Aliases.TryGetValue(type, out var alias))
        {
            return alias;
        }

        if (type.IsArray)
        {
            return Of(type.GetElementType()!) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
        }

        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
        {
            return Of(underlying) + "?";
        }

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var name = type.Name;
        var tick = name.IndexOf('`');
        var builder = new StringBuilder(tick < 0 ? name : name.Substring(0, tick)).Append('<');
        var arguments = type.GetGenericArguments();
        for (var i = 0; i < arguments.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(Of(arguments[i]));
        }

        return builder.Append('>').ToString();
    }

    /// <summary>Returns the type name with an indefinite article: <c>a string</c>, <c>an int</c>.</summary>
    public static string WithArticle(Type type)
    {
        var name = Of(type);
        return ("aeiouAEIOU".IndexOf(name[0]) >= 0 ? "an " : "a ") + name;
    }
}
