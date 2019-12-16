using BorovClub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Data
{
    public class ConnectionService
    {
        private readonly IHttpContextAccessor _context;
        private readonly ConnectionManagerService _connectionManagerService;

        public string ConnectionId { get; set; }
        public string UserName { get; set; }

        public ConcurrentDictionary<string, ConcurrentDictionary<string, AlertHelper>> Connections { get; set; }

        public ConnectionService(IHttpContextAccessor context, ConnectionManagerService connectionManagerService)
        {
            _context = context;
            _connectionManagerService = connectionManagerService;
            //Connections = _connectionManagerService.Connections;
            //ConnectionId = _context.HttpContext.Request.Query["id"];
        }

        private string GetConnectionId()
        {
            return _context.HttpContext.Request.Query["id"];
        }

        private string GetUserName()
        {
            return _context.HttpContext.User.Identity.Name;
        }

        public void AddConnection()
        {
            Connections = _connectionManagerService.Connections;

            ConnectionId = GetConnectionId();
            UserName = GetUserName();

            if (!Connections.TryGetValue(UserName, out ConcurrentDictionary<string, AlertHelper> user))
            {
                var currentUser = new ConcurrentDictionary<string, AlertHelper>();
                currentUser.TryAdd(ConnectionId, new AlertHelper());
                Connections.TryAdd(UserName, currentUser);
            }
            else
            {
                var currentUser = Connections[UserName];
                currentUser.TryAdd(ConnectionId, new AlertHelper());
            }
        }

        public void RemoveConnection()
        {
            if (Connections.TryGetValue(UserName, out ConcurrentDictionary<string, AlertHelper> user))
            {
                Connections[UserName].TryRemove(ConnectionId, out AlertHelper helper);
                if (Connections[UserName].IsEmpty)
                {
                    Connections.TryRemove(UserName, out user);
                }
            }
        }
        public bool Alert<T>(T alert) where T : IMessageReciever, IMessageSender
        {
            ConcurrentDictionary<string, AlertHelper> user = new ConcurrentDictionary<string, AlertHelper>();

            var @invokeMapper = new Dictionary<Type, Action<string>>
                {
                    {
                        typeof(Message), (connectionId) => user[connectionId].NotifyMessage?.DynamicInvoke(alert)
                    },
                    {
                        typeof(Friendship), (connectionId) => user[connectionId].NotifyRequest?.DynamicInvoke(alert)
                    },
                    {
                        typeof(Chat), (connectionId) => user[connectionId].NotifyChat?.DynamicInvoke(alert)
                    }
             };

            var invokeAlert = invokeMapper[typeof(T)];

            if (Connections.TryGetValue(alert.Reciever.UserName, out user))
            {
                foreach (var connection in user)
                {
                    invokeAlert(connection.Key);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AddOn<T>(Func<T, Task> handle)
        {
            ConcurrentDictionary<string, AlertHelper> user = new ConcurrentDictionary<string, AlertHelper>();

            var @handleMapper = new Dictionary<Type, Action>
            {
                {
                    typeof(Message), () => user[ConnectionId].NotifyMessage += (dynamic) handle
                },
                {
                    typeof(Friendship), () => user[ConnectionId].NotifyRequest += (dynamic) handle
                },
                {
                    typeof(Chat), () => user[ConnectionId].NotifyChat += (dynamic)handle
                },
                {
                    typeof(UsersBlogs), () => user[ConnectionId].NotifyBlog += (dynamic)handle
                }
            };

            var addHandle = handleMapper[typeof(T)];

            try
            {
                if (Connections.TryGetValue(UserName, out user))
                {
                    addHandle();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RemoveOn<T>(Func<T, Task> handle)
        {
            ConcurrentDictionary<string, AlertHelper> user = new ConcurrentDictionary<string, AlertHelper>();

            var @handleMapper = new Dictionary<Type, Action>
                {
                    {
                        typeof(Message), () => user[ConnectionId].NotifyMessage -= (dynamic) handle
                    },
                    {
                        typeof(Friendship), () => user[ConnectionId].NotifyRequest -= (dynamic) handle
                    },
                    {
                        typeof(Chat), () => user[ConnectionId].NotifyChat -= (dynamic) handle
                    },
                    { 
                        typeof(UsersBlogs), () => user[ConnectionId].NotifyBlog -= (dynamic) handle
                    }
             };

            var removeHandle = handleMapper[typeof(T)];

            try
            {
                if (Connections.TryGetValue(UserName, out user))
                {
                    removeHandle();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void AlertFriends(IList<ApplicationUser> friends, UsersBlogs record)
        { 
            foreach (var friend in friends)
            {
                if (Connections.TryGetValue(friend.UserName, out var connectionIds))
                {
                    foreach (var connectionId in connectionIds)
                    {
                        connectionId.Value.NotifyBlog?.Invoke(record);
                    }
                }
            }
        }
    }
}
