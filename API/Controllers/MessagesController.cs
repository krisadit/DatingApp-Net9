using API.DTOs;
using API.Extensions;
using API.Interfaces;
using API.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using API.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController(IMessageRepository messageRepository,
        IUserRepository userRepository, IMapper mapper) : APIControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            var username = User.GetUsername();

            if (username == createMessageDTO.RecipientUserName.ToLower())
            {
                return BadRequest("You cannot message yourself");
            }

            var sender = await userRepository.GetByUsernameAsync(username);
            var recipient = await userRepository.GetByUsernameAsync(createMessageDTO.RecipientUserName);

            if (sender == null || recipient == null)
            {
                return BadRequest("Cannot send message at this time");
            }

            Message message = new()
            {
                Sender = sender,
                SenderUserName = sender.UserName,
                Recipient = recipient,
                RecipientUserName = recipient.UserName,
                Content = createMessageDTO.Content
            };

            messageRepository.AddMessage(message);

            if (await messageRepository.SaveAllAsync())
            {
                return Ok(mapper.Map<MessageDTO>(message));
            }

            return BadRequest("Could not send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await messageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages);

            return Ok(messages);
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();

            return Ok(await messageRepository.GetMessageThread(username, currentUsername));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var currentUsername = User.GetUsername();

            var message = await messageRepository.GetMessage(id);

            if (message == null)
            {
                return BadRequest("Cannot delete this message");
            }

            if (message.SenderUserName != currentUsername && message.RecipientUserName != currentUsername)
            {
                return Forbid();
            }

            if (message.SenderUserName == currentUsername)
                message.SenderDeleted = true;

            if (message.RecipientUserName == currentUsername)
                message.RecipientDeleted = true;

            if (message is { SenderDeleted: true, RecipientDeleted: true })
                messageRepository.DeleteMessage(message);

            if (await messageRepository.SaveAllAsync())
                return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}
