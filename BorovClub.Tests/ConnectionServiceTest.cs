using BorovClub.Data;
using BorovClub.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Tests
{
    [TestClass]
    public class ConnectionServiceTest
    {
        private ApplicationDbContext dbContext;
        private ConnectionService service;

        private ApplicationUser user1;
        private ApplicationUser user2;

        private ConnectionManagerService connectionManagerService;
        public ConcurrentDictionary<string, ConcurrentDictionary<string, AlertHelper>> Connections { get; set; }

        private TestContext m_testContext;
        public TestContext TestContext

        {

            get { return m_testContext; }

            set { m_testContext = value; }

        }

        [TestInitialize]
        public void TestInit()
        {
            dbContext = new ApplicationDbContext(Mocks.GetApplicationDbContextOptions());
            connectionManagerService = new ConnectionManagerService();
            Connections = connectionManagerService.Connections;
            Initializer.InitializeDbForTests(dbContext);

            user1 = dbContext.Users.FirstOrDefault(u => u.UserName == "Varunastra");
            user2 = dbContext.Users.FirstOrDefault(u => u.UserName == "Valera");

            var httpContext = Mocks.GetMockIHttpContextAccessor(user1);
            service = new ConnectionService(httpContext, connectionManagerService);
        }

        [TestMethod]
        public void AddConnection()
        {
            service.AddConnection();

            Assert.AreEqual(Connections.Count, 1);
            Assert.AreEqual(Connections["Varunastra"].Count, 1);

            var service2 = new ConnectionService(
                Mocks.GetMockIHttpContextAccessor(user2), 
                connectionManagerService);

            service2.AddConnection();

            Assert.AreEqual(Connections.Count, 2);
            Assert.AreEqual(Connections["Valera"].Count, 1);
            Assert.AreEqual(Connections["Varunastra"].Count, 1);

            var service3 = new ConnectionService(
                Mocks.GetMockIHttpContextAccessor(user1), 
                connectionManagerService);

            service3.AddConnection();

            Assert.AreEqual(Connections.Count, 2);
            Assert.AreEqual(Connections["Varunastra"].Count, 2);
            Assert.AreEqual(Connections["Valera"].Count, 1);
        }

        [TestMethod]
        public void AddEvent()
        {
            service.AddConnection();

            service.AddOn<Message>(OnMessageEvent);

            Assert.AreEqual(Connections["Varunastra"].Values.FirstOrDefault().NotifyMessage.GetInvocationList().Length, 1);

            service.AddOn<Friendship>(OnRequestEvent);

            Assert.AreEqual(Connections["Varunastra"].Values.FirstOrDefault().NotifyRequest.GetInvocationList().Length, 1);
        }

        [TestMethod]
        public void Alert()
        {
            service.AddConnection();

            service.AddOn<Message>(OnMessageEvent);
            var message = dbContext.Messages.FirstOrDefault(m => m.Reciever.UserName == "Varunastra" && m.Sender.UserName == "Valera");

            service.Alert(message);

            service.AddOn<Friendship>(OnRequestEvent);
            var chat = dbContext.Friendships.FirstOrDefault(c => c.Reciever.UserName == "Varunastra" && c.Sender.UserName == "Valera");

            service.Alert(chat);
        }

        private Task OnRequestEvent(Friendship request)
        {
            TestContext.WriteLine(request.Status.ToString("g") + " From " + request.Sender.UserName);
            Assert.AreEqual(request.Reciever.UserName, "Varunastra");
            Assert.AreEqual(request.Sender.UserName, "Valera");

            return Task.CompletedTask;
        }

        private Task OnMessageEvent(Message message)
        {
            TestContext.WriteLine(message.Text + " From " + message.Sender.UserName);
            Assert.AreEqual(message.Reciever.UserName, "Varunastra");
            Assert.AreEqual(message.Sender.UserName, "Valera");

            return Task.CompletedTask;
        }

        [TestMethod]
        public void RemoveConnection()
        {
            var serviceManager = new ConnectionManagerService();

            var service2 = new ConnectionService(
                Mocks.GetMockIHttpContextAccessor(user1),
               serviceManager);

            var service3 = new ConnectionService(
               Mocks.GetMockIHttpContextAccessor(user1),
               serviceManager);

            service2.AddConnection();

            service2.RemoveConnection();

            Assert.AreEqual(serviceManager.Connections.Count, 0);

            service2.AddConnection();
            service3.AddConnection();

            service3.RemoveConnection();

            Assert.AreEqual(serviceManager.Connections.Count, 1);
            Assert.AreEqual(serviceManager.Connections["Varunastra"].Count, 1);
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbContext.Database.EnsureDeleted();
        }
    }
}
