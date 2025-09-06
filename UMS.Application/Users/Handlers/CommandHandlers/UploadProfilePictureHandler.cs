using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Application.Interfaces;
using UMS.Application.Users.Commands;

namespace UMS.Application.Users.Handlers.CommandHandlers
{
    public class UploadProfilePictureHandler : IRequestHandler<UploadProfilePictureCommand, string>
    {

        private readonly IFileStorageService _fileStorage;

        public UploadProfilePictureHandler(IFileStorageService fileStorage)
        {
            _fileStorage = fileStorage;
        }

        public async Task<string> Handle(UploadProfilePictureCommand request, CancellationToken cancellationToken)
        {
            var imagePath = await _fileStorage.SaveFileAsync(request.File,request.Folder);
            return imagePath;
        }
    }
}
