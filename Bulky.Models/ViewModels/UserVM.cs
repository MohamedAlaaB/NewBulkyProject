using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
    public class UserVM
    {
        [ValidateNever]
        public Appuser user { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> roles { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> companies { get; set; }
       


    }
}
