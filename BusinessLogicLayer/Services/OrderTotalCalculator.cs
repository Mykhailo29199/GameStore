namespace BusinessLogicLayer.Services;

/// <summary>
/// Calculates order sums. A discount is a whole number of percent, for example 20 means 20%.
/// </summary>
public static class OrderTotalCalculator
{
    private const decimal Percent = 100m;

    public static decimal LineTotal(decimal unitPrice, int quantity, double? discountPercent)
    {
        var discount = Math.Clamp((decimal)(discountPercent ?? 0), 0m, Percent);

        return unitPrice * (1 - (discount / Percent)) * quantity;
    }

    public static decimal Total(IEnumerable<(decimal UnitPrice, int Quantity, double? DiscountPercent)> lines)
    {
        return lines.Sum(line => LineTotal(line.UnitPrice, line.Quantity, line.DiscountPercent));
    }
}
