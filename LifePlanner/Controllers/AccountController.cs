using LifePlanner.Data.Entities;
using LifePlanner.Helpers;
using LifePlanner.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LifePlanner.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;

        public AccountController(IUserHelper userHelper)
        {
            _userHelper = userHelper;
        }


        // LOGIN - GET
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (User.Identity != null &&
                User.Identity.IsAuthenticated &&
                User.Identity.Name != null)
            {
                var user = await _userHelper
                    .GetUserByEmailAsync(User.Identity.Name);

                if (user != null &&
                    await _userHelper.IsUserInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // LOGIN - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _userHelper.LoginAsync(model);
            if (result.Succeeded)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);

                if (user != null &&
                    await _userHelper.IsUserInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(
                string.Empty,
                "Email ou password incorretos.");

            return View(model);
        }


        // LOGOUT
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();

            return RedirectToAction("Index", "Home");
        }


        // REGISTER - GET
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }


        // REGISTER - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterNewUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userHelper
                .GetUserByEmailAsync(model.Username);

            if (user != null)
            {
                ModelState.AddModelError(
                    "Username",
                    "Já existe um utilizador com este email.");

                return View(model);
            }

            user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Username,
                UserName = model.Username
            };

            var result = await _userHelper
                .AddUserAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Faz login automaticamente após o registo
                var loginModel = new LoginViewModel
                {
                    Username = model.Username,
                    Password = model.Password,
                    RememberMe = false
                };

                var loginResult =
                    await _userHelper.LoginAsync(loginModel);

                if (loginResult.Succeeded)
                {
                    return RedirectToAction(
                        "Index",
                        "Home");
                }

                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }


        // EDITAR PERFIL - GET
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChangeUser()
        {
            if (User.Identity?.Name == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userHelper
                .GetUserByEmailAsync(User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ChangeUserViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Email ?? string.Empty
            };

            return View(model);
        }


        // EDITAR PERFIL - POST
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUser(
            ChangeUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (User.Identity?.Name == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userHelper
                .GetUserByEmailAsync(User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            // Verificar se o novo email já pertence
            // a outro utilizador
            if (!string.Equals(
                    user.Email,
                    model.Username,
                    StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _userHelper
                    .GetUserByEmailAsync(model.Username);

                if (existingUser != null)
                {
                    ModelState.AddModelError(
                        "Username",
                        "Já existe um utilizador com este email.");

                    return View(model);
                }
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Username;
            user.UserName = model.Username;

            var result = await _userHelper
                .UpdateUserAsync(user);

            if (result.Succeeded)
            {
                ViewBag.UserMessage =
                    "Perfil atualizado com sucesso.";

                return View(model);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }


        // ALTERAR PASSWORD - GET
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }


        // ALTERAR PASSWORD - POST
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (User.Identity?.Name == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userHelper
                .GetUserByEmailAsync(User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userHelper
                .ChangePasswordAsync(
                    user,
                    model.OldPassword,
                    model.NewPassword);

            if (result.Succeeded)
            {
                ViewBag.UserMessage =
                    "Password alterada com sucesso.";

                ModelState.Clear();

                return View(
                    new ChangePasswordViewModel());
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        // RECUPERAR PASSWORD - GET
        [HttpGet]
        public IActionResult RecoverPassword()
        {
            return View();
        }


        // RECUPERAR PASSWORD - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecoverPassword(
            RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userHelper
                .GetUserByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Não existe nenhum utilizador com este email.");

                return View(model);
            }

            var token = await _userHelper
                .GeneratePasswordResetTokenAsync(user);

            return RedirectToAction(
                nameof(ResetPassword),
                new
                {
                    email = model.Email,
                    token = token
                });
        }


        // RESET PASSWORD - GET
        [HttpGet]
        public IActionResult ResetPassword(
            string email,
            string token)
        {
            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(token))
            {
                return BadRequest();
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }


        // RESET PASSWORD - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userHelper
                .GetUserByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Utilizador não encontrado.");

                return View(model);
            }

            var result = await _userHelper
                .ResetPasswordAsync(
                    user,
                    model.Token,
                    model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] =
                    "Password alterada com sucesso. Já pode iniciar sessão.";

                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }


        // ACESSO NÃO AUTORIZADO
        public IActionResult NotAuthorized()
        {
            return View();
        }
    }
}
