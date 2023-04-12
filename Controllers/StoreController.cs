﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

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
                    County = store.County,
                    District = store.District,
                    Icon = store.Icon,
                    UserReview = db.Review.SingleOrDefault(r => r.Store == store.Id && r.Reviewer == User.Identity.Name),
                    Reviews = db.Review.Where(r => r.Store == store.Id && r.Reviewer != User.Identity.Name).ToList(),
                    Products = db.Product.Where(p => p.Store == store.Id).ToList()
                };

                return View(model);
            }
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize(Roles = "Store")]
    [ValidateAntiForgeryToken]
    public ActionResult EditProfile(string storeUsername)
    {
        using (var db = new MyDbContext())
        {
            var store = db.Store.FirstOrDefault(r => r.Username == storeUsername);

            if (store != null)
            {
                var model = new EditProfileViewModel
                {
                    Name = store.Name,
                    Email = store.Email,
                };

                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Store")]
    public ActionResult EditProfile(EditProfileViewModel model, string storeUsername, IFormFile Image)
    {
        if (Image != null && Image.Length > 0)
        {
            var fileName = Path.GetFileName(Image.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                Image.CopyToAsync(fileStream);
            }

            if (ModelState.IsValid)
            {
                using (var db = new MyDbContext())
                {
                    var store = db.Store.FirstOrDefault(s => s.Username == storeUsername);

                    if (store != null)
                    {
                        store.Name = model.Name;
                        store.Email = model.Email;
                        store.Phone = model.Phone;
                        store.County = model.County;
                        store.District = model.District;
                        store.Instagram = model.Instagram;
                        store.Facebook = model.Facebook;
                        store.Icon = fileName;

                        db.SaveChanges();
                    }

                    return RedirectToAction("StoreProfile", "Store", new { storeUsername });
                }
            }
        }

        return View(model);
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
                var store = db.Store.FirstOrDefault(s => s.Username == model.StoreUsername);
                var user = db.User.FirstOrDefault(u => u.Username == model.UserUsername);

                if (store != null && user != null)
                {
                    var review = new Review
                    {
                        Store = store.Id,
                        Reviewer = user.Username,
                        Rating = model.Rating,
                        Comment = model.Comment,
                        ReviewDate = DateTime.Now
                    };

                    db.Review.Add(review);
                    db.SaveChanges();

                    return RedirectToAction("StoreProfile", new { store.Username });
                }
            }
        }

        // If the model state is not valid, return the view with the model errors
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    [ValidateAntiForgeryToken]
    public ActionResult EditReview(string storeUsername, string userUsername, int reviewId, int rating, string comment)
    {
        using (var db = new MyDbContext())
        {
            var store = db.Store.FirstOrDefault(s => s.Username == storeUsername);
            var review = db.Review.FirstOrDefault(r => r.Id == reviewId && r.Store == store.Id && r.Reviewer == userUsername);

            if (review != null)
            {
                review.Rating = rating;
                review.Comment = comment;
                review.ReviewDate = DateTime.Now;

                db.SaveChanges();
            }
        }

        return RedirectToAction("StoreProfile", new { storeUsername });
    }
}

