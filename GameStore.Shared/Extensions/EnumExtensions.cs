using System.ComponentModel;
using System.Reflection;
using GameStore.Shared.Exceptions;

namespace GameStore.Shared.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();

        return attribute?.Description ?? value.ToString();
    }

    public static T ToEnum<T>(this string value)
        where T : struct, Enum
    {
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();

            if (attribute != null && string.Equals(attribute.Description, value, StringComparison.OrdinalIgnoreCase))
            {
                return (T)field.GetValue(null)!;
            }

            if (string.Equals(field.Name, value, StringComparison.OrdinalIgnoreCase))
            {
                return (T)field.GetValue(null)!;
            }
        }

        return Enum.TryParse<T>(value, true, out var result)
            ? result
            : throw new NotFoundException($"The value '{value}' was not found in the {typeof(T).Name}");
    }
}
