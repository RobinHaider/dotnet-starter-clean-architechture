using Application.Core;

namespace Application.Activities
{
    public class ActivityParams : PagingParams
    {
        public bool IsGoing { get; set; }
        public bool IsHost { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public string DefaultSort { get; set; } = "title";
    }
}