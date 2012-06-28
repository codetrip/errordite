using ProtoBuf;

namespace CodeTrip.Core.Paging
{
    /// <summary>
    /// Information about Paginated results.
    /// </summary>
    [ProtoContract]
    public class PagingStatus
    {
        public PagingStatus()
        {}

        public PagingStatus(int pageSize, int pageNumber, int totalRecords)
        {
            TotalItems = totalRecords;
            PageSize = pageSize == 0 ? 1024 : pageSize;
            PageNumber = pageNumber == 0 ? 1 : pageNumber;

            int pageCount = TotalItems / PageSize + (TotalItems % PageSize == 0 ? 0 : 1);

            FirstItem = ((PageNumber - 1) * PageSize) + 1;
            HasNextPage = PageNumber < pageCount;
            HasPreviousPage = PageNumber > 1;
            LastItem = PageNumber < pageCount ? ((PageNumber - 1) * PageSize) + PageSize : TotalItems;
            TotalPages = pageCount;
        }

        [ProtoMember(1)]
        public int PageNumber { get; private set; }
        [ProtoMember(2)]
        public int PageSize { get; private set; }
        [ProtoMember(3)]
        public int FirstItem { get; private set; }
        [ProtoMember(4)]
        public int LastItem { get; private set; }
        [ProtoMember(5)]
        public bool HasNextPage { get; private set; }
        [ProtoMember(6)]
        public bool HasPreviousPage { get; private set; }
        [ProtoMember(7)]
        public int TotalItems { get; private set; }
        [ProtoMember(8)]
        public int TotalPages { get; private set; }
    }
}
