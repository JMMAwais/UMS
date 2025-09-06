using Azure.Core;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Application.DTO_s;
using UMS.Application.Interfaces;
using UMS.Application.Users.Commands;
using UMS.Domain.Entities;
using UMS.Infrastructure.Persistance;
using UMS.Infrastructure.Services;

namespace UMS.Infrastructure.Repositories
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IjwtTokenService _jwtTokenService;
        private readonly IWebHostEnvironment _env;
        public UserService(UserManager<AppUser> userManager, IjwtTokenService jwtTokenService,IWebHostEnvironment env)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _env = env;
        }

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.AsNoTracking().Where(a=>!a.IsDeleted).Select(u => new UserDTO
            {
                Id= u.Id,
                Email = u.Email,
                UserName = u.UserName
            }).ToListAsync();

            foreach (var user in users)
            {
                var appUser = await _userManager.FindByEmailAsync(user.Email);
                user.Roles = await _userManager.GetRolesAsync(appUser);
            }
            return users;
        }

        public async Task<User> GetUserDetailsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new User
            {
                Id= user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }


        public async Task<bool> DeleteAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UpdateUserDetailsAsync(UpdateUserCommand command)
        {
            var user = await _userManager.FindByIdAsync(command.Id.ToString());
            if (user == null)
                return false;

        
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            if (command.File != null && command.File.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(command.File.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await command.File.CopyToAsync(stream);
                }

                // Update profile image URL
                user.ProfileImageUrl = "/uploads/" + fileName;
            }

            user.Email = command.Email;
            user.UserName = command.UserName;
       
            if (!string.IsNullOrWhiteSpace(command.Roles.FirstOrDefault()))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, command.Roles.FirstOrDefault());
            }
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
