using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Org.BouncyCastle.Crypto;
using System.Linq;

public class ShoppingController : Controller
{
    [HttpGet]
    [Route("shopping")]
    public IActionResult Shopping(ShoppingViewModel m)
    {
        using (var db = new MyDbContext())
        {
            if (m.DistrictCounties == null)
            {
                var products = db.Product.ToList();
                var stores = db.Store.ToList();
                var districtCounties = GetDistrictCounties();

                var model = new ShoppingViewModel
                {
                    Min = 0,
                    Max = 1000,
                    Products = products,
                    Stores = stores,
                    DistrictCounties = districtCounties
                };

                return View(model);
            }

            else 
            {
                var products = m.Products;

                if (!string.IsNullOrEmpty(m.Category))
                {
                    products = db.Product.Where(p => p.Category == m.Category).ToList();
                }

                if (m.Min.HasValue)
                {
                    products = products.Where(p => p.Price >= m.Min.Value).ToList();
                }

                if (m.Max.HasValue)
                {
                    products = products.Where(p => p.Price <= m.Max.Value).ToList();
                }

                var stores = db.Store.ToList();
                var districtCounties = GetDistrictCounties();

                var model = new ShoppingViewModel
                {
                    Min = m.Min,
                    Max = m.Max,
                    Products = products.ToList(),
                    Stores = stores,
                    DistrictCounties = districtCounties,
                    Category = m.Category,
                    District = m.District,
                    County = m.County
                };

                return View(model);
            }
        }
    }
    

    [HttpPost]
    public IActionResult Filter(ShoppingViewModel model)
    {
        using (var db = new MyDbContext())
        {
            var products = db.Product.AsQueryable();
            var stores = db.Store.ToList();
            var districtCounties = GetDistrictCounties();

            if (!string.IsNullOrEmpty(model.Category))
            {
                products = products.Where(p => p.Category == model.Category);
            }

            if (model.Min.HasValue)
            {
                products = products.Where(p => p.Price >= model.Min.Value);
            }

            if (model.Max.HasValue)
            {
                products = products.Where(p => p.Price <= model.Max.Value);
            }

            var filteredProducts = products.ToList();

            if (filteredProducts.Count == 0)
            {
                ViewBag.Message = "No products were found matching your search criteria.";
            }

            var filteredStores = stores.Where(s => filteredProducts.Any(p => p.Store == s.Id)).ToList();

            var resultModel = new ShoppingViewModel
            {
                Min = model.Min,
                Max = model.Max,
                Products = filteredProducts,
                Stores = filteredStores,
                DistrictCounties = districtCounties,
                District = model.District, // include selected district in the view model
                County = model.County // include selected county in the view model
            };

            
            return RedirectToAction("Shopping", resultModel);
        }
    }

    private Dictionary<string, List<string>> GetDistrictCounties()
    {
        var districtCounties = new Dictionary<string, List<string>>();

        using (var reader = new StreamReader("Data/ConcelhoDistrito.csv"))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                var district = values[1];
                var county = values[0];

                if (!districtCounties.ContainsKey(district))
                {
                    districtCounties[district] = new List<string>();
                }

                districtCounties[district].Add(county);
            }
        }

        return districtCounties;
    }

}
