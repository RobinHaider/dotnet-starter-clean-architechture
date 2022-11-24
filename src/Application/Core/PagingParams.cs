namespace Application.Core
{
    public class PagingParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        // search
        private string _search;

        public string? Search
        {
            get => _search;
            set => _search = value.Trim().ToLower();
        }

        // sort
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public string? Sort => $"{SortBy}_{SortDirection}";
    }
}