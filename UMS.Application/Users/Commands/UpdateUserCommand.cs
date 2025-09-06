using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMS.Application.Users.Commands
{
    public class UpdateUserCommand: IRequest<bool>
    {
        public Guid Id { get; set; }           
        public string Email { get; set; }
        public string UserName { get; set; }
        public string[]  Roles { get; set; }
        public IFormFile? File { get; set; }
    }
}
