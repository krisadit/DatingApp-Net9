namespace API.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> OnlineUsers = []; // not scalable, not thread safe, use Redis or something else.

        public Task UserConnected(string username, string connectionId)
        {
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    OnlineUsers.Add(username, [connectionId]);
                }    
            }

            return Task.CompletedTask;
        }

        public Task UserDisconnected(string username, string connectionId)
        {
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username))
                {
                    return Task.CompletedTask;
                }

                OnlineUsers[username].Remove(connectionId);

                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                }
            }

            return Task.CompletedTask;
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] result = [];
            lock (OnlineUsers)
            {
                result = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            return Task.FromResult(result);
        }

        public static Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;

            if (OnlineUsers.TryGetValue(username, out var connections))
            {
                lock (connections)
                {
                    connectionIds = connections.ToList();
                }
            }
            else
            {
                connectionIds = [];
            }

            return Task.FromResult(connectionIds);
        }
    }
}
