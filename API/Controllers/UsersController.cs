﻿using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUnitOfWork unitOfWork, IPhotoService photoService, IMapper mapper) : APIControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery]UserParams userParams)
        {
            userParams.CurrentUserName = User.GetUsername();
            var users = await unitOfWork.UserRepository.GetAllMembersAsync(userParams);
            Response.AddPaginationHeader(users);

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            var currentUsername = User.GetUsername();
            var user = await unitOfWork.UserRepository.GetMemberAsync(username, isCurrentUser: username == currentUsername);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdate)
        {
            var user = await unitOfWork.UserRepository.GetByUsernameAsync(User.GetUsername());

            if (user == null)
            {
                return BadRequest("Could not find user");
            }

            mapper.Map(memberUpdate, user);

            if (await unitOfWork.Complete())
            {
                return NoContent();
            }

            return BadRequest("Failed to update the user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var user = await unitOfWork.UserRepository.GetByUsernameAsync(User.GetUsername());

            if (user == null)
            {
                return BadRequest("Could not find user");
            }

            var result = await photoService.AddPhotoAsync(file);

            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }

            Photo photo = new()
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            user.Photos.Add(photo);

            if (await unitOfWork.Complete())
            {
                return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, mapper.Map<PhotoDTO>(photo));
            }
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await unitOfWork.UserRepository.GetByUsernameAsync(User.GetUsername());

            if (user == null)
            {
                return BadRequest("Could not find user");
            }

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null || photo.IsMain)
                return BadRequest("Cannot use this as main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain != null)
            {
                currentMain.IsMain = false;
            }

            photo.IsMain = true;
            if (await unitOfWork.Complete())
            {
                return NoContent();
            }
            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await unitOfWork.UserRepository.GetByUsernameAsync(User.GetUsername());

            if (user == null)
            {
                return BadRequest("Could not find user");
            }

            var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photo == null || photo.IsMain)
                return BadRequest("This photo cannot be deleted");

            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }
            }

            user.Photos.Remove(photo);

            if (await unitOfWork.Complete())
                return Ok();

            return BadRequest("Problem deleting photo");
        }
    }
}
