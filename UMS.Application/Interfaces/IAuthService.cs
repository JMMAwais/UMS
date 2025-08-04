using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Application.DTO_s;
using UMS.Application.Users.Commands;

namespace UMS.Application.Interfaces
{
    public  interface IAuthService
    {
        Task<RegisterUserResponseDTO> RegisterAsync(RegisterUserCommand command);
        Task<LoginResponseDTO> LoginAsync(LoginCommand command);
        Task LogoutUserAsync();
    }
}
