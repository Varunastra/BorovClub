using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Models
{
    public interface IMessageReciever
    {
        public ApplicationUser Reciever { get; set; }
    }
}
