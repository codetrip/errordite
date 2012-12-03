using System.Collections.Generic;

namespace CodeTrip.Core.Paging
{
    public interface IPagingConfiguration
    {
        int PageSelectorCount { get; set; }
        int NormalPageSize { get; set; }
        int LargePageSize { get; set; }
        IList<int> PageSizes { get; set; }
    }

    public class PagingConfiguration : IPagingConfiguration
    {
        public int PageSelectorCount { get; set; }
        public int NormalPageSize { get; set; }
        public int LargePageSize { get; set; }
        public IList<int> PageSizes { get; set; }
    }
}