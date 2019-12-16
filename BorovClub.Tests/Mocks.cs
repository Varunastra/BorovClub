using BorovClub.Data;
using BorovClub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace BorovClub.Tests
{
    static class Mocks
    {
        public static UserManager<ApplicationUser> GetMockUserManager(List<ApplicationUser> users)
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((string x) => users.FirstOrDefault(u => u.UserName == x));
            userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((string x) => users.FirstOrDefault(u => u.Email == x));
            userManager.SetupGet(x => x.Users).Returns(GetQueryableMockDbSet(users));
            userManager.Setup(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(
                (ApplicationUser u, string pw) => true);
            return userManager.Object;
        }

        public static IHttpContextAccessor GetMockIHttpContextAccessor(ApplicationUser user = null)
        {
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var ConnectionId = Guid.NewGuid().ToString();

            var httpContext = new DefaultHttpContext();

            if (user != null)
            {
                ClaimsPrincipal claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName) }
                ));
                httpContext.User = claims;
            }

            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues> { { "id", ConnectionId } });

            httpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);
            
            return httpContextAccessor.Object;
        }

        private static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));

            return dbSet.Object;
        }

        public static DbContextOptions<ApplicationDbContext> GetApplicationDbContextOptions()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseInMemoryDatabase(databaseName: "BorovClubTest")
               .Options;
            return options;
        }

        public static IConfiguration GetConfiguration()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            IConfiguration configuration = configurationBuilder.Build();
            return configuration;
        }
    }
}
