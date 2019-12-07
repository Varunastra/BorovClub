using BorovClub.Data;
using BorovClub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using QRCoder;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace BorovClub
{
    public enum AuthorizeMethod
    {
        Normal,
        TwoFactor,
        Error
    }
    public class AccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        public string OauthCode { get; set; }
   
        public AccountService(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<bool> UserRegister(UserRegister model)
        {
            var user = new ApplicationUser()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                AvatarPath = "/avatars/default.png"
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                return true;
            }
            return false;
        }

        public string CreateJsonWebToken(ApplicationUser user)
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

        private async Task<bool> VerifyModel(UserLogin model)
        {
            ApplicationUser user;
            if (model.Email != null)
            {
                user = await _userManager.FindByEmailAsync(model.Email);
            }
            else
            {
                user = GetUser(model.UserName);
            }
            var passwordVerified = await _userManager.CheckPasswordAsync(user, model.Password);
            if (passwordVerified && user != null)
            {
                return true;
            }
            return false;
        }

        public async Task<string> UserLogin(UserLogin model)
        {
            ApplicationUser user;
            if (await VerifyModel(model))
            {
                if (model.UserName != null)
                {
                    user = GetUser(model.UserName);
                }
                else
                {
                    user = await GetUserByEmail(model.Email);
                }
                return CreateJsonWebToken(user);
            }
            return null;
        }

        public async Task<AuthorizeMethod> GetAuthorizeMethod(UserLogin model)
        {
            var verified = await VerifyModel(model);
            if (verified)
            {
                var user = GetUser(model.UserName);
                if (user.TwoFactorEnabled)
                {
                    return AuthorizeMethod.TwoFactor;
                }
                return AuthorizeMethod.Normal;
            }
            else
            {
                return AuthorizeMethod.Error;
            }
        }

        public ApplicationUser GetUser(string UserName = null)
        {
            ApplicationUser user;
            if (String.IsNullOrEmpty(UserName))
            {
                user = _userManager.Users.FirstOrDefault(u => u.UserName == _httpContextAccessor.HttpContext.User.Identity.Name);
            }
            else {
                user = _userManager.Users.FirstOrDefault(u => u.UserName == UserName);
            }
            return user;
        }

        public async Task<bool> ChangeEmail(string Email)
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (user != null)
            {
                user.Email = Email;
                await _userManager.UpdateAsync(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ChangePassword(string OldPassword, string NewPassowrd)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.UserName == _httpContextAccessor.HttpContext.User.Identity.Name);
            var result = await _userManager.ChangePasswordAsync(user, OldPassword, NewPassowrd);
            if (result == IdentityResult.Success)
            {
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<string> GenerateQRCode(string secret)
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            var buffer = Base32.ToByteArray(secret);
            var secretBase32 = Base32.ToString(buffer);
            string payload = $"otpauth://totp/BorovClub:{user.Email}?secret={secret}&issuer=BorovClub&algorithm=SHA1&digits=6&period=30";
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            string qrCodeImageAsBase64 = qrCode.GetGraphic(20);

            user.TwoFactorEnabled = true;
            user.TotpSecret = secretBase32;
            await _userManager.UpdateAsync(user);
            await _dbContext.SaveChangesAsync();

            return qrCodeImageAsBase64;
        }

        public bool ValidateTotp(string code, ApplicationUser user)
        {
            Totp totp = new Totp(user.TotpSecret);
            Console.WriteLine("CODE:" + totp.AuthenticationCode);
            if (totp.AuthenticationCode == code)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> GetTwoFactorStatus()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            return user.TwoFactorEnabled;
        }

        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
    }
}
