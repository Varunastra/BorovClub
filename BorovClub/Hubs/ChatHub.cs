using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using BorovClub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using BorovClub.Data;

namespace SamaraClub.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ApplicationDbContext _dbContext;
        private readonly Task<ApplicationUser> _userTask;
        public ChatHub(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContext, ApplicationDbContext dbContext, MessageService messageService)
        {
            _userManager = userManager;
            _httpContext = httpContext;
            _dbContext = dbContext;
            _userTask = _userManager.GetUserAsync(_httpContext.HttpContext.User);
        }

        private async Task setUserOnline(bool online)
        {
            var _user = await _userTask;
            _user.Online = online;
            await _userManager.UpdateAsync(_user);
        }
        public override async Task OnConnectedAsync()
        {
            string userName = Context.User.Identity.Name;
            string connectionId = Context.ConnectionId;

            await Groups.AddToGroupAsync(connectionId, userName);
            await setUserOnline(true);

            await base.OnConnectedAsync();
        }

        public async void SendMessageToUser(string recieverName, string messageText)
        {
            string senderName = Context.User.Identity.Name;
            var reciever = _dbContext.Users.FirstOrDefault(u => u.UserName == recieverName);
            var user = await _userTask;
            var message  = new Message { Sender = user, Reciever = reciever, Text = messageText, When = DateTime.UtcNow };
            _dbContext.Messages.Add(message);
            _dbContext.SaveChanges();
            await Clients.Group(recieverName).SendAsync("broadcastMessage", senderName, messageText);
            await Clients.Group(senderName).SendAsync("broadcastMessage", senderName, messageText);
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            await setUserOnline(false);
            await base.OnDisconnectedAsync(exception);
        }
    }

}