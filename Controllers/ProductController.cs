using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using unnamed.Models;

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

                if (product != null)
                {
                    var model = new ProductPageViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Category = product.Category,
                        Price = product.Price,
                        Seller = db.Store.FirstOrDefault(s => s.Id == product.Store).Name,
                        Images = product.Images
                    };

                    return View(model);
                }

                return RedirectToAction("Index", "Home");
            }
        }

        return RedirectToAction("Index", "Home");
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
        Console.WriteLine(storeUsername);
        var model = new AddProductViewModel
        {
            Store = storeUsername
        };

        return View(model);

    }

    [HttpPost]
    [Authorize(Roles = "Store")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(AddProductViewModel model, IFormFile Icon, List<IFormFile> Images) 
    {
        var selectedColours = Request.Form["Features.Shoes.Colour[]"].ToArray();
        var colourString = string.Join(",", selectedColours);
        model.Features.Clothes.Colour = colourString;
        model.Features.Shoes.Colour = colourString;

        if (Icon != null && Icon.Length > 0) // Upload Icon
        {
            var fileName = Path.GetFileName(Icon.FileName);
            var storeDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", model.Store);
            var storeFileName = $"{model.Store}-{DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")}-{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var filePath = Path.Combine(storeDirectory, storeFileName);

            if (!Directory.Exists(storeDirectory))
            {
                Directory.CreateDirectory(storeDirectory);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await Icon.CopyToAsync(fileStream);
            }

            if (Images != null && Images.Count > 0) // Upload Images
            {
                var imagePaths = new List<string>();
                foreach (var image in Images)
                {
                    var imageFileName = Path.GetFileName(image.FileName);
                    var imageStoreFileName = $"{model.Store}-{DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")}-{Guid.NewGuid()}{Path.GetExtension(imageFileName)}";
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

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                if (ModelState.IsValid)
                {
                    using (var db = new MyDbContext())
                    {
                        var store = db.Store.FirstOrDefault(s => s.Username == model.Store);

                        if (store != null)
                        {
                            var product = new Product
                            {
                                Name = model.Name,
                                Description = model.Description,
                                Category = model.Category,
                                Price = model.Price,
                                Store = store.Id,
                                Icon = storeFileName,
                                Images = allImagePaths
                            };

                            db.Product.Add(product);
                            db.SaveChanges();

                            for (int i = 0; i < model.Quantities.Count(); i++)
                            {
                                var productQuantity = new ProductQuantity
                                {
                                    Product = product.Id,
                                    Type = model.Quantities[i].Type,
                                    Quantity = model.Quantities[i].Quantity
                                };

                                db.ProductQuantity.Add(productQuantity);
                            }

                            db.SaveChanges();

                            switch (model.Category)
                            {
                                case "clothes":
                                    var clothesFeatures = new ClothesFeatures
                                    {
                                        Gender = model.Features.Clothes.Gender,
                                        Age = model.Features.Clothes.Age,
                                        Colour = model.Features.Clothes.Colour,
                                        Brand = model.Features.Clothes.Brand
                                    };

                                    List<string> clothesproperties = new List<string>
                                        {
                                            clothesFeatures.Gender,
                                            clothesFeatures.Age,
                                            clothesFeatures.Colour,
                                            clothesFeatures.Brand
                                        };

                                    for (int i = 1; i <= 4; i++)
                                    {
                                        var fv = new FeatureValue
                                        {
                                            Product = product.Id,
                                            Feature = i,
                                            Value = clothesproperties[i]
                                        };

                                        db.FeatureValue.Add(fv);
                                    }

                                    break;

                                case "shoes":
                                    var shoesFeatures = new ShoesFeatures
                                    {
                                        Gender = model.Features.Shoes.Gender,
                                        Age = model.Features.Shoes.Age,
                                        Colour = model.Features.Shoes.Colour,
                                        Brand = model.Features.Shoes.Brand
                                    };

                                    List<string> shoesproperties = new List<string>
                                        {
                                            shoesFeatures.Gender,
                                            shoesFeatures.Age,
                                            shoesFeatures.Colour,
                                            shoesFeatures.Brand
                                        };

                                    for (int i = 0; i < 4; i++)
                                    {
                                        var fv = new FeatureValue
                                        {
                                            Product = product.Id,
                                            Feature = i + 1,
                                            Value = shoesproperties[i]
                                        };

                                        db.FeatureValue.Add(fv);
                                    }

                                    break;
                                default:
                                    break;
                            }

                            db.SaveChanges();
                        }

                        return RedirectToAction("StoreProfile", "Store", new { storeUsername = model.Store });
                    }
                }
            }
        }

        return View(model);
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
}

