using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskApiClient.models
{
    class DeskCustomer
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string uid { get; set; }
        public int external_id { get; set; }
        public IDictionary<string, string> custom_fields { get; set; }
    }
}
