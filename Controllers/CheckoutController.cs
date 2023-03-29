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
                string value = model.Value;
                if (model.Type == "Shipping")
                {
                    value = "Free Shipping";
                }
                else
                {
                    value = model.Value;
                }

                var discountCode = new DiscountCode
                {
                    Code = model.Code,
                    Type = model.Type,
                    Value = value,
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

            DateTime now = DateTime.Now;

            foreach (var discount in model.DiscountCodes){
                if (discount.Start < now && discount.End > now)
                    discount.IsActive = true;
            }

            db.SaveChanges();

            return View(model);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public ActionResult EditDiscount(int id)
    {
        using (var db = new MyDbContext())
        {
            var discount = db.DiscountCode.FirstOrDefault(x => x.Id == id);

            var model = new AddDiscountViewModel
            {
                Id = id,
                Code = discount.Code,
                Type = discount.Type,
                Value = discount.Value,
                Start = discount.Start,
                End = discount.End,
                Limit = discount.Limit,
                isActive = discount.IsActive,
                Creation = discount.CreationDate,
                Updated = discount.UpdateDate
            };

            return RedirectToAction("EditDiscount", "Checkout", new { id }); 
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public ActionResult EditDiscount(AddDiscountViewModel model, int id)
    {
        if (ModelState.IsValid)
        {
            using (var db = new MyDbContext())
            {
                var discount = db.DiscountCode.FirstOrDefault(x => x.Id == id);

                if (discount != null)
                {
                    discount.Code = model.Code;
                    discount.Type = model.Type;
                    discount.Value = model.Value;
                    discount.Start = model.Start;
                    discount.End = model.End;
                    discount.Limit = model.Limit;
                    discount.IsActive = model.isActive;
                    discount.CreationDate = model.Creation;
                    discount.UpdateDate = DateTime.Now;

                    db.SaveChanges();
                }

                return RedirectToAction("DiscountCodes", "Checkout");
            }
        }

        return View(id);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public ActionResult DeleteDiscount(int id)
    {
        using (var db = new MyDbContext())
        {
            var discountCode = db.DiscountCode.FirstOrDefault(d => d.Id == id);

            if (discountCode != null)
            {
                db.DiscountCode.Remove(discountCode);
                db.SaveChanges();

                return RedirectToAction("DiscountCodes", "Checkout");
            }     

            return RedirectToAction("Index", "Home");
        }
    }

}

