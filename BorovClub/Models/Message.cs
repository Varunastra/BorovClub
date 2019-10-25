using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public string SenderId { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        public string Text { get; set; }
        public DateTime When { get; set; }
        public string RecieverId { get; set; }
        public virtual ApplicationUser Reciever { get; set; }
    }
}
