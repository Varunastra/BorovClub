using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Data
{
    public class ConnectionService
    {
        private readonly IHttpContextAccessor _context;

        public ConnectionService(IHttpContextAccessor context)
        {
            _context = context;
        }

        public string GetConnectionId()
        {
            var id = _context.HttpContext.Request.Query["id"];
            return id;
        }
    }
}
