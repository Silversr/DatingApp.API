using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required] // dataAnnotations
        public string Username { get; set; }

        [Required]
        [System.ComponentModel.DataAnnotations.StringLength(8,MinimumLength =4,ErrorMessage = "You must specify password between 4 and 8")]
        public string Password { get; set; }
    }
}
