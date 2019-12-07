using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Models
{
    public interface IAlertHelper
    {
        public ApplicationUser Reciever { get; set; }

        public ApplicationUser Sender { get; set; }
    }
}
