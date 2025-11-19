using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Core.DTO.Auth
{
    public class RegisterDto
    {
        [Required][EmailAddress] public string Email { get; set; } = null!;
        [Required][MinLength(6)] public string Password { get; set; } = null!;
    }
}