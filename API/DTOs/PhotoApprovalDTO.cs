namespace API.DTOs
{
    public class PhotoApprovalDTO
    {
        public int Id { get; set; }
        public string? Url { get; set; }
        public string? Username { get; set; }
        public bool IsApproved { get; set; }
    }
}
