using BorovClub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Data
{
    public class AlertService
    {
        public event Action<IAlertHelper, string> OnShow;

        public void ShowMessage(IAlertHelper alert, string message)
        {
            OnShow?.Invoke(alert, message);
        }
    }
}
