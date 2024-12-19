using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService) : APIControllerBase
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
        public async Task<ActionResult<IEnumerable<PhotoDTO>>> GetPhotosForModeration()
        {
            var photos = await unitOfWork.PhotoRepository.GetUnapprovedPhotos();
            return Ok(photos);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photo == null)
                return BadRequest("Photo not found");

            photo.IsApproved = true;

            var user = await unitOfWork.UserRepository.GetUserByPhotoId(photoId);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            if (!user.Photos.Any(x => x.IsMain))
            {
                photo.IsMain = true;
            }

            if (await unitOfWork.Complete())
                return Ok();

            return BadRequest("Failed to approve photo");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photo == null)
                return BadRequest("Photo not found");

            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);

                if (result.Result == "ok")
                {
                    unitOfWork.PhotoRepository.RemovePhoto(photo);
                }
            }
            else
            {
                unitOfWork.PhotoRepository.RemovePhoto(photo);
            }

            if (await unitOfWork.Complete())
                return Ok();

            return BadRequest("Failed to remove photo");
        }
    }
}
