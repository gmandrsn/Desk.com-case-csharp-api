using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskApiClient.models
{
    class DeskMessage
    {
        public string direction { get; set; } = "in";
        public string status { get; set; } = "sent";
        public string body { get; set; }
        public string subject { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        // for imported cases, disable rules processing by default
        public bool supress_rules { get; set; } = true;
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime? sent_at { get; set; }
    }
}
