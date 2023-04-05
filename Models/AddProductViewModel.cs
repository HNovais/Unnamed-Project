﻿public class AddProductViewModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public float Price { get; set; }
    public int Quantity { get; set; }
    public string Store { get; set; }
    public IFormFile Icon { get; set; }
    public List<IFormFile> Images { get; set; }
}