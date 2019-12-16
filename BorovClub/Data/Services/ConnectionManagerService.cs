using BorovClub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BorovClub.Data
{
    public class ConnectionManagerService
    {
        public ConcurrentDictionary<string, ConcurrentDictionary<string, AlertHelper>> Connections { get; set; }

        public ConnectionManagerService()
        {
            Connections = new ConcurrentDictionary<string, ConcurrentDictionary<string, AlertHelper>>();
        }
    }
}
