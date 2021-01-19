using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CV.Web.ViewModels
{
    public class SignupViewModel : LoginViewModel
    {
        public bool Block { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
