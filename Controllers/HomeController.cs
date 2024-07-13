using System.Diagnostics;
using LoginwithEmail.Context;
using LoginwithEmail.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoginwithEmail.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.link = "https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/dist/browser/signalr.min.js";
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
