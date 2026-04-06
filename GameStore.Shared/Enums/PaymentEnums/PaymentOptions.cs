using System.ComponentModel;

namespace GameStore.Shared.Enums.PaymentEnums;

public enum PaymentOptions
{
    [Description("Visa")]
    Visa,

    [Description("IBox terminal")]
    IBoxTerminal,

    [Description("Bank")]
    Bank,
}
