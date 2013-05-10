using System;
using Errordite.Core.Extensions;
using ProtoBuf;

namespace Errordite.Core.Paging
{
    /// <summary>
    /// Paging request information.
    /// </summary>
    [Serializable, ProtoContract]
    public class PageRequest
    {
        [ProtoMember(1)]
        public int PageNumber { get; set; }
        [ProtoMember(2)]
        public int PageSize { get; set; }

        public PageRequest()
        {}

        public PageRequest(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public virtual string CacheKeyComponent
        {
            get { return "P:{0}:{1}".FormatWith(PageNumber, PageSize); }
        }
    }

    [Serializable, ProtoContract]
    public class PageRequestWithSort : PageRequest
    {
        public PageRequestWithSort(int pageNumber, int pageSize, string sort = null, bool? sortDescending = null) : 
            base(pageNumber, pageSize)
        {
            Sort = sort;
            SortDescending = sortDescending.HasValue && sortDescending.Value;
        }

        [ProtoMember(1)]
        public string Sort { get; set; }
        [ProtoMember(2)]
        public bool SortDescending { get; set; }
    }
}
