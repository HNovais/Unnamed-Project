﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using unnamed.Models;
using static System.Net.Mime.MediaTypeNames;

public class ProductController : Controller
{
    [HttpGet]
    [Authorize(Roles = "User, Store")]
    public ActionResult ProductPage(int productID, string storeName)
    {
        if ((User.IsInRole("Store") && User.Identity.Name.ToLower() == storeName.ToLower()) || User.IsInRole("User"))
        {
            using (var db = new MyDbContext())
            {
                var product = db.Product.FirstOrDefault(p => p.Id == productID);

                var store = db.Store.FirstOrDefault(s => s.Id == product.Store);

                var rating = GetStoreRating(store.Id);

                if (product != null && store != null)
                {
                    var model = new ProductPageViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Category = product.Category,
                        Price = product.Price,
                        Seller = db.Store.FirstOrDefault(s => s.Id == product.Store).Name,
                        Images = product.Images.Split(",").ToList(),
                        Icon = product.Icon,
                        Store = store,
                        StoreRating = rating
                    };

                    return View(model);
                }

                return RedirectToAction("Index", "Home");
            }
        }

        return RedirectToAction("Index", "Home");
    }

    private decimal GetStoreRating(int storeID)
    {
        using (var context = new MyDbContext())
        {
            var reviews = context.Review.Where(r => r.Store == storeID).ToList();

            if (reviews.Count == 0)
            {
                return 0;
            }

            decimal totalRating = 0;
            foreach (var review in reviews)
            {
                totalRating += review.Rating;
            }

            decimal averageRating = totalRating / reviews.Count;
            return Math.Round(averageRating, 2);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Store")]
    public ActionResult DeleteProduct(int productId, string storeName)
    {
        using (var db = new MyDbContext())
        {
            var product = db.Product.FirstOrDefault(p => p.Id == productId && db.Store.Any(s => s.Name == storeName && s.Id == p.Store));

            if (product != null)
            {
                db.Product.Remove(product);
                db.SaveChanges();

                return RedirectToAction("StoreProfile", new { storeUsername = storeName });
            }         
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize(Roles = "Store")]
    public ActionResult AddProduct(string storeUsername)
    {
        using (var db = new MyDbContext())
        {
            var categories = db.Category.Select(p => p.Name).ToList();

            var model = new AddProductViewModel
            {
                Store = storeUsername,
                Categories = categories
            };

            return View(model);
        }
    }

    [HttpPost]
    [Authorize(Roles="Store")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(AddProductViewModel model, IFormFile Icon, List<IFormFile> Images) // Feature Values DB
    {
        string icon;
        string images;

        if (Icon != null && Icon.Length > 0 && Images != null && Images.Count > 0)
        {
            icon = await uploadIcon(Icon, model.Store);
            images = await uploadImages(Images, model.Store);
        } else
            return View(model);

        if (ModelState.IsValid)
        {
            using (var db = new MyDbContext())
            {
                var store = db.Store.FirstOrDefault(s => s.Username == model.Store);

                if (store != null)
                {
                    var product = new Product // Add Product
                    {
                        Name = model.Name,
                        Description = model.Description,
                        Category = model.Category,
                        Price = model.Price,
                        Store = store.Id,
                        Icon = icon,
                        Images = images
                    };

                    db.Product.Add(product);
                    db.SaveChanges();

                    for (int i = 0; i < model.Quantities.Count(); i++) // Add Quantities
                    {
                        var productQuantity = new ProductQuantity
                        {
                            Product = product.Id,
                            Type = model.Quantities[i].Type,
                            Quantity = model.Quantities[i].Quantity
                        };

                        db.ProductQuantity.Add(productQuantity);
                        db.SaveChanges();
                    }

                    var id = db.Category.FirstOrDefault(c => c.Name == model.Category).Id;
                    var features = db.Feature.Where(f => f.Category == id).ToList();

                    foreach (var feature in model.Features) // Add Features
                    {
                        string featureName = features[feature.Index].Name;

                        if (feature.Values != null)
                        {
                            List<string> selectedValues = feature.Values.Where(v => !string.IsNullOrEmpty(v)).ToList();

                            var featureValue = new FeatureValue
                            {
                                Feature = features[feature.Index].Id,
                                Product = product.Id,
                                Value = string.Join(",", selectedValues)
                            };

                            db.FeatureValue.Add(featureValue);
                        }

                        else
                        {
                            string enteredValue = feature.EnteredValue;

                            var featureValue = new FeatureValue
                            {
                                Feature = features[feature.Index].Id,
                                Product = product.Id,
                                Value = string.Join(",", enteredValue)
                            };

                            db.FeatureValue.Add(featureValue);
                        }

                        db.SaveChanges();
                    }
                }
            }

            return RedirectToAction("StoreProfile", "Store", new { storeUsername = model.Store });
        }

        foreach (var modelState in ModelState.Values)
        {
            foreach (var error in modelState.Errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }

        return View(model);
    }

    private async Task<string> uploadIcon(IFormFile Icon, string store)
    {
        var fileName = Path.GetFileName(Icon.FileName);
        var storeDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", store);
        var storeFileName = $"{store}-{DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")}-{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(storeDirectory, storeFileName);

        if (!Directory.Exists(storeDirectory))
        {
            Directory.CreateDirectory(storeDirectory);
        }

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await Icon.CopyToAsync(fileStream);
        }

        return storeFileName;
    }

    private async Task<string> uploadImages(List<IFormFile> Images, string store)
    {
        var imagePaths = new List<string>();
        foreach (var image in Images)
        {
            var imageFileName = Path.GetFileName(image.FileName);
            var storeDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", store);
            var imageStoreFileName = $"{store}-{DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")}-{Guid.NewGuid()}{Path.GetExtension(imageFileName)}";
            var imageFilePath = Path.Combine(storeDirectory, imageStoreFileName);

            if (!Directory.Exists(storeDirectory))
            {
                Directory.CreateDirectory(storeDirectory);
            }

            using (var fileStream = new FileStream(imageFilePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            imagePaths.Add(imageStoreFileName);
        }

        var allImagePaths = string.Join(",", imagePaths);

        return allImagePaths;
    }

    [HttpGet]
    [Authorize(Roles = "Store")]
    public ActionResult EditProduct(int productId, string storeName)
    {
        using (var db = new MyDbContext())
        {
            var product = db.Product.FirstOrDefault(p => p.Id == productId && db.Store.Any(s => s.Name == storeName && s.Id == p.Store));

            if (product != null)
            {
                var model = new EditProductViewModel
                {
                    Id = productId,
                    Name = product.Name,
                    Description = product.Description,
                    Category = product.Category,
                    Price = product.Price,
                    Store = storeName
                };

                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Store")]
    [ValidateAntiForgeryToken]
    public IActionResult EditProduct(EditProductViewModel model, IFormFile Image, string Store, int Id)
    {
        if (ModelState.IsValid)
        {
            using (var db = new MyDbContext())
            {
                var product = db.Product.FirstOrDefault(p => p.Id == Id);

                if (product != null && User.Identity.Name.ToLower() == Store.ToLower())
                {
                    product.Name = model.Name;
                    product.Description = model.Description;
                    product.Category = model.Category;
                    product.Price = model.Price;

                    if (Image != null && Image.Length > 0)
                    {
                        var fileName = Path.GetFileName(Image.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            Image.CopyToAsync(fileStream);
                        }

                        product.Icon = fileName;
                        product.Images = fileName;
                    }

                    db.SaveChanges();

                    return RedirectToAction("ProductPage", new { productId = product.Id, storeName = Store });
                }
            }
        }

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    [ValidateAntiForgeryToken]
    public ActionResult AddToCart(int productID, string seller)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        using (var db = new MyDbContext())
        {
            var cart = db.Cart.FirstOrDefault(c => c.User == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    User = userId,
                    Time = DateTime.Now
                };

                db.Cart.Add(cart);

                db.SaveChanges();
            }

            else
            {
                cart.Time = DateTime.Now;
            }

            var product = db.Product.FirstOrDefault(p => p.Id == productID);

            if (product != null)
            {
                var cartProduct = db.CartProduct.FirstOrDefault(cp => cp.Cart == cart.Id && cp.Product == productID);

                if (cartProduct == null)
                {
                    cartProduct = new CartProduct
                    {
                        Cart = cart.Id,
                        Product = productID,
                        Quantity = 1
                    };

                    db.CartProduct.Add(cartProduct);
                }

                else
                {
                    cartProduct.Quantity += 1;
                }
            }

            db.SaveChanges();

            return RedirectToAction("ProductPage", new { productId = productID, storeName = seller });
        }
    }

    [HttpGet]
    public ActionResult GetFeatures(string category)
    {
        using (var database = new MyDbContext())
        {
            var id = database.Category.FirstOrDefault(c => c.Name == category)?.Id;

            if (id == null)
            {
                return Json(new { error = "Category not found" });
            }

            var features = database.Feature
                .Where(f => f.Category == id)
                .Select(f => f.Name)
                .ToList();

            return Json(features);
        }
    }

    [HttpGet]
    public ActionResult GetFeatureOptions(string feature)
    {
        using (var db = new MyDbContext())
        {
            var id = db.Feature.FirstOrDefault(f => f.Name == feature).Id;

            var options = db.FeatureOptions
                .Where(f => f.Feature == id)
                .Select(f => f.Value)
                .ToList();

            return Json(options);
        }
    }
}