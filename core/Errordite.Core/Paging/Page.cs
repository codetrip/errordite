using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Errordite.Core.Paging
{
    /// <summary>
    /// Represents one page of entities, along with information about which page it is.
    /// 
    /// Usually you would use the GetPagedResults ICriteria extension method do create this
    /// directly in a repository.
    /// </summary>
    [ProtoContract]
    public class Page<TEntity>
    {
        [ProtoMember(1)] 
        private readonly IList<TEntity> _items;
        [ProtoMember(2)]
        public PagingStatus PagingStatus { get; private set; }

        public Page()
        {}

        public Page(IList<TEntity> items, PagingStatus pagingStatus)
        {
            _items = items;
            PagingStatus = pagingStatus;
        }

        public IList<TEntity> Items
        {
            get { return _items ?? new List<TEntity>(); }
        }

        public Page<TOtherEntity> ConvertPage<TOtherEntity>(Func<TEntity, TOtherEntity> projection)
        {
            return new Page<TOtherEntity>(Items.Select(projection).ToList(), PagingStatus);
        }

        public static Page<TEntity> Empty(PageRequestWithSort paging)
        {
            return new Page<TEntity>(new TEntity[0], new PagingStatus(paging.PageSize, paging.PageNumber, 0));
        }
    }
}