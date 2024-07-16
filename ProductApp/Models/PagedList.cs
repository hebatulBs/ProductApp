namespace ProductApp.Models
{
    public class PagedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public string ErrorMessage { get; private set; }

        public PagedList(List<T> items, int count, int pageIndex, int pageSize, string errorMessage = null)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            ErrorMessage = errorMessage;

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static PagedList<T> Create(List<T> source, int pageIndex, int pageSize, string error = null)
        {
            var count = source.Count;
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, count, pageIndex, pageSize, error);
        }
    }
}
