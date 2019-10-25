using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string AvatarPath { get; set; }
        public string About { get; set; }
        public string City { get; set; }
        public bool Gender { get; set; }
        public string Country { get; set; }
        public bool Online { get; set; }

        //public virtual ICollection<Message> Messages { get; set; }
    }

    //public partial class ApplicationUser
    //{
    //    public ICollection<ApplicationUser> getUserFriends()
    //    {
    //        return Friendships.Where(f => f.Status == FriendshipStatus.Approved).Select(f => f.Sender).ToList();
    //    }
    //}

}
