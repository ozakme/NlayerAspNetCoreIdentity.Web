using AspNetCoreIdentity.Repository.Models;
using AspNetCoreIdentity.Core.ViewModels;
using AspNetCoreIdentity.Web.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using AspNetCoreIdentity.Service.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AspNetCoreIdentity.Core.Models;

namespace AspNetCoreIdentity.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            returnUrl ??= Url.Action("Index", "Home");

            var hasUser = await _userManager.FindByEmailAsync(model.Email);

            if (hasUser == null)
            {
                ModelState.AddModelError(string.Empty, "Email veya şifre yanlış");
                return View();
            }


            var signInResult = await _signInManager.PasswordSignInAsync(hasUser, model.Password, model.RememberMe, true);


            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelErrorList(new List<string>() { "3 dakika boyunca giriş yapamazsınız." });
                return View();
            }

            if (!signInResult.Succeeded)
            {
                ModelState.AddModelErrorList(new List<string>() { $"Email veya şifre yanlış", $"Başarısız giriş sayısı = {await _userManager.GetAccessFailedCountAsync(hasUser)}" });
                return View();
            }

            if (hasUser.BirthDate.HasValue)
            {
                await _signInManager.SignInWithClaimsAsync(hasUser, model.RememberMe, new[] { new Claim("birthdate", hasUser.BirthDate.Value.ToString()) });
                }
                return Redirect(returnUrl!);
       }
    


        [HttpPost]
        public async Task< IActionResult> SignUp(SignUpViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

           var identityResult= await _userManager.CreateAsync(new() { UserName= request.UserName,PhoneNumber=request.Phone, Email= request.Email},request.PasswordConfirm);
            if (identityResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Üyelik kayıt işlemi başarılı bir şekilde gerçekleşmiştir";
                return RedirectToAction(nameof(HomeController.SignUp));
            }

            foreach (IdentityError item in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description); 

            }
            return View();
        }


        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel request)
        {
            var hasUser = await _userManager.FindByEmailAsync(request.Email);

            if (hasUser == null)
            {
                ModelState.AddModelError(String.Empty, "Bu email adresine sahip kullanıcı bulunamamıştır.");
                return View();
            }

            string passwordResestToken = await _userManager.GeneratePasswordResetTokenAsync(hasUser);

            var passwordResetLink = Url.Action("ResetPassword", "Home", new { userId = hasUser.Id, Token = passwordResestToken }, HttpContext.Request.Scheme);
            //örnek link https://localhost:7024?userId=12213&token=aajsdfjdsalkfjkdsfj

            await _emailService.SendResetPasswordEmail(passwordResetLink!, hasUser.Email!);

            TempData["SuccessMessage"] = "Şifre yenileme linki, eposta adresinize gönderilmiştir";

            return RedirectToAction(nameof(ForgetPassword));
        }


        public IActionResult ResetPassword(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel request)
        {
            var userId = TempData["userId"];
            var token = TempData["token"];

            if (userId == null || token == null)
            {
                throw new Exception("Bir hata meydana geldi");
            }

            var hasUser = await _userManager.FindByIdAsync(userId.ToString()!);

            if (hasUser == null)
            {
                ModelState.AddModelError(String.Empty, "Kullanıcı bulunamamıştır.");
                return View();
            }

            IdentityResult result = await _userManager.ResetPasswordAsync(hasUser, token.ToString()!, request.Password);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla yenilenmiştir";
            }
            else
            {
                ModelState.AddModelErrorList(result.Errors.Select(x => x.Description).ToList());
            }

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