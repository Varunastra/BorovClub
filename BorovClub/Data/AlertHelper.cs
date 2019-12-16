using BorovClub.Models;
using System;
using System.Threading.Tasks;

namespace BorovClub.Data
{
    public class AlertHelper
    {
        public Func<Message, Task> NotifyMessage;
        public Func<Friendship, Task> NotifyRequest;
        public Func<Chat, Task> NotifyChat;
        public Func<UsersBlogs, Task> NotifyBlog;
    }
}
