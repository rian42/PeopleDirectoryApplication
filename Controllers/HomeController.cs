using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PeopleDirectoryApplication.Models;
using PeopleDirectoryApplication.Repositories.Interfaces;

namespace PeopleDirectoryApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPersonRepository _userRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IPersonRepository userRepo, ILogger<HomeController> logger)
        {
            _userRepository = userRepo;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var results = await _userRepository.GetPersonsBySearchTerm(searchTerm ?? "");

            return new JsonResult(results);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}