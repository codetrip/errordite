
namespace Errordite.Core.Domain.Error
{
    public class SearchFilter
    {
        public string Id { get; set; }
        public string Query { get; set; }
        public string Name { get; set; }
        public FilterClass Class { get; set; }
    }

    public enum FilterClass
    {
        Error,
        Class
    }
}
