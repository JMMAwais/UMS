using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMS.Application.Users.Commands
{
    public class UploadProfilePictureCommand: IRequest<string>
    {
        public Guid UserId { get; set; }
        public IFormFile File { get; set; }
        public string Folder { get; set; }
    }
}
