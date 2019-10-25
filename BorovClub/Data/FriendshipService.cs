using BorovClub.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BorovClub.Data
{
    public class FriendshipService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly ApplicationDbContext _dbContext;
        private readonly ApplicationUser user;

        public FriendshipService(IHttpContextAccessor httpContext, ApplicationDbContext dbContext)
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

        public void addFriend(ApplicationUser userToAdd)
        {
            var response = _dbContext.Friendships.Find(userToAdd.Id, user.Id);
            var friendship = new Friendship { Sender = user, Reciever = userToAdd, Status = FriendshipStatus.Pending };
            if (response != null)
            {
                friendship.Status = FriendshipStatus.Approved;
                response.Status = FriendshipStatus.Approved;
            }
            _dbContext.Friendships.Add(friendship);
            _dbContext.SaveChanges();
        }

        public ICollection<ApplicationUser> getRequests()
        {
            var requests = _dbContext.Friendships.Where(f => f.RecieverId == user.Id && f.Status == FriendshipStatus.Pending).Select(f => f.Sender);
            return requests.ToList();
        }

        public ICollection<ApplicationUser> getFriends()
        {
            var friends = _dbContext.Friendships.Where(f => f.RecieverId == user.Id && f.Status == FriendshipStatus.Approved).Select(f => f.Sender);
            return friends.ToList();
        }

        public FriendshipStatus getFriendshipStatus(ApplicationUser userToCheck)
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

        public void removeFriend(ApplicationUser userToRemove)
        {
            var requestFromUser = _dbContext.Friendships.Find(user.Id, userToRemove.Id);
            var requestToUser = _dbContext.Friendships.Find(userToRemove.Id, user.Id);
            requestToUser.Status = FriendshipStatus.Pending;
            _dbContext.Friendships.Remove(requestFromUser);
            _dbContext.SaveChanges();
        }

    }
}
