using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PeopleDirectoryApplication.Models;
using PeopleDirectoryApplication.Models.Identity;
using PeopleDirectoryApplication.ViewModels;

namespace PeopleDirectoryApplication.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(loginViewModel);
        }

        var email = loginViewModel.EmailAddress.Trim();
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            _logger.LogWarning("Login failed. No user found for email {Email}", email);
            TempData["Error"] = "Please enter valid credentials";
            return View(loginViewModel);
        }

        var passwordCheck = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);
        if (!passwordCheck)
        {
            _logger.LogWarning("Login failed. Invalid password for email {Email}", email);
            TempData["Error"] = "Password incorrect. Please try again";
            return View(loginViewModel);
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, UserRoles.Admin);
        if (!isAdmin)
        {
            _logger.LogWarning("Login blocked. Non-admin attempted to access admin area. UserId {UserId}", user.Id);
            TempData["Error"] = "Only admin users can access management features.";
            return View(loginViewModel);
        }

        var signInResult = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            _logger.LogWarning("Sign-in operation failed for user {UserId}.", user.Id);
            TempData["Error"] = "Sign-in failed. Please try again.";
            return View(loginViewModel);
        }

        _logger.LogInformation("Admin login succeeded for user {UserId}", user.Id);
        return Redirect("/admin/people");
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out");
        return Redirect("/");
    }
}
