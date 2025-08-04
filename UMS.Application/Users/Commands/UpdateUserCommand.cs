using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMS.Application.Users.Commands
{
    public class UpdateUserCommand: IRequest<bool>
    {
        public string Id { get; set; }           
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<string>  Roles { get; set; }
    }
}
