using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorovClub.Models
{
    public class BlogRecord
    {
        public int BlogRecordId { get; set; }
        public string Text { get; set; }
        public DateTime PublicationDate { get; set; }
    }
}
