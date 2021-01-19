using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CV.Web.ViewModels
{
    public class DashboardViewModel
    {
        [Required]
        public string Email { get; set; }

        [DataType(DataType.MultilineText)]
        public string Nav { get; set; }

        [Required]
        [DataType(DataType.Html)]
        public string Body { get; set; }

        public string TinyMCEApi { get; set; }

        [Display(Name = "Use Raw Html")]
        public bool UseRawHtml { get; set; }
    }
}
