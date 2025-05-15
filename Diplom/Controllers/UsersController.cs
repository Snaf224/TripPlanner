using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Diplom.Models; 
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Diplom.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }
    }
}
