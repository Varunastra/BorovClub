using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Models.Toasts
{
    public class ToastInfo
    {
        public string Heading { get; set; }
        public string Avatar { get; set; }
        public string SenderURL { get; set; }
        public string Message { get; set; }
        public AlertType AlertType { get; set; }
    }

    public class ToastInstance
    {
        public Guid Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public ToastInfo ToastInfo { get; set; }
    }
}
