using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

public class CheckoutController : Controller
{
    [HttpGet]
    [Authorize(Roles = "User")]
    public ActionResult Checkout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        using (var db = new MyDbContext())
        {
            var cart = db.Cart.FirstOrDefault(c => c.User == userId);

            if (cart == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var cartItems = new List<CartItemViewModel>();
            List<CartProduct> cartProducts = db.CartProduct.Where(cp => cp.Cart == cart.Id).ToList();

            foreach (var cartProduct in cartProducts)
            {
                var product = db.Product.FirstOrDefault(p => p.Id == cartProduct.Product);

                if (product == null)
                {
                    continue;
                }

                var cartItem = new CartItemViewModel
                {
                    CartItemId = cartProduct.Id,
                    ProductId = cartProduct.Product,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = cartProduct.Quantity
                };

                cartItems.Add(cartItem);
            }

            var model = new CheckoutViewModel
            {
                Items = cartItems,
                Subtotal = cartItems.Sum(ci => ci.Price * ci.Quantity),
                Shipping = 3,
                Total = cartItems.Sum(ci => ci.Price * ci.Quantity) + 3,
                Discount = 0,
            };

            return View(model);
        }
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    [ValidateAntiForgeryToken]
    public IActionResult ApplyDiscount(CheckoutViewModel model, List<CartItemViewModel> Items)
    {
        model.Items = Items;
        using (var db = new MyDbContext())
        {
            var discountCode = db.DiscountCode.FirstOrDefault(dc => dc.Code == model.DiscountCode);

            if (discountCode != null)
            {
                if (discountCode.IsActive)
                {
                    if ((discountCode.Limit != null && discountCode.Count >= discountCode.Limit) || discountCode.Start > DateTime.Now || discountCode.End < DateTime.Now)
                        discountCode.IsActive = false;
                }  
                else
                {
                    if ((discountCode.Limit == null || discountCode.Count < discountCode.Limit) &&
                        (discountCode.Start == null || discountCode.Start <= DateTime.Now) &&
                        (discountCode.End == null || discountCode.End >= DateTime.Now))
                        discountCode.IsActive = true;
                }
                    

                db.SaveChanges();

                float discount = 0;
                int value = 0;

                if (discountCode.IsActive)
                {
                    if (discountCode.Value != "Free Shipping")
                    {
                        int v = int.Parse(discountCode.Value);
                        value = v;
                    }

                    if (discountCode.Type == "Percentage")
                        discount = model.Subtotal * (value / 100);
                    else if (discountCode.Type == "Value")
                        discount = value;
                    else if (discountCode.Type == "Shipping")
                        model.Shipping = 0;

                    model.Discount = discount;
                    model.Total = model.Subtotal + model.Shipping - model.Discount;
                }
                else
                {
                    ModelState.AddModelError("DiscountCode", "Invalid discount code");
                    return View("Checkout", model);
                }
            }
            else
            {
                ModelState.AddModelError("DiscountCode", "Invalid discount code");
                return View("Checkout", model);
            }

            return View("Checkout", model);
        }
    }

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

