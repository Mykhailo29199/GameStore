using System.ComponentModel;

namespace GameStore.Shared.Enums;

public enum BanDurationOption
{
    [Description("1 hour")]
    OneHour,

    [Description("1 day")]
    OneDay,

    [Description("1 week")]
    OneWeek,

    [Description("1 month")]
    OneMonth,

    Permanent,
}
