using System.ComponentModel;

namespace GameStore.Shared.Enums.FilterOptionsEnums;

public enum PublichDateOptions
{
    [Description("Last 24 hours")]
    Last24Hours,

    [Description("Last week")]
    LastWeek,

    [Description("Last month")]
    LastMonth,

    [Description("Last year")]
    LastYear,

    [Description("All time")]
    AllTime,
}
