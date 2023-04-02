public class ShoppingViewModel
{
    // Filter Options
    public string? County { get; set; }
    public string? District { get; set; }
    public string? Category { get; set; }
    public float? Min { get; set; }
    public float? Max { get; set; }
    
    // Show Products
    public List<Product> Products { get; set; }
    public List<Store> Stores { get; set; }
    public Dictionary<string, List<string>> DistrictCounties { get; set; }
}
