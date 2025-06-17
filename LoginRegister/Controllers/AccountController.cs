using BCrypt.Net;
using LoginRegister.Data;
using LoginRegister.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LoginRegister.Controllers
{
    public class AccountController : Controller
    {
        private readonly LoginDbContext _context;
        private const string SessionKeyEmail = "UserEmail";

        public AccountController(LoginDbContext context) => _context = context;

        /* ----------  REGISTER  ---------- */

        // GET: /Account/Register
        public IActionResult Register()
        {
            // If already logged in, no need to register again
            if (HttpContext.Session.GetString(SessionKeyEmail) != null)
                return RedirectToAction(nameof(Welcome));

            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(string userName, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError("", "Email already registered. Please log in.");
                return View();
            }

            var user = new User
            {
                UserName = userName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // after successful registration, jump to login
            TempData["Message"] = "Registration successful. Please log in.";
            return RedirectToAction(nameof(Login));
        }

        /* ----------  LOGIN  ---------- */

        // GET: /Account/Login
        public IActionResult Login()
        {
            // If already logged in, go straight to Welcome
            if (HttpContext.Session.GetString(SessionKeyEmail) != null)
                return RedirectToAction(nameof(Welcome));

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // user NOT found → redirect to Register
            if (user == null)
            {
                TempData["Message"] = "User not found. Please register first.";
                return RedirectToAction(nameof(Register));
            }

            // password mismatch → stay on Login
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid password.");
                return View();
            }

            // ----- SUCCESS -----
            HttpContext.Session.SetString(SessionKeyEmail, user.Email);
            TempData["Message"] = "Login successful!";
            return RedirectToAction(nameof(Welcome));
        }

        /* ----------  WELCOME (protected)  ---------- */

        // GET: /Account/Welcome
        public IActionResult Welcome()
        {
            var email = HttpContext.Session.GetString(SessionKeyEmail);
            if (email == null)
                return RedirectToAction(nameof(Login));   // not logged in

            ViewBag.Email = email;
            return View();
        }

        /* ----------  LOGOUT  ---------- */

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
