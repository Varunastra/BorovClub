using Microsoft.VisualStudio.TestTools.UnitTesting;
using BorovClub.Models;
using Microsoft.Extensions.Configuration;
using BorovClub.Data;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System;

namespace BorovClub.Tests
{
    [TestClass]
    public class AccountServiceTest
    {
        private ApplicationDbContext dbContext;
        private AccountService service;
        public IConfiguration Configuration { get; set; } = Mocks.GetConfiguration();

        [TestInitialize]
        public void TestInit()
        {
            dbContext = new ApplicationDbContext(Mocks.GetApplicationDbContextOptions());
            Initializer.InitializeDbForTests(dbContext);
            var userManager = Mocks.GetMockUserManager(dbContext.Users.ToList());
            service = new AccountService(
                userManager, 
                Mocks.GetMockIHttpContextAccessor(dbContext.Users.FirstOrDefault(u => u.UserName == "Varunastra")), 
                Configuration, dbContext
            );
        }

        [TestMethod]

        public async Task Login()
        {
            UserLogin user = new UserLogin { UserName = "Valera", Password = "112112Vaal!" };

            var token = await service.UserLogin(user);

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidAudience = Configuration["JWT:Issuer"],
                ValidIssuer = Configuration["JWT:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Key"]))
            };

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            var test = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

            Assert.AreEqual("Valera", principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value);
        }

        [TestMethod]
        public void GetUser()
        {
            var user = service.GetUser();

            var user2 = service.GetUser("Valera");

            Assert.AreEqual(user.UserName, "Varunastra");
            Assert.AreEqual(user2.UserName, "Valera");
        }

        [TestMethod]
        public void TotpValidation()
        {
            string secret = "TESTQ====";

            var totp = new Totp(secret);
            var totp2 = new Totp(secret);

            var expiryTimeDiff = (totp.ExpiryUtc - DateTime.UtcNow).TotalSeconds;

            Assert.IsTrue(expiryTimeDiff <= 30 && expiryTimeDiff >= 8);
            Assert.IsFalse(totp.IsExpired);

            Assert.AreEqual(totp.AuthenticationCode, totp2.AuthenticationCode);
        }

        [TestCleanup]

        public void Cleanup()
        {
            dbContext.Database.EnsureDeleted();
        }

    }
}
