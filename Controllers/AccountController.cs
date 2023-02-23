using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class AccountController : Controller
{
    [HttpGet]
    public ActionResult Register()
    {
        return View();
    }

    [HttpGet]
    public ActionResult RegisterStore()
    {
        return View();
    }

    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    [HttpGet]
    [Route("Account/{storeUsername}")]
    public ActionResult StoreProfile(string storeUsername)
    {
        using (var db = new MyDbContext())
        {
            Console.WriteLine(storeUsername);
            var store = db.Store.FirstOrDefault(s => s.Username == storeUsername);

            if (store != null)
            {
                var model = new RegisterStoreViewModel
                {
                    Name = store.Name,
                    Email = store.Email
                    // set other properties as needed
                };

                return View(model);
            }
        }

        return RedirectToAction("Index", "Home");
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult RegisterStore(RegisterStoreViewModel model)
    {
        if (ModelState.IsValid)
        {
            using (var db = new MyDbContext())
            {
                // Check if the username already exists in the database
                if (db.Store.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "The username already exists");
                }

                else if (db.Store.Any(u => u.Email == model.Email))
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
                    var store = new Store
                    {
                        Name = model.Name,
                        Username = model.Username,
                        Email = model.Email,
                        Phone = model.Phone,
                        Password = hashedPassword,
                        Salt = salt
                    };

                    // Add the user to the Users DbSet and save changes to the database
                    db.Store.Add(store);
                    db.SaveChanges();

                    // Redirect to the appropriate page after registration
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            using (var db = new MyDbContext())
            {
                // Try to find a user with the provided username or email
                var user = db.User.FirstOrDefault(u => u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail);

                if (user != null && VerifyPassword(model.Password, user.Salt, user.Password))
                {
                    // User login succeeded, set the user role in the cookie
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, "User")
                };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                        IssuedUtc = DateTimeOffset.UtcNow,
                        IsPersistent = true
                    };

                    HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Try to find a store with the provided username
                    var store = db.Store.FirstOrDefault(s => s.Username == model.UsernameOrEmail || s.Email == model.UsernameOrEmail);

                    if (store != null && VerifyPassword(model.Password, store.Salt, store.Password))
                    {
                        // Store login succeeded, set the store role in the cookie
                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, store.Username),
                        new Claim(ClaimTypes.Role, "Store")
                    };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                            IssuedUtc = DateTimeOffset.UtcNow,
                            IsPersistent = true
                        };

                        HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid username or password.");
                    }
                }
            }
        }

        return View(model);
    }



    private bool VerifyPassword(string enteredPassword, string storedSalt, string storedHashedPassword)
    {
        using (var sha256 = SHA256.Create())
        {
            // Combine entered password with stored salt and hash the result
            string saltedPassword = enteredPassword + storedSalt;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

            // Compare the hashed password with the stored hashed password
            return hashedPassword == storedHashedPassword;
        }
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
