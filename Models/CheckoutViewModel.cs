public class CheckoutViewModel
{
    public List<CartItemViewModel> Items { get; set; }
    public float Total { get; set; }
    public float Subtotal { get; set; }
    public float Shipping { get; set; }
    public float Discount { get; set; }
    public string? DiscountCode { get; set; }
}

