using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Application.DTO_s;
using UMS.Application.Interfaces;
using UMS.Application.Users.Queries;

namespace UMS.Application.Users.Handlers.QueryHandlers
{
    public class AllUserQueryHandler : IRequestHandler<GetAllUserQuery, List<UserDTO>>
    {
        private readonly IUserService _userService;

        public AllUserQueryHandler(IUserService userService)
        {
            _userService = userService;
        }
        public async Task<List<UserDTO>> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
        {
            return await _userService.GetAllUsersAsync();
        }
    }
}
