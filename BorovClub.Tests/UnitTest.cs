using Microsoft.VisualStudio.TestTools.UnitTesting;
using BorovClub.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.EntityFrameworkCore;
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
    public class UnitTest
    {
        private Mock<IHttpContextAccessor> _httpContextAccessor;
        private ServiceProvider _serviceProvider;

        private Mock<UserManager<ApplicationUser>> _userManager;

        private List<ApplicationUser> fakeUsers = new List<ApplicationUser>
        {
            new ApplicationUser {
                UserName = "Varunastra",
                Email = "dyadyavaleras@gmail.com",
                FirstName = "Konstantsin",
                LastName = "Kolasau",
                TotpSecret = "TESQ===="
            },

            new ApplicationUser
            {
                UserName = "Valera",
                Email = "valera@yandex.by",
                FirstName = "Valera",
                LastName = "Kimovsk" 
        }
        };
        private List<Friendship> fakeFriendships = new List<Friendship>();
        private List<Message> fakeMessages = new List<Message>();
        private List<Chat> fakeChats = new List<Chat>();

        private ApplicationUser mainUser;
        public IConfiguration Configuration { get; set; }

        [TestInitialize]
        public void TestInit()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var startup = new Startup(config);

            Configuration = startup.Configuration;

            var services = new ServiceCollection();

            startup.ConfigureServices(services);

            _httpContextAccessor = new Mock<IHttpContextAccessor>();

            services.AddDbContext<ApplicationDbContext>(opt => opt.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
;
            var store = new Mock<IUserStore<ApplicationUser>>();

            _userManager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            var signInManager = new Mock<SignInManager<ApplicationUser>>(_userManager.Object, _httpContextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);

            services.AddSingleton<IConfiguration>(provider => config);
            services.AddSingleton(_httpContextAccessor.Object);
            services.AddSingleton(_userManager.Object);
            services.AddSingleton(signInManager.Object);

            services.AddScoped<AccountService>();
            services.AddScoped<MessageService>();
            services.AddScoped<FriendshipService>();

            fakeFriendships.Add(new Friendship
            {
                Sender = fakeUsers[0],
                Reciever = fakeUsers[1],
                Status = FriendshipStatus.Approved
            });

            fakeFriendships.Add(new Friendship
            {
                Sender = fakeUsers[1],
                Reciever = fakeUsers[0],
                Status = FriendshipStatus.Approved
            });

            fakeMessages.Add(new Message
            {
                Reciever = fakeUsers[0],
                Sender = fakeUsers[1],
                Text = "testB",
                When = DateTime.UtcNow
            });

            fakeMessages.Add(new Message
            {
                Reciever = fakeUsers[1],
                Sender = fakeUsers[0],
                Text = "testA",
                When = DateTime.UtcNow
            });

            fakeChats.Add(new Chat
            {
                LastMessage = fakeMessages[0],
                Sender = fakeMessages[0].Sender,
                Reciever = fakeMessages[0].Reciever
            });

            fakeChats.Add(new Chat
            {
                LastMessage = fakeMessages[0],
                Sender = fakeMessages[0].Sender,
                Reciever = fakeMessages[0].Reciever
            });


            _serviceProvider = services.BuildServiceProvider();

            var dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager =  _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            dbContext.Users.Add(fakeUsers[0]);
            dbContext.Users.Add(fakeUsers[1]);

            dbContext.Friendships.Add(fakeFriendships[0]);
            dbContext.Friendships.Add(fakeFriendships[1]);

            dbContext.SaveChanges();

            mainUser = dbContext.Users.FirstOrDefault(u => u.UserName == "Varunastra");

            ClaimsPrincipal claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.NameIdentifier, mainUser.Id),
                    new Claim(ClaimTypes.Name, mainUser.UserName) }
            ));

            _httpContextAccessor.SetupGet(x => x.HttpContext).Returns(new DefaultHttpContext { User = claims });
            MockUserManager();
        }

        private void MockUserManager()
        {
            var dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            _userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync((ApplicationUser u, string pswd) =>
                {
                var checkExist = dbContext.Users.FirstOrDefault(x => x.UserName == u.UserName);

                if (checkExist == null)
                {
                    dbContext.Users.Add(u);
                    dbContext.SaveChanges();
                    return IdentityResult.Success;
                }
                return IdentityResult.Failed();
            });
            _userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((string x) => dbContext.Users.FirstOrDefault(u => u.UserName == x));
            _userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((string x) => dbContext.Users.FirstOrDefault(u => u.Email == x));
            _userManager.SetupGet(x => x.Users).Returns(dbContext.Users);
            _userManager.Setup(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(
                (ApplicationUser u, string pw) => true);
        }

        [TestMethod]
        public async Task Register()
        {
            UserRegister user = new UserRegister { 
                UserName = "TestA", Password = "112112Vaal!", FirstName = "Putinds", LastName = "DDSDsds", Email = "test@yandex.by"
            };

            var service = _serviceProvider.GetRequiredService<AccountService>();

            var registerStatus = await service.UserRegister(user);
            Assert.IsTrue(registerStatus);

            registerStatus = await service.UserRegister(user);
            Assert.IsFalse(registerStatus);
        }

        [TestMethod]

        public async Task Login()
        {
            UserLogin user = new UserLogin { UserName = "Valera", Password = "112112Vaal!" };

            var service = _serviceProvider.GetRequiredService<AccountService>();

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
            var service = _serviceProvider.GetRequiredService<AccountService>();

            var user = service.GetUser();

            var user2 = service.GetUser("Valera");

            Assert.AreEqual(user.UserName, "Varunastra");
            Assert.AreEqual(user2.UserName, "Valera");
        }

        [TestMethod]
        public void TotpValidation()
        {
            var accountService = _serviceProvider.GetRequiredService<AccountService>();
            var dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            var user = dbContext.Users.FirstOrDefault(u => u.UserName == "Varunastra");

            var secret = user.TotpSecret;

            Totp totp = new Totp(secret);

            var totpValidationResult = accountService.ValidateTotp("173650", user);

            Assert.IsFalse(totpValidationResult);

            var totpValidationResult2 = accountService.ValidateTotp(totp.AuthenticationCode, user);

            Assert.IsTrue(totpValidationResult2);
        }

        [TestMethod]

        public void GetUserFriends()
        {
            var service = _serviceProvider.GetRequiredService<FriendshipService>();

            var friends = service.GetFriends();

            Assert.AreEqual(friends.Count, 1);
        }

        [TestMethod]
        public void GetRequests()
        {
            var service = _serviceProvider.GetRequiredService<FriendshipService>();

            var requests = service.GetRequests();

            Assert.AreEqual(requests.Count, 1);
        }

        [TestMethod]
        public async Task RemoveFriend()
        {
            var service = _serviceProvider.GetRequiredService<FriendshipService>();
            var dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

            var user = dbContext.Users.FirstOrDefault(u => u.UserName == "Valera");

            await service.RemoveFriend(user);

            var friends = service.GetFriends();

            Assert.AreEqual(friends.Count, 0);
        }

        [TestMethod]
        public void ShowChats()
        {
            var service = _serviceProvider.GetRequiredService<MessageService>();

            var chats = service.ViewChats();

            Assert.AreEqual(chats.Count, 1);
        }

        [TestMethod]
        public void ShowMessages()
        {
            var service = _serviceProvider.GetRequiredService<MessageService>();

            var messages = service.ViewMessages("Valera");

            Assert.AreEqual(messages.Count, 15);
        }

        [TestMethod]

        public void CreateMessage()
        {
            var service = _serviceProvider.GetRequiredService<MessageService>();

            var message = service.CreateMessage("Valera", "TEST MSG");

            Assert.AreEqual(message.Reciever.UserName, "Valera");
            Assert.AreEqual(message.Sender.UserName, "Varunastra");
            Assert.AreEqual(message.Text, "TEST MSG");
        }
    }
}
