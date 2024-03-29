﻿using System.ComponentModel.DataAnnotations;

public class StoreProfileViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string County { get; set; }
    public string District { get; set; }
    public string Icon { get; set; }
    public int ReviewCount { get; set; }
    public double AverageRating { get; set; }
    public List<Review> Reviews { get; set; }
    public Review UserReview { get; set; }
    public List<Product> Products { get; set; }
}


