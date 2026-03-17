using System.ComponentModel;

namespace GameStore.Shared.Enums;

public enum PaymentOptions
{
    [Description("Visa")]
    Visa,

    [Description("IBox terminal")]
    IBoxTerminal,

    [Description("Bank")]
    Bank,
}
