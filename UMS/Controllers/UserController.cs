using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UMS.Application.DTO_s;
using UMS.Application.Users.Commands;
using UMS.Application.Users.Queries;

namespace UMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("AllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserDTO>>> GetAllUsers()
        {
            var result = await _mediator.Send(new GetAllUserQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (currentUserId != id && !isAdmin)
                return Forbid();

            var user = await _mediator.Send(new GetUserByIdQuery(id));
            if (user == null)
                return NotFound();

            return Ok(user);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromForm] UpdateUserCommand command)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (currentUserId != id.ToString() && !isAdmin)
                return Forbid();

            if (id.ToString() != command.Id.ToString())
                return BadRequest("ID in URL and body must match");

            var success = await _mediator.Send(command);
            if (!success)
                return NotFound("User not found");

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _mediator.Send(new DeleteUserCommand(id));

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id}/profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var result = await _mediator.Send(new UploadProfilePictureCommand
            {
                UserId = id,
                File = file,
                Folder = "profile-pictures"
            });

            return Ok(new { imageUrl = result });
        }
    }
}
