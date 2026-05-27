using System.Diagnostics;
using ASP_MVC.Entities;
using ASP_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using ASP_MVC.Repository;

namespace ASP_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserRepository _userRepository;

        public HomeController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        public IActionResult Index()
        {
            var users = _userRepository.GetAll();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new User());
        }

        [HttpPost]
        public IActionResult Create(User? user)
        {
            if (user is null || string.IsNullOrWhiteSpace(user.UserName))
            {
                ModelState.AddModelError("UserName", "名前を入力してください。");
                return View(user ?? new User());
            }

            user.UserName = user.UserName.Trim();
            _userRepository.Add(user);
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Detail(int id)
        {
            return Content($"id = {id}");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
