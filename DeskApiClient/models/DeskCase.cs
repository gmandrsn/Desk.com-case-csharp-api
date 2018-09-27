using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskApiClient.models
{
    class DeskCase
    {
        public int id { get; set; }
        //public int external_id { get; set; }
        public string subject { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? changed_at { get; set; }
        public int priority { get; set; } = 4;
        public string type { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public string[] labels { get; set; }
        public DeskCustomer customer { get; set; }
        public DeskMessage message { get; set; }

        public IDictionary<string, string> custom_fields { get; set; }

    }
}
