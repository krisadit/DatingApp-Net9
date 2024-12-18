namespace API.Entities
{
    public class Connection
    {
        public required string ConnectionId { get; set; } // Assumed to be the Id of this entity because of 'Id' in the property name
        public required string Username { get; set; }

    }
}
