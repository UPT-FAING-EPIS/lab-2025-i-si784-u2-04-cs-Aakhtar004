namespace EcommerceApp.Api.Services;

/// <summary>
/// Interface for discount services that can be applied to cart totals
/// </summary>
public interface IDiscountService
{
    /// <summary>
    /// Calculates the discount amount for a given total
    /// </summary>
    /// <param name="total">The original total amount</param>
    /// <returns>The discount amount to be subtracted from the total</returns>
    double CalculateDiscount(double total);
}
