using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Core.DTO.Auth
{
    public class AuthResultDto
    {
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }

}
