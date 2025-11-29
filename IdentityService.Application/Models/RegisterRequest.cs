using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Models
{
    public record RegisterRequest(string Email, string Password, string Username);
}
