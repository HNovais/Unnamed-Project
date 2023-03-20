public class ShoppingCartViewModel
{
    public List<CartItemViewModel> CartItems { get; set; }
    public float TotalPrice { get; set; }
}

public class CartItemViewModel
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public float Price { get; set; }
    public int Quantity { get; set; }
    public float Subtotal { get { return Price * Quantity; } }
}
