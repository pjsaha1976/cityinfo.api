namespace CityInfo.API.Services
{
    public class PaginationMetadata
    {
        public int TotalItemCount { get; set; }
        public int TotalPageCount { get; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }

        public PaginationMetadata(int totalItemCount, int pageSize, int currentPage)
        {
            TotalItemCount = totalItemCount;
            TotalPageCount = (int)Math.Ceiling(totalItemCount / pageSize * 1.0);
            PageSize = pageSize;
            CurrentPage = currentPage;
        }
    }
}
