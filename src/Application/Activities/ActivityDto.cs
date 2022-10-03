namespace Application.Activities
{
    public class ActivityDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Venue { get; set; }
        public string HostUsername { get; set; }
        public bool IsCancelled { get; set; }
        public ICollection<AttendeeDto> Attendees { get; set; }
    }

    public class AttendeeDto
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        // public string Image { get; set; }
        // public bool Following { get; set; }
        // public int FollowersCount { get; set; }
        // public int FollowingCount { get; set; }
    }
}