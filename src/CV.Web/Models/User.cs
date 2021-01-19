using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CV.Web.Models
{
    public class User : Entity
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string Nav { get; set; }

        public string Body { get; set; }

        public string Image { get; set; }

        public string CV { get; set; }

        public string TinyMCEApi { get; set; }
    }
}
