using System;
using System.Linq.Expressions;
using CodeTrip.Core;
using CodeTrip.Core.Paging;
using Raven.Client.Indexes;
using Raven.Client.Linq;

namespace Errordite.Core.Session
{
    public abstract class SessionAccessBase : ComponentBase
    {
        public IAppSession Session { protected get; set; }

        protected void CentralStore(object o)
        {
            Session.CentralRaven.Store(o);
        }

        protected void Store(object o)
        {
            Session.Raven.Store(o);
        }

        protected T CentralLoad<T>(string id)
        {
            return Session.CentralRaven.Load<T>(id);
        }

        protected T Load<T>(string id)
        {
            return Session.Raven.Load<T>(id);
        }

        protected void Delete<T>(T obj)
        {
            Session.Raven.Delete(obj);
        }

        protected IRavenQueryable<TEntity> Query<TEntity>()
        {
            return Session.Raven.Query<TEntity>();
        }

        protected IRavenQueryable<TEntity> Query<TEntity, TIndex>()
            where TIndex : AbstractIndexCreationTask, new()
        {
            return Session.Raven.Query<TEntity, TIndex>();
        }

        protected Page<TEntity> GetPage<TEntity, TIndex, TOrdProp>(PageRequestWithSort paging, Expression<Func<TEntity, bool>> whereClause = null, Expression<Func<TEntity, TOrdProp>> orderByClause = null, bool orderDescending = false)
            where TIndex : AbstractIndexCreationTask, new()
        {
            return Session.Raven.GetPage<TEntity, TIndex, TOrdProp>(paging, whereClause, orderByClause, orderDescending);
        }
    }
}