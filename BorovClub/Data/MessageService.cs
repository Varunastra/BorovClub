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
        private readonly ApplicationUser user;
        public MessageService(IHttpContextAccessor httpContext, ApplicationDbContext dbContext)
        {
            _httpContext = httpContext;
            _dbContext = dbContext;
            var userClaim = _httpContext.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userClaim != null)
            {
                user = _dbContext.Users.Find(userClaim.Value);
                //_dbContext.Entry(user).Collection(u => u.Friendships).Load();
            }
        }

        public ICollection<Message> viewMessages()
        {
            var messages = _dbContext.Messages.Include(u => u.Sender)
                .Where(m => m.Reciever == user || m.Sender == user).ToList();
            return messages ?? null;
        }

        public void addMessage(string recieverName, string msgText)
        {
            var reciever = _dbContext.Users.FirstOrDefault(u => u.UserName == recieverName);
            var message = new Message { Sender = user, Reciever = reciever, Text = msgText };
            _dbContext.Messages.Add(message);
            _dbContext.SaveChanges();
        }
    }
}
