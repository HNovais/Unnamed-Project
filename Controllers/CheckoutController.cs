using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

public class CheckoutController : Controller
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public ActionResult AddDiscount()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public ActionResult AddDiscount(AddDiscountViewModel model)
    {
        if (ModelState.IsValid)
        {
            using (var db = new MyDbContext())
            {
                var discountCode = new DiscountCode
                {
                    Code = model.Code,
                    Type = model.Type,
                    Value = model.Value,
                    Start = model.Start,
                    End = model.End,
                    Limit = model.Limit,
                    Count = 0,
                    IsActive = false,
                    CreationDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };

                db.DiscountCode.Add(discountCode);
                db.SaveChanges();

                return RedirectToAction("DiscountCodes", "Checkout");
            }
        }

        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public ActionResult DiscountCodes()
    {
        using (var db = new MyDbContext())
        {
            var model = new DiscountCodesViewModel
            {
                DiscountCodes = db.DiscountCode.ToList()
            };

            return View(model);
        }
    }
}

