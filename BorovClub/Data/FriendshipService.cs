using BorovClub.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BorovClub.Data
{
    public class FriendshipService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly ApplicationDbContext _dbContext;
        private readonly ApplicationUser user;
        private readonly ILogger<FriendshipService> _logger;

        public Dictionary<string, Friendship> FriendshipsQuery = new Dictionary<string, Friendship>();

        public FriendshipService(IHttpContextAccessor httpContext, ApplicationDbContext dbContext, ILogger<FriendshipService> logger)
        {
            _httpContext = httpContext;
            _dbContext = dbContext;
            _logger = logger;
            var userClaim = _httpContext.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userClaim != null)
            {
                user = _dbContext.Users.Find(userClaim.Value);
                //_dbContext.Entry(user).Collection(u => u.Friendships).Load();
            }
        }

        public async Task<FriendshipStatus> AddFriend(ApplicationUser userToAdd)
        {
            var response = _dbContext.Friendships.Find(userToAdd.Id, user.Id);
            var friendship = new Friendship { Sender = user, Reciever = userToAdd, Status = FriendshipStatus.Pending };
            if (response != null)
            {
                friendship.Status = FriendshipStatus.Approved;
                response.Status = FriendshipStatus.Approved;
            }
            _dbContext.Friendships.Add(friendship);
            await _dbContext.SaveChangesAsync();

            ConnectionManager.Alert(friendship);

            return friendship.Status;
        }

        public IList<ApplicationUser> GetRequests()
        {
            IList<ApplicationUser> requests = null;
            try
            {
                requests = _dbContext.Friendships.Where(f => f.RecieverId == user.Id && f.Status == FriendshipStatus.Pending).Select(f => f.Sender).ToList();
            }
            catch(Exception) {
                _logger.LogInformation("Requests not found");
            }
            return requests;
        }

        public IList<ApplicationUser> GetFriends()
        {
            IList<ApplicationUser> friends = null;
            try
            {
                friends = _dbContext.Friendships.Where(f => f.RecieverId == user.Id && f.Status == FriendshipStatus.Approved).Select(f => f.Sender).ToList();
            }
            catch (Exception)
            {
                _logger.LogInformation("Friends not found");
            }
            return friends;
        }

        public FriendshipStatus GetFriendshipStatus(ApplicationUser userToCheck)
        {
            if (_httpContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var userStatus = _dbContext.Friendships.Find(user.Id, userToCheck.Id);
                if (userStatus != null)
                {
                    return userStatus.Status;
                }
            }
            return FriendshipStatus.NotExist;
        }

        public async Task<FriendshipStatus> RemoveFriend(ApplicationUser userToRemove)
        {
            var requestFromUser = _dbContext.Friendships.Find(user.Id, userToRemove.Id);
            var requestToUser = _dbContext.Friendships.Find(userToRemove.Id, user.Id);
            requestToUser.Status = FriendshipStatus.Pending;
            _dbContext.Friendships.Remove(requestFromUser);
            await _dbContext.SaveChangesAsync();
            return FriendshipStatus.NotExist;
        }

    }
}
