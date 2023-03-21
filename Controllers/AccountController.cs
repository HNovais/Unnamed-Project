using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

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
    [Authorize(Roles = "User")]
    public ActionResult ShoppingCart()
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

            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cartItems,
                TotalPrice = cartItems.Sum(ci => ci.Price * ci.Quantity)
            };
            
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DecrementQuantity(int id, int quantity)
    {
        using (var db = new MyDbContext())
        {
            var cartProduct = db.CartProduct.FirstOrDefault(cp => cp.Id == id);

            if (cartProduct != null)
            {
                if (quantity >= 2)
                    cartProduct.Quantity -= 1;

                else
                    db.Remove(cartProduct);
            }

            db.SaveChanges();

            return RedirectToAction("ShoppingCart", "Account");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult IncrementQuantity(int id)
    {
        using (var db = new MyDbContext())
        {
            var cartProduct = db.CartProduct.FirstOrDefault(cp => cp.Id == id);

            if (cartProduct != null)
                cartProduct.Quantity += 1;

            db.SaveChanges();

            return RedirectToAction("ShoppingCart", "Account");
        }
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
    public async Task<ActionResult> LoginAsync(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            using (var db = new MyDbContext())
            {
                // Try to find a user with the provided username or email
                var user = db.User.FirstOrDefault(u => u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail);

                if (user != null && VerifyPassword(model.Password, user.Salt, user.Password))
                {
                    // Create an authenticated identity
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Username));
                    identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
                    identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
                    
                    // Set the forms authentication ticket
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Try to find a store with the provided username
                    var store = db.Store.FirstOrDefault(s => s.Username == model.UsernameOrEmail || s.Email == model.UsernameOrEmail);

                    if (store != null && VerifyPassword(model.Password, store.Salt, store.Password))
                    {
                        // Create an authenticated identity
                        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, store.Username));
                        identity.AddClaim(new Claim(ClaimTypes.Name, store.Username));
                        identity.AddClaim(new Claim(ClaimTypes.Role, "Store"));

                        // Set the forms authentication ticket
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

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
