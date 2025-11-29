using IdentityService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Contracts
{
    public interface IJwtTokenGenerators
    {
        string GenerateToken(AppUser user);
    }
}
