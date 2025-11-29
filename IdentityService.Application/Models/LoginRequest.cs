using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Models
{
    public record LoginRequest(string Email, string Password);
}
