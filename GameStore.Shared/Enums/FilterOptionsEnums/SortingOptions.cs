using System.ComponentModel;

namespace GameStore.Shared.Enums.FilterOptionsEnums;

public enum SortingOptions
{
    [Description("Most popular")]
    MostPopular,

    [Description("Most commented")]
    MostCommented,

    [Description("Price ASC")]
    PriceAsc,

    [Description("Price DESC")]
    PriceDesc,

    [Description("New")]
    New,
}
