using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Models
{
    public partial class UsersBlogs
    {
        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }
        public virtual BlogRecord Blog { get; set; }
        public int BlogId { get; set; }
    }

    public partial class UsersBlogs: IMessageSender
    {
        public ApplicationUser Sender { get { return User; }  set { User = value; } }
        public string SenderId { get { return User.Id; } set { User.Id = value; } }
    }
}
