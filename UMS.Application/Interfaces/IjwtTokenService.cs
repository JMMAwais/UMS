using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Application.DTO_s;

namespace UMS.Application.Interfaces
{
    public interface IjwtTokenService
    {
        Task<string> GenerateToken(string userId,string email,string role);
    }
}
