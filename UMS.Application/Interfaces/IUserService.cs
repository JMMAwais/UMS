using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Application.DTO_s;
using UMS.Application.Users.Commands;
using UMS.Domain.Entities;

namespace UMS.Application.Interfaces
{
    public interface IUserService
    {
        //Task<string> RegisterAsync(string name, string email, string password);
        //Task<string> LoginAsync(string Email, string Password);
        //Task LogoutUserAsync();

        Task<List<UserDTO>> GetAllUsersAsync();
        Task<User> GetUserDetailsAsync(string userId);
        Task<bool> UpdateUserDetailsAsync(UpdateUserCommand command);
        Task<bool> DeleteAsync(string userId);
    }
}
