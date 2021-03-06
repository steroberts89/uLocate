﻿namespace uLocate.Persistance
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using uLocate.Models;

    using Umbraco.Core;
    using Umbraco.Core.Cache;
    using Umbraco.Core.Persistence;

    /// <summary>
    /// The repository base.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of TEntity
    /// </typeparam>
    internal abstract class RepositoryBase<TEntity>
        where TEntity : IEntity
    {
        /// <summary>
        /// The database.
        /// </summary>
        private readonly UmbracoDatabase _database;

        /// <summary>
        /// The runtime cache.
        /// </summary>
        private readonly IRuntimeCacheProvider _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryBase{TEntity}"/> class.
        /// </summary>
        /// <param name="database">
        /// The database.
        /// </param>
        /// <param name="cache">
        /// The cache.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Throw an exception if the database or cache parameters are null
        /// </exception>
        protected RepositoryBase(UmbracoDatabase database, IRuntimeCacheProvider cache)
        {
            if (database == null) throw new ArgumentNullException("database");
            if (cache == null) throw new ArgumentNullException("cache");

            _cache = cache;
            _database = database;
        }


        #region Get Entities

        /// <summary>
        /// Gets an entity by the passed in Key
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The entity retrieved
        /// </returns>
        public TEntity Get(Guid Key)
        {
            var fromCache = TryGetFromCache(Key);
            if (fromCache.Success)
            {
                return fromCache.Result;
            }

            var entity = PerformGet(Key);
            if (entity != null)
            {
                _cache.InsertCacheItem(GetCacheKey(Key), () => entity);
                //_cache.GetCacheItem(GetCacheKey(Key), () => entity);
            }

            return entity;
        }

        /// <summary>
        /// Gets all entities of type TEntity or a list according to the passed in Guid Keys
        /// </summary>
        /// <param name="keys">The keys of the entities to be returned</param>
        /// <returns>A collection of entities</returns>
        public IEnumerable<TEntity> GetAll(params Guid[] Keys)
        {
            if (Keys.Any())
            {
                var entities = new List<TEntity>();

                foreach (var key in Keys)
                {
                    var entity = _cache.GetCacheItem(GetCacheKey(key));
                    if (entity != null) entities.Add((TEntity)entity);
                }

                if (entities.Count().Equals(Keys.Count()) && entities.Any(x => x.Equals(default(TEntity))) == false)
                    return entities;
            }
            else
            {
                var allEntities = _cache.GetCacheItemsByKeySearch(typeof(TEntity).Name + ".").ToArray();


                if (allEntities.Any())
                {
                    var query = this.GetBaseQuery(true);
                    var totalCount = PerformCount(query);

                    if (allEntities.Count() == totalCount)
                        return allEntities.Select(x => (TEntity)x);
                }
            }

            var entityCollection = PerformGetAll(Keys).ToArray();

            foreach (var entity in entityCollection)
            {
                if (!entity.Equals(default(TEntity)))
                {
                    var en = entity;
                    _cache.GetCacheItem(GetCacheKey(entity.Key), () => en);
                }
            }

            return entityCollection;
        }

        /// <summary>
        /// The abstract perform get all.
        /// </summary>
        /// <param name="keys">
        /// The keys.
        /// </param>
        /// <returns>
        /// The collection of all entities or with keys matching those in the parameter collection.
        /// </returns>
        protected abstract IEnumerable<TEntity> PerformGetAll(params Guid[] Keys);

        /// <summary>
        /// The perform get.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="TEntity"/>.
        /// </returns>
        protected abstract TEntity PerformGet(Guid Key);

        /// <summary>
        /// The perform exists.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// A value indicating whether or not an entity with the key exists.
        /// </returns>
        protected bool PerformExists(Guid Key)
        {
            var sql = GetBaseQuery(true);
            sql.Where(GetBaseWhereClause(), new { Key = Key });
            var count = _database.ExecuteScalar<int>(sql);
            return count == 1;
        }

        /// <summary>
        /// The try get from cache.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="Attempt"/>.
        /// </returns>
        protected Attempt<TEntity> TryGetFromCache(Guid Key)
        {
            var cacheKey = GetCacheKey(Key);

            var retEntity = _cache.GetCacheItem(cacheKey);

            return retEntity != null ?
                Attempt<TEntity>.Succeed((TEntity)retEntity) :
                Attempt<TEntity>.Fail();
        }

        /// <summary>
        /// Gets the cache key for the entity.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The cache key <see cref="string"/>.
        /// </returns>
        protected static string GetCacheKey(Guid Key)
        {
            return Caching.CacheKeys.GetEntityCacheKey<TEntity>(Key);
        }

        #endregion
    
        #region Persist Entities

        /// <summary>
        /// The persist new item.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// An <see cref="object"/> representing the new item's Id/Key
        /// </returns>
        protected abstract void PersistNewItem(TEntity item);

        /// <summary>
        /// The persist updated item.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        protected abstract void PersistUpdatedItem(TEntity item);

        /// <summary>
        /// The persist deleted item.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        protected abstract void PersistDeletedItem(TEntity item, out StatusMessage StatusMsg);

        #endregion

        #region SQL

        /// <summary>
        /// Perform count query
        /// </summary>
        /// <param name="sql">
        /// The SQL query.
        /// </param>
        /// <returns>
        /// The <see cref="int"/> count
        /// </returns>
        protected int PerformCount(Sql sql)
        {
            return _database.ExecuteScalar<int>(sql);
        }

        /// <summary>
        /// The get base query.
        /// </summary>
        /// <param name="isCount">
        /// The is count.
        /// </param>
        /// <returns>
        /// The <see cref="Sql"/>.
        /// </returns>
        protected abstract Sql GetBaseQuery(bool isCount);

        /// <summary>
        /// The get base where clause.
        /// </summary>
        /// <returns>
        /// The base "where" string.
        /// </returns>
        protected abstract string GetBaseWhereClause();

        /// <summary>
        /// The get delete clauses.
        /// </summary>
        /// <returns>
        /// The collection of delete clauses
        /// </returns>
        protected abstract IEnumerable<string> GetDeleteClauses();

        #endregion

    }
}