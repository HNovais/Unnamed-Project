﻿using System.ComponentModel.DataAnnotations;

public class ProductPageViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public float Price { get; set; }
    public string Seller { get; set; }
    public List<string> Images { get; set; }
    public string Icon { get; set; }
    public Store Store { get; set; }
    public decimal StoreRating { get; set; }
}
