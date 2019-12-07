using BorovClub.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BorovClub.Data
{
    public static class ConnectionManager
    {
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, AlertHelper>> Connections { get; set; } =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, AlertHelper>>();

        public static void AddConnection(string Username, string ConnectionId)
        {
            if (!Connections.TryGetValue(Username, out ConcurrentDictionary<string, AlertHelper> user))
            {
                var currentUser = new ConcurrentDictionary<string, AlertHelper>();
                currentUser.TryAdd(ConnectionId, new AlertHelper());
                Connections.TryAdd(Username, currentUser);
            }
            else
            {
                var currentUser = Connections[Username];
                currentUser.TryAdd(ConnectionId, new AlertHelper());
            }
        }

        public static void RemoveConnection(string Username, string ConnectionId)
        {
            if (!Connections.TryGetValue(Username, out ConcurrentDictionary<string, AlertHelper> user))
            {
                Connections[Username].TryRemove(ConnectionId, out AlertHelper helper);
                if (Connections[Username].IsEmpty)
                {
                    Connections.TryRemove(Username, out user);
                }
            }
        }
        public static bool Alert<T>(T alert) where T: IAlertHelper
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

        public static bool AddOn<T>(string Username, string ConnectionId, Func<T, Task> handle)
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
                }
            };

            var addHandle = handleMapper[typeof(T)];

            try
            {
                if (Connections.TryGetValue(Username, out user))
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

        public static bool RemoveOn<T>(string Username, string ConnectionId, Func<T, Task> handle)
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
                    }
             };

            var removeHandle = handleMapper[typeof(T)];

            try
            {
                if (Connections.TryGetValue(Username, out user))
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


        public static ConcurrentDictionary<string, ConcurrentDictionary<string, AlertHelper>> GetConnections()
        {
            return Connections;
        }
    }
}
