using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController(UserManager<AppUser> userManager) : APIControllerBase
    {
        [Authorize(Policy = "RequiredAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await userManager.Users
                .OrderBy(x => x.UserName)
                .Select(x => new
                {
                    x.Id,
                    Username = x.UserName,
                    Roles = x.UserRoles.Select(r => r.Role.Name).ToList()
                }).ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "RequiredAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, string roles)
        {
            if (string.IsNullOrEmpty(roles))
            {
                return BadRequest("You must select at least one role");
            }

            var selectedRoles = roles.Split(",").ToArray();

            var user = await userManager.FindByNameAsync(username);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            var currentRoles = await userManager.GetRolesAsync(user);

            var results = await userManager.AddToRolesAsync(user, selectedRoles.Except(currentRoles));

            if (!results.Succeeded)
            {
                return BadRequest("Failed to add to roles");
            }

            results = await userManager.RemoveFromRolesAsync(user, currentRoles.Except(selectedRoles));

            if (!results.Succeeded)
            {
                return BadRequest("Failed to remove old roles");
            }

            return Ok(await userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Only admins/moderators can see this");
        }
    }
}
