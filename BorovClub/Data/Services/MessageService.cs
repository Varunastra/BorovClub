using BorovClub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BorovClub.Data
{
    public class MessageService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly ApplicationDbContext _dbContext;
        private const int Messages_size = 15;
        private Dictionary<string, Message> _messagesDictionary;
        private readonly ConnectionService _connectionService;

        public Dictionary<string, Message> QueryMesssages { get { return _messagesDictionary; } set { _messagesDictionary = value; } }
        public MessageService(IHttpContextAccessor httpContext, ApplicationDbContext dbContext, ConnectionService connectionService)
        {
            _httpContext = httpContext;
            _dbContext = dbContext;
            _messagesDictionary = new Dictionary<string, Message>();
            _connectionService = connectionService;
    }
    public List<Message> ViewMessages(string opponent, int page = 1)
        {
            var user = GetUser();
            var assocUser = _dbContext.Users.FirstOrDefault(u => u.UserName == opponent);

            var messages = _dbContext.Messages.Include(u => u.Sender)
                .Where(m => (m.Reciever == user && m.Sender == assocUser) || (m.Sender == user && m.Reciever == assocUser))
                .OrderByDescending(m => m.When).Skip((page - 1) * Messages_size).Take(Messages_size).ToList();

            messages.Reverse();
            return messages ?? null;
        }

        private ApplicationUser GetUser()
        {
            var userClaim = _httpContext.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userClaim != null)
            {
                return _dbContext.Users.Find(userClaim.Value);
            }
            return null;
        }

        public List<Chat> ViewChats()
        {
            var user = GetUser();

            var chats = _dbContext.Chats.Include(c => c.LastMessage).Include(c => c.Sender)
                .Where(c => c.Reciever == user).ToList();
            return chats;
        }

        public Message CreateMessage(string recieverName, string msgText)
        {
            var user = GetUser();

            var reciever = _dbContext.Users.FirstOrDefault(u => u.UserName == recieverName);
            var message = new Message { Sender = user, Reciever = reciever, Text = msgText, When = DateTime.UtcNow };
            return message;
        }

        public void SendMessage(Message message)
        {
            var user = GetUser();

            _dbContext.Messages.Add(message);
            var chat1 = _dbContext.Chats.FirstOrDefault(c => c.Sender == user && c.Reciever == message.Reciever);
            var chat2 = _dbContext.Chats.FirstOrDefault(c => c.Sender == message.Reciever && c.Reciever == user);

            if (chat1 == null || chat2 == null)
            {
                chat1 = new Chat { Sender = user, Reciever = message.Reciever, LastMessage = message };
                chat2 = new Chat { Sender = message.Reciever, Reciever = user, LastMessage = message };
                _dbContext.Chats.Add(chat1);
                _dbContext.Chats.Add(chat2);
            }

            chat1.LastMessage = message;
            chat2.LastMessage = message;

            _dbContext.SaveChangesAsync();

            _connectionService.Alert(message);
            _connectionService.Alert(chat1);
            QueryMesssages[message.Sender.UserName] = message;
        }

        public List<Message> SetMessagesAsRead(string opponent)
        {
            var user = GetUser();

            var messages = ViewMessages(opponent);

            foreach (var message in messages)
            {
                if (message.Reciever.UserName == user.UserName)
                {
                    if (message.Status == MessageStatus.Unread)
                    {
                        message.Status = MessageStatus.Read;
                        _connectionService.Alert(new Chat { Sender = message.Reciever, Reciever = message.Sender, LastMessage = message });
                        QueryMesssages[message.Sender.UserName] = message;
                    }
                }
            }
           _dbContext.SaveChangesAsync();
           return messages;
        }

        public void SetMessageAsRead(Message message)
        {
            message.Status = MessageStatus.Read;
            _connectionService.Alert(new Chat { Sender = message.Reciever, Reciever = message.Sender, LastMessage = message });
            QueryMesssages[message.Sender.UserName] = message;
        }
    }
}
