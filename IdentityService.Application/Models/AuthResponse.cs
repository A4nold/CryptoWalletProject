using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Models
{
    public record AuthResponse(string AccessToken, string UserId, string Email);

}
