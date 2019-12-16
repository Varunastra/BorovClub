using BorovClub.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Data
{
    public class BlogService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContext;
        private readonly FriendshipService _friendshipService;
        private readonly ConnectionService _connectionService;
        public BlogService(ApplicationDbContext dbContext, IHttpContextAccessor httpContext, 
            FriendshipService friendshipService, ConnectionService connectionService)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
            _friendshipService = friendshipService;
            _connectionService = connectionService;
        }

        public List<BlogRecord> GetBlogs(string UserName)
        {
            var blogsQuery = _dbContext.UsersBlogs.Where(u => u.User.UserName == UserName).Select(b => b.Blog);
            if (blogsQuery != null)
            {
                return blogsQuery.ToList(); 
            }
            return null;
        }

        public void ApplyBlog(string text)
        {
            var blog = new BlogRecord { Text = text, PublicationDate = DateTime.UtcNow };
            var user = _dbContext.Users.FirstOrDefault(u => u.UserName == _httpContext.HttpContext.User.Identity.Name);
            _dbContext.BlogsRecords.Add(blog);

            var userblogs = new UsersBlogs { Blog = blog, User = user };

            var friends = _friendshipService.GetFriends();

            _connectionService.AlertFriends(friends, userblogs);

            _dbContext.UsersBlogs.Add(userblogs);
            _dbContext.SaveChangesAsync();
        }
    }
}
