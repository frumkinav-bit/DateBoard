namespace DateBoard.Models
{
    public class OnlineStatus
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsOnline => (DateTime.UtcNow - LastActivity).TotalMinutes < 5;
    }
}