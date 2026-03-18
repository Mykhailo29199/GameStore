using System.ComponentModel;
using System.Reflection;

namespace GameStore.Shared.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute == null ? value.ToString() : attribute.Description;
    }

    // string → enum via Description
    public static T ToEnumFromDescription<T>(this string description)
        where T : Enum
    {
        foreach (var field in typeof(T).GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
            {
                if (attr.Description.Equals(description, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)field.GetValue(null)!;
                }
            }
            else if (field.Name.Equals(description, StringComparison.OrdinalIgnoreCase))
            {
                return (T)field.GetValue(null)!;
            }
        }

        throw new ArgumentException($"Cannot map '{description}' to enum {typeof(T).Name}");
    }
}
