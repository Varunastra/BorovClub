using BorovClub.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorovClub.Tests
{
    [TestClass]
    public class FriendshipServiceTest
    {
        private ApplicationDbContext dbContext;
        private FriendshipService service;
        public IConfiguration Configuration { get; set; } = Mocks.GetConfiguration();

        [TestInitialize]
        public void TestInit()
        {
            dbContext = new ApplicationDbContext(Mocks.GetApplicationDbContextOptions());
            Initializer.InitializeDbForTests(dbContext);
            var httpContext = Mocks.GetMockIHttpContextAccessor(dbContext.Users.FirstOrDefault(u => u.UserName == "Varunastra"));
            service = new FriendshipService(httpContext, dbContext,
                new Mock<ILogger<FriendshipService>>().Object, new Mock<ConnectionService>().Object);
        }

        [TestMethod]

        public void GetUserFriends()
        {
            var friends = service.GetFriends();

            Assert.AreEqual(friends.Count, 1);
        }

        [TestMethod]
        public void GetRequests()
        {
            var requests = service.GetRequests();

            Assert.AreEqual(requests.Count, 0);
        }

        [TestMethod]
        public async Task RemoveFriend()
        {
            var user = dbContext.Users.FirstOrDefault(u => u.UserName == "Valera");

            await service.RemoveFriend(user);

            var friends = service.GetFriends();

            Assert.AreEqual(friends.Count, 0);
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbContext.Database.EnsureDeleted();
        }
    }
}
