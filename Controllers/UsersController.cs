using System.Net.Mail;
using System.Security.Cryptography;
using LoginwithEmail.Context;
using LoginwithEmail.Models;
using LoginwithEmail.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoginwithEmail.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }



        [HttpGet]
        [Route("/register")]
        public IActionResult Register()
        {
            return View();
        }

        public IActionResult RegistrationConfirmation()
        {
            return View();  // Create a view that says "Check your email for verification link."
        }


        private async Task SendVerificationEmail(User user)
        {
            var verifyUrl = Url.Action("VerifyEmail", "Users", new { id = user.Id }, protocol: HttpContext.Request.Scheme);
            var message = new MailMessage
            {
                From = new MailAddress("labautomation7@gmail.com", "Lab Automation"),
                Subject = "Complete Your Registration",
                Body = $"Please verify your account by clicking <a href='{verifyUrl}'>here</a>.",
                IsBodyHtml = true,
            };
            message.To.Add(new MailAddress(user.Email));

            using (var smtp = new SmtpClient("smtp.gmail.com"))
            {
                smtp.Port = 587;
                smtp.Credentials = new System.Net.NetworkCredential("labautomation7@gmail.com", "zibbgfbonagebwxd");
                smtp.EnableSsl = true;

                await smtp.SendMailAsync(message);
            }
        }



        [HttpPost]
        [Route("/register")]
        public async Task<IActionResult> Register(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists
                var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "An account with this email already exists.");

                    return View(model);
                }

                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    RememberToken = "",  // Token will be generated later
                    Role = "User",
                    Password = model.Password,  // Consider hashing this password before storing
                    PhoneNumber = model.PhoneNumber
                };

                _context.Add(user);
                await _context.SaveChangesAsync();

                // Send verification email
                await SendVerificationEmail(user);

                return RedirectToAction("RegistrationConfirmation");
            }

            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> VerifyEmail(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Generate a random token
                user.RememberToken = GenerateRandomToken();
                _context.Update(user);
                await _context.SaveChangesAsync();

                return View("TokenDisplay", user);  // Create this view to display the token
            }

            return NotFound();  // User not found
        }





        private string GenerateRandomToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[32];
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }




        [HttpGet]
        [Route("/login")]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password); // Consider hashing passwords!

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            if (string.IsNullOrEmpty(user.RememberToken))
            {
                await SendVerificationEmail(user); // Re-send verification email
                return RedirectToAction("RegistrationConfirmation"); // Inform user to check email
            }

            // Create session and store user ID
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name); // Optional, store other needed info

            return RedirectToAction("Dashboard"); // Redirect to the dashboard
        }

        public IActionResult Dashboard()
        {
            // Check if the user ID is stored in the session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // If not logged in, redirect to the login page
                return RedirectToAction("Login");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                // If user not found, handle accordingly (e.g., session data is stale)
                HttpContext.Session.Clear(); // Clear the session
                return RedirectToAction("Login");
            }

            // Pass the user's name to the view
            ViewBag.UserName = user.Name;
            return View();
        }

        public IActionResult Logout()
        {
            // Clear the user session
            HttpContext.Session.Clear();

            // Redirect to the login page or home page
            return RedirectToAction("Login");
        }


    }
}
