public class ShoppingViewModel
{
    // Filter Options
    public string Store { get; set; } = "All";
    public string County { get; set; } = "All";
    public string District { get; set; } = "All";
    public string Category { get; set; } = "All";
    public float Min { get; set; } = 0;
    public float Max { get; set; } = 1000;

    // Show Products
    public List<Product> Products { get; set; }
    public List<Store> Stores { get; set; }
    public List<string> Categories { get; set; }
    public Dictionary<string, List<string>> DistrictCounties { get; set; }

    // Category Filter
    public Dictionary<string, List<string>>? CatFilter { get; set; }
}
