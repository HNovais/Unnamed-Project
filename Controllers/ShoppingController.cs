using Microsoft.AspNetCore.Mvc;

public class ShoppingController : Controller
{
    [HttpGet]
    [Route("shopping")]
    public IActionResult Shopping(ShoppingViewModel m)
    {
        using (var db = new MyDbContext())
        {
            if (m == null ||
            (m.Category == null && m.District == null && m.County == null &&
            m.Min == null && m.Max == null))
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
                var products = db.Product.ToList();

                if (!string.IsNullOrEmpty(m.Category))
                {
                    products = db.Product.Where(p => p.Category == m.Category).ToList();
                }

                if (m.Min.HasValue)
                {
                    products = products.Where(p => p.Price >= m.Min.Value).ToList();
                }
                else
                {
                    m.Min = 0;
                }

                if (m.Max.HasValue)
                {
                    products = products.Where(p => p.Price <= m.Max.Value).ToList();
                }
                else
                {
                    m.Max = 1000;
                }

                var stores = db.Store.ToList();

                List<Store> filteredStores = new List<Store>();
                List<Product> filteredProducts = new List<Product>();

                if (!string.IsNullOrEmpty(m.District) && !string.IsNullOrEmpty(m.County))
                {
                    foreach (var store in stores)
                    {
                        if (store.District == m.District && store.County == m.County)
                        {
                            filteredStores.Add(store);
                        }
                    }
                }

                else if (!string.IsNullOrEmpty(m.District) && string.IsNullOrEmpty(m.County))
                {
                    foreach (var store in stores)
                    {
                        if (store.District == m.District)
                        {
                            filteredStores.Add(store);
                        }
                    }
                }

                else
                {
                    filteredStores = stores;
                }

                foreach (var product in products)
                {
                    foreach (var store in filteredStores)
                    {
                        if (product.Store == store.Id)
                        {
                            filteredProducts.Add(product);
                            break;
                        }
                    }
                }

                products = filteredProducts;

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
