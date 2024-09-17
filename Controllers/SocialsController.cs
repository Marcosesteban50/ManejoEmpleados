using ManejoClientes.Data;
using ManejoClientes.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ManejoClientes.Controllers
{
    public class SocialsController : Controller
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly AppDbcontext appDbcontext;

        public SocialsController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            AppDbcontext appDbcontext)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.appDbcontext = appDbcontext;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string mensaje = null)
        {
            if (mensaje != null)
            {
                ViewData["mensaje"] = mensaje;
            }

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ChallengeResult LoginExterno(string proveedor, string urlRetorno = null)
        {
            var urlRedireccion = Url.Action("RegistrarUsuarioExterno", values: new { urlRetorno });
            var propiedades = signInManager.ConfigureExternalAuthenticationProperties(proveedor, urlRedireccion);
            return new ChallengeResult(proveedor, propiedades);
        }

        

        [AllowAnonymous]
        public async Task<IActionResult> RegistrarUsuarioExterno(string urlRetorno = null, string remoteError = null)
        {
            urlRetorno = urlRetorno ?? Url.Content("~/");
            var mensaje = "";

            if (remoteError != null)
            {
                mensaje = $"Error from external provider: {remoteError}";
                if (remoteError == "access_denied")
                {
                    mensaje = "Login was canceled";
                }
                return RedirectToAction("Login", "Usuarios", new { mensaje });
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                mensaje = "Error loading external login information.";
                return RedirectToAction("Login", "Usuarios", new { mensaje });
            }

            var resultadoLoginExterno = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);

            // Ya la cuenta existe
            if (resultadoLoginExterno.Succeeded)
            {
                
                return LocalRedirect(urlRetorno);
            }
            else
            {
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    //Consiguiendo el Email
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                    //Verificando si Existe
                    var user = await userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new IdentityUser { UserName = email, Email = email };
                        var result = await userManager.CreateAsync(user);
                        if (result.Succeeded)
                        {
                            result = await userManager.AddLoginAsync(user, info);
                            if (result.Succeeded)
                            {
                                await signInManager.SignInAsync(user, isPersistent: true);
                                var usuarioPersonalizado = new Usuario
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Email = user.Email,
                                    EmailNormalizado = user.Email.Normalize(),
                                    AspNetUserId = user.Id
                                };
                                await appDbcontext.Usuarios.AddAsync(usuarioPersonalizado);
                                await appDbcontext.SaveChangesAsync();
                                return LocalRedirect(urlRetorno);
                            }
                        }
                        mensaje = "Error creating user.";
                    }
                    else
                    {
                        var result = await userManager.AddLoginAsync(user, info);
                        if (result.Succeeded)
                        {
                            await signInManager.SignInAsync(user, isPersistent: true);
                            return LocalRedirect(urlRetorno);
                        }
                        mensaje = "Error adding external login to existing user.";
                    }
                }
                else
                {
                    mensaje = "Error reading user email from external provider.";
                }
                return RedirectToAction("Login", "Usuarios", new { mensaje });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Lista", "Empleados");
        }
    }
}
