using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    [HttpGet]
    public ActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Create a new user using the model data
            var user = new User
            {
                Name = model.Name,
                Username = model.Username,
                Email = model.Email,
                Password = model.Password
            };

            // Add the user to the Users DbSet and save changes to the database
            using (var db = new MyDbContext())
            {
                db.User.Add(user);
                db.SaveChanges();
            }

            // Redirect to the appropriate page after registration
            return RedirectToAction("Index", "Home");
        }

        return View(model);
    }
}
