using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics.Metrics;

public class ShoppingController : Controller
{
    [HttpGet]
    [Route("shopping")]
    public IActionResult Shopping()
    {
        using (var db = new MyDbContext())
        {
            var products = db.Product.ToList();
            var stores = db.Store.ToList();
            var dc = GetDistrictCounties();
            var categories = db.Category.Select(c => c.Name).ToList();

            var model = new ShoppingViewModel
            {
                Products = products,
                Stores = stores,
                DistrictCounties = dc,
                Categories = categories,
                Min = 0,
                Max = 1000
            };

            return View(model);
        }
    }

    [HttpPost]
    public IActionResult FilterProducts(string store, string county, string district, string category, float min, float max, Dictionary<string, List<string>> featureValues)
    {
        using (var db = new MyDbContext())
        {
            var products = db.Product.AsQueryable();
            var stores = db.Store.AsQueryable();
            Dictionary<string, List<string>> features = new Dictionary<string, List<string>>();

            // Apply filters
            if (!string.IsNullOrEmpty(store))
            {
                stores = stores.Where(s => s.Name.Contains(store));
                products = products.Where(p => stores.Any(s => s.Id == p.Store));
            }

            if (!string.IsNullOrEmpty(county))
            {
                stores = stores.Where(s => s.County.Contains(county));
                products = products.Where(p => stores.Any(s => s.Id == p.Store));
            }

            if (!string.IsNullOrEmpty(district))
            {
                stores = stores.Where(s => s.District.Contains(district));
                products = products.Where(p => stores.Any(s => s.Id == p.Store));
            }

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category == category);
                features = CategoryFilter(category);

                if (featureValues != null && featureValues.Count > 0)
                {
                    foreach (var kvp in featureValues)
                    {
                        var featureName = kvp.Key;
                        var featureValue = kvp.Value;

                        if (featureValue != null && featureValue.Count() > 0)
                        {
                            var cID = db.Category.FirstOrDefault(c => c.Name == category).Id;
                            var feature = db.Feature.FirstOrDefault(f => f.Name == featureName && f.Category == cID);

                            if (feature != null)
                            {
                                var featureValueQuery = db.FeatureValue.Where(fv => fv.Feature == feature.Id && featureValue.Contains(fv.Value));

                                products = products.Where(p => featureValueQuery.Any(fv => fv.Product == p.Id));
                            }
                        }
                    }
                }
            }

            products = products.Where(p => p.Price >= min);
            products = products.Where(p => p.Price <= max);

            var model = new ShoppingViewModel
            {
                Products = products.ToList(),
                Stores = db.Store.ToList(),
                DistrictCounties = GetDistrictCounties(),
                Categories = db.Category.Select(c => c.Name).ToList(),
                Store = store,
                County = county,
                District = district,
                Category = category,
                Min = min,
                Max = max,
                CatFilter = features
            };

            return View("Shopping", model);
        }
    }

    private Dictionary<string, List<string>> GetDistrictCounties()
    {
        var districtCounties = new Dictionary<string, List<string>>();

        using (var db = new MyDbContext())
        {
            var dc = db.DistrictCounty.ToList();

            // Group the counties by district using LINQ
            var grouped = dc.GroupBy(x => x.District);

            // Populate the dictionary with the grouped data
            foreach (var group in grouped)
            {
                var district = group.Key;
                var counties = group.Select(x => x.County).ToList();
                districtCounties.Add(district, counties);
            }
        }

        return districtCounties;
    }

    private Dictionary<string, List<string>> CategoryFilter(string category)
    {
        Dictionary<string, List<string>> filters = new Dictionary<string, List<string>>();

        using (var db = new MyDbContext())
        {
            var cat = db.Category.FirstOrDefault(c => c.Name == category);

            var features = db.Feature.Where(f => f.Category == cat.Id).ToList();

            foreach (var feature in features)
            {
                var options = db.FeatureOptions
                    .Where(fo => fo.Feature == feature.Id)
                    .Select(fo => fo.Value)
                    .ToList();

                filters.Add(feature.Name, options);
            }

            return filters;
        }
    }
}
