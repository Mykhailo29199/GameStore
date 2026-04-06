using System.ComponentModel;

namespace GameStore.Shared.Enums.FilterOptionsEnums;

public enum PaginationOptions
{
    [Description("10")]
    Ten = 10,

    [Description("20")]
    Twenty = 20,

    [Description("50")]
    Fifty = 50,

    [Description("100")]
    Hundred = 100,

    [Description("All")]
    All = int.MaxValue,
}