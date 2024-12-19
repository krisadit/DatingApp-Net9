﻿using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper, IHubContext<PresenceHub> presenceHubContext) : Hub
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
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser!);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
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

            var groupName = getGroupName(sender.UserName, recipient.UserName);

            var group = await messageRepository.GetMessageGroup(groupName);

            if (group != null && group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null && connections.Count != 0)
                {
                    await presenceHubContext.Clients.Clients(connections).SendAsync("NewMessageReceived", new
                    {
                        username = sender.UserName,
                        knownAs = sender.KnownAs
                    });
                }
            }


            messageRepository.AddMessage(message);

            if (await messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", (mapper.Map<MessageDTO>(message)));
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var username = Context.User?.GetUsername() ?? throw new Exception("Could not get user");

            Group? group = await messageRepository.GetMessageGroup(groupName);
            Connection connection = new() { ConnectionId = Context.ConnectionId, Username = username };

            if (group == null)
            {
                group = new() { Name = groupName };
                messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if(await messageRepository.SaveAllAsync())
            {
                return group;
            }

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (connection != null && group != null)
            {
                messageRepository.RemoveConnection(connection);
                if (await messageRepository.SaveAllAsync())
                {
                    return group;
                }
            }
            throw new HubException("Failed to remove from group");
        }

        private string getGroupName(string caller, string? other) {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}
