using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext?.Request.Query["user"];

            if (Context.User == null || string.IsNullOrEmpty(otherUser))
            {
                throw new Exception("Cannot join group");
            }

            var groupName = getGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var messages = await messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser!);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDTO createMessageDTO)
        {
            var username = Context.User?.GetUsername() ?? throw new Exception("Could not get user");

            if (username == createMessageDTO.RecipientUserName.ToLower())
            {
                throw new HubException("You cannot message yourself");
            }

            var sender = await userRepository.GetByUsernameAsync(username);
            var recipient = await userRepository.GetByUsernameAsync(createMessageDTO.RecipientUserName);

            if (sender == null || recipient == null || sender.UserName == null || recipient.UserName == null)
            {
                throw new HubException("Cannot send message at this time");
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
                var group = getGroupName(sender.UserName, recipient.UserName);

                await Clients.Group(group).SendAsync("NewMessage", (mapper.Map<MessageDTO>(message)));
            }
        }

        private string getGroupName(string caller, string? other) {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}
