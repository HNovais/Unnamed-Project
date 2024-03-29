﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using unnamed.Models;

namespace unnamed.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.UserName = HttpContext.User.Identity.Name;
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

        public ActionResult Search(string searchQuery)
        {
            using (var db = new MyDbContext())
            {
                var results = db.Store.Where(i => i.Name.Contains(searchQuery)).ToList();
                return View(results);
            }
        }
    }
}