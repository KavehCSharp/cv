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
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Display(Description = "leave it blank if you don't want to change password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.MultilineText)]
        public string Nav { get; set; }

        [DataType(DataType.Html)]
        [Display(Description = "this is one page CV, add some Anchor links (Insert Menu/Anchor... and set ID)")]
        public string Body { get; set; }

        public string TinyMCEApi { get; set; }

        [Display(Name = "i don't want this!")]
        public bool UseRawHtml { get; set; }

        public string CV { get; set; }

        public string Image { get; set; }
    }
}
