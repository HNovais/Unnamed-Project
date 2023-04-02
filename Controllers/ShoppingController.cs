using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using System.Linq;

public class ShoppingController : Controller
{
    [HttpGet]
    public IActionResult Shopping()
    {
        using (var db = new MyDbContext())
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
                var county = values[0];
                var district = values[1];

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
