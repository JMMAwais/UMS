using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Application.DTO_s;
using UMS.Application.Interfaces;
using UMS.Application.Users.Commands;
using UMS.Infrastructure.Persistance;

namespace UMS.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IjwtTokenService _jwtTokenService;

        public AuthService(UserManager<AppUser> userManager, IjwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginCommand request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new LoginResponseDTO
                {
                    Success = false,
                    Message = "User not found",
                    StatusCode = System.Net.HttpStatusCode.NotFound
                };
            }
            var result = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!result)
            {
                return new LoginResponseDTO
                {
                    Success = false,
                    Message = "Invalid Credentials",
                    StatusCode=System.Net.HttpStatusCode.BadRequest
                };
            }
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();
            var token = await _jwtTokenService.GenerateToken(user.Id, user.Email, role);
            return new LoginResponseDTO
            {
                Success = true,
                Token = token,
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }



        public async Task<RegisterUserResponseDTO> RegisterAsync(RegisterUserCommand request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return new RegisterUserResponseDTO
                {
                    Success = false,
                    Message = "UserName, Email and Password are required",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return new RegisterUserResponseDTO
                {
                    Success = false,
                    Message = "User with this email already exist",
                    StatusCode = System.Net.HttpStatusCode.Conflict
                };

            var user = new AppUser
            {
                Email = request.Email,
                NormalizedEmail = request.Email.ToUpper(),
                UserName = request.Name,
                NormalizedUserName = request.Name.ToUpper(),
                IsDeleted = false
            };
           
            var result = await _userManager.CreateAsync(user, request.Password);
           if(!result.Succeeded)
            return new RegisterUserResponseDTO
            {
                Success = false,
                Message = "User or password is invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };

            if (string.IsNullOrWhiteSpace(request.Role))
                 await _userManager.AddToRoleAsync(user, "User");

            await _userManager.AddToRoleAsync(user, request.Role ?? "User");

            return new RegisterUserResponseDTO
            {
                Success = true,
                Message = "User registered Successfully",
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }

        public Task LogoutUserAsync()
        {
            throw new NotImplementedException();
        }

     

      
    }
}
