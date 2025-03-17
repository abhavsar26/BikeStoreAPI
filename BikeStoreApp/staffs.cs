using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeStoreApp
{
    public class staffs
    {
        public int staff_id { get; set; }

        public string first_name { get; set; } = null!;

        public string last_name { get; set; } = null!;

        public string email { get; set; } = null!;

        public string? phone { get; set; }

        public byte active { get; set; }

        public int store_id { get; set; }

        public int? manager_id { get; set; }

        public string? password { get; set; }

    }
}
