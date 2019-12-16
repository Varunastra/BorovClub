using BorovClub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Data
{
    public class AlertService
    {
        public event Action<IMessageSender, AlertType, string> OnShow;

        public void ShowMessage(IMessageSender alert, AlertType alertType, string message)
        {
            OnShow?.Invoke(alert, alertType, message);
        }
    }
}
