using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Application.DTO_s;

namespace UMS.Application.Users.Commands
{
    public class LoginCommand:IRequest<LoginResponseDTO>
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
