using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using unnamed.Models;

public class ProductController : Controller
{
    [HttpGet]
    [Authorize(Roles = "User")]
    public ActionResult ProductPage(int productID)
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
}

