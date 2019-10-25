using Microsoft.AspNetCore.Identity;
using BorovClub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;

namespace BorovClub
{
    public class AccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        public AccountService(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<bool> userRegister(UserRegister model)
        {
            var user = new ApplicationUser()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                return true;
            }
            return false;
        }

        public async Task<string> userLogin(UserLogin model)
        {
            IdentityModelEventSource.ShowPII = true;
            var user = await _userManager.FindByNameAsync(model.UserName);
            var passwordVerified = await _userManager.CheckPasswordAsync(user, model.Password);
            if (user != null && passwordVerified)
            {
                var claims = new[] {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiry = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JWT:ExpiryInMinutes"]));
                var token = new JwtSecurityToken(
                    _configuration["JWT:Issuer"],
                    _configuration["JWT:Audience"],
                    claims,
                    expires: expiry,
                    signingCredentials: creds
                 );
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            return null;
        }

        public async Task<bool> userLogout()
        {
            var currentUser = _httpContextAccessor.HttpContext.User;
            if (_signInManager.IsSignedIn(currentUser))
            {
                await _signInManager.SignOutAsync();
                return true;
            }
            return false;
        }

        public ApplicationUser getUser(string user)
        {
            var userList = from u in _userManager.Users where u.UserName == user select u;
            if (userList.Count() > 0)
            {
                return userList.First();
            }
            return null;
        }
    }
}
