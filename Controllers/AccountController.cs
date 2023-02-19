using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

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
            using (var db = new MyDbContext())
            {
                // Check if the username already exists in the database
                if (db.User.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "The username already exists");
                }

                else if (db.User.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "It already exists an account with this e-mail");
                }

                else
                {
                    // Generate a random salt
                    var salt = GenerateSalt();

                    // Hash the password with the salt using SHA256
                    var hashedPassword = GetHashedPassword(model.Password, salt);

                    // Create a new user using the model data
                    var user = new User
                    {
                        Name = model.Name,
                        Username = model.Username,
                        Email = model.Email,
                        Password = hashedPassword,
                        Salt = salt
                    };

                    // Add the user to the Users DbSet and save changes to the database
                    db.User.Add(user);
                    db.SaveChanges();

                    // Redirect to the appropriate page after registration
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        return View(model);
    }

    private string GenerateSalt()
    {
        // Generate a random salt using a cryptographically secure random number generator
        var rng = new RNGCryptoServiceProvider();
        var saltBytes = new byte[32];
        rng.GetBytes(saltBytes);
        var salt = Convert.ToBase64String(saltBytes);

        return salt;
    }

    private string GetHashedPassword(string password, string salt)
    {
        // Append the salt to the password
        var saltedPassword = password + salt;

        // Compute the hash of the salted password using SHA256
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return hashedPassword;
        }
    }
}
