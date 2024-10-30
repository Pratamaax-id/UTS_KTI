using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SampleSecureWeb.Data;
using SampleSecureWeb.Models;
using SampleSecureWeb.ViewModels;
using System.Text.RegularExpressions;

namespace SampleSecureWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUser _userData;

        public AccountController(IUser user)
        {
            _userData = user;
        }

        // GET: AccountController
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegistrationViewModel registrationViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validasi panjang kata sandi
                    if (registrationViewModel.Password.Length <= 12 || registrationViewModel.Password.Length >= 128)
                    {
                        ModelState.AddModelError("Password", "Kata sandi harus antara 12 dan 128 karakter.");
                        return View(registrationViewModel);
                    }
                    // Validasi kata sandi
                    if (!IsValidPassword(registrationViewModel.Password))
                    {
                        ModelState.AddModelError("Password", "Kata sandi harus minimal 12 karakter dan mengandung setidaknya satu huruf besar, satu huruf kecil, satu angka, dan satu karakter spesial.");
                        return View(registrationViewModel);
                    }

                    var user = new Models.User
                    {
                        Username = registrationViewModel.Username,
                        Password = registrationViewModel.Password,
                        RoleName = "Contributor"
                    };
                    _userData.Registration(user);
                    ViewBag.Message = "Pendaftaran berhasil!";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(registrationViewModel);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel loginViewModel)
        {
            try
            {
                loginViewModel.ReturnUrl = loginViewModel.ReturnUrl ?? Url.Content("~/");

                var user = new User
                {
                    Username = loginViewModel.Username,
                    Password = loginViewModel.Password
                };

                var loginUser = _userData.Login(user);
                if (loginUser == null)
                {
                    ViewBag.Message = "Upaya Login Tidak Valid";
                    return View(loginViewModel);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = loginViewModel.RememberLogin
                    });

                return RedirectToAction("Index", "Home");
            }
            catch (System.Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View(loginViewModel);
        }

        // GET: Ubah Kata Sandi
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validasi panjang kata sandi baru
                    if (changePasswordViewModel.NewPassword.Length <= 12 || changePasswordViewModel.NewPassword.Length >= 128)
                    {
                        ModelState.AddModelError("NewPassword", "Kata sandi harus antara 12 dan 128 karakter.");
                        return View(changePasswordViewModel);
                    }

                    // Validasi kata sandi baru
                    if (!IsValidPassword(changePasswordViewModel.NewPassword))
                    {
                        ModelState.AddModelError("NewPassword", "Kata sandi harus minimal 12 karakter dan mengandung setidaknya satu huruf besar, satu huruf kecil, satu angka, dan satu karakter spesial.");
                        return View(changePasswordViewModel);
                    }


                    var username = User.Identity?.Name;
                    if (username == null)
                    {
                        return RedirectToAction("Login");
                    }

                    var user = _userData.GetUserByUsername(username);
                    if (user == null || !BCrypt.Net.BCrypt.Verify(changePasswordViewModel.CurrentPassword, user.Password))
                    {
                        ModelState.AddModelError("CurrentPassword", "Kata sandi saat ini salah.");
                        return View(changePasswordViewModel);
                    }

                    // Hanya melakukan pembaruan jika semua validasi berhasil
                    user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordViewModel.NewPassword);
                    _userData.UpdateUser(user);
                    ViewBag.Message = "Kata sandi berhasil diubah!";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(changePasswordViewModel);
        }

        private bool IsValidPassword(string password)
        {
            // Memeriksa panjang kata sandi
            return password.Length >= 12 &&  // Minimal 12 karakter
                   password.Length <= 128 && // Maksimal 128 karakter
                   Regex.IsMatch(password, @"[A-Z]") && // Harus ada huruf besar
                   Regex.IsMatch(password, @"[a-z]") && // Harus ada huruf kecil
                   Regex.IsMatch(password, @"[0-9]") && // Harus ada angka
                   Regex.IsMatch(password, @"[!@#$%^&*(),.?\""{}|<>]"); // Harus ada karakter spesial
        }


        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
