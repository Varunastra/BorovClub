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
    public class MessageServiceTest
    {
        private ApplicationDbContext dbContext;
        private MessageService service;
        public IConfiguration Configuration { get; set; } = Mocks.GetConfiguration();

        [TestInitialize]
        public void TestInit()
        {
            dbContext = new ApplicationDbContext(Mocks.GetApplicationDbContextOptions());
            Initializer.InitializeDbForTests(dbContext);
            var httpContext = Mocks.GetMockIHttpContextAccessor(dbContext.Users.FirstOrDefault(u => u.UserName == "Varunastra"));
            service = new MessageService(httpContext, dbContext, new Mock<ConnectionService>().Object);
        }

        [TestMethod]
        public void ShowChats()
        {
            var chats = service.ViewChats();

            Assert.AreEqual(chats.Count, 1);
        }

        [TestMethod]
        public void ShowMessages()
        {
            var messages = service.ViewMessages("Valera");

            Assert.AreEqual(messages.Count, 2);
        }

        [TestMethod]
        public void CreateMessage()
        {
            var message = service.CreateMessage("Valera", "TEST MSG");

            Assert.AreEqual(message.Reciever.UserName, "Valera");
            Assert.AreEqual(message.Sender.UserName, "Varunastra");
            Assert.AreEqual(message.Text, "TEST MSG");
        }

        //[TestMethod]
        //public void SendMessage()
        //{
        //    var message = service.CreateMessage("Valera", "TEST MSG2");
        //    service.SendMessage(message);
        //}

        [TestCleanup]
        public void Cleanup()
        {
            dbContext.Database.EnsureDeleted();
        }
    }
}

