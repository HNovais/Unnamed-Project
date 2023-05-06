using Microsoft.AspNetCore.Mvc.ModelBinding;

public class AddProductViewModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public float Price { get; set; }
    public List<ProdQuantity> Quantities { get; set; }
    public string Store { get; set; }
    public List<string> Categories { get; set; }
    public List<FeatureViewModel> Features { get; set; }
}

public class ProdQuantity
{
    public string Type { get; set; }
    public int Quantity { get; set; }
}

public class FeatureViewModel
{
    [BindNever]
    public string? Name { get; set; }
    [BindNever]
    public List<string>? Values { get; set; }
    [BindNever]
    public string? EnteredValue { get; set; }
    public int Index { get; set; }
}

/*
public class CategoryFeatures
{
    public ClothesFeatures Clothes { get; set; }
    public ShoesFeatures Shoes { get; set; }
}

public class ClothesFeatures
{
    public string? Gender { get; set; } // Men, Woman, Unissex
    public string? Age { get; set; } // Adult, Child, Baby
    public List<string>? Colour { get; set; }
    public string? Brand { get; set; }
}

public class ShoesFeatures
{
    public string? Gender { get; set; } // Men, Woman, Unissex
    public string? Age { get; set; } // Adult, Child, Baby
    public List<string>? Colour { get; set; }
    public string? Brand { get; set; }
}
*/