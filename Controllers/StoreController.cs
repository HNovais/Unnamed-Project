using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

public class StoreController : Controller
{
    [HttpGet]
    [Route("{storeUsername}")]
    public ActionResult StoreProfile(string storeUsername)
    {
        using (var db = new MyDbContext())
        {
            var store = db.Store.FirstOrDefault(s => s.Username == storeUsername);

            if (store != null)
            {
                var model = new StoreProfileViewModel
                {
                    Name = store.Name,
                    Email = store.Email,
                    // set other properties as needed

                    Reviews = db.Review.Where(r => r.Id == store.Id).ToList()
                };

                return View(model);
            }
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    [ValidateAntiForgeryToken]
    public IActionResult AddReview(AddReviewViewModel model)
    {
        if (ModelState.IsValid)
        {
            using (var db = new MyDbContext())
            {
                var review = new Review
                {
                    Store = model.StoreID,
                    User = model.UserID,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    ReviewDate = DateTime.Now
                };

                db.Review.Add(review);
                db.SaveChanges();
            }

            return RedirectToAction("{storeUsername}");
        }

        // If the model state is not valid, return the view with the model errors
        return View(model);
    }

}

