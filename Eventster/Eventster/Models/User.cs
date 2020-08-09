using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Eventster.Models
{
    public class User
    {
        [Required, MaxLength(15), Display(Name = "UserName")]
        public string UserName { get; set; }

        [Required, DataType(DataType.Password), MinLength(2), MaxLength(20), Display(Name = "Password")]
        public string Password { get; set; }
    }
}
