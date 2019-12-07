using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Models
{
    public enum FriendshipStatus
    {
        Approved,
        Declined,
        Pending,
        NotExist
    }

    public partial class Friendship
    {
        //public int FriendshipId { get; set; }
        public string RecieverId { get; set; }

        public virtual ApplicationUser Reciever { get; set; }

        public string SenderId { get; set; }

        public virtual ApplicationUser Sender { get; set; }

        public FriendshipStatus Status { get; set; }

        public DateTime When { get; set; }

    }

    public partial class Friendship : IAlertHelper
    {
    }
}
