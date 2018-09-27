using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskApiClient.models
{
    class DeskNote
    {
        public int id { get; set; }
        public string body { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        // for imported notes, disable rules processing by default
        public bool supress_rules { get; set; } = true;
    }
}
