using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Models
{
    public partial class Chat
    {
        public string SenderId { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        public string RecieverId { get; set; }
        public virtual ApplicationUser Reciever { get; set; }
        public int LastMessageId { get; set; }
        public virtual Message LastMessage { get; set; }
    }

    public partial class Chat: IMessageSender, IMessageReciever
    {

    }
}
