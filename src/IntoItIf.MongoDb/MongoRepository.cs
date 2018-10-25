﻿namespace IntoItIf.MongoDb
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Linq.Expressions;
   using System.Threading;
   using System.Threading.Tasks;
   using Base.Domain;
   using Base.Domain.Options;
   using Base.Helpers;
   using Base.Repositories;
   using MongoDB.Driver;

   public sealed class MongoRepository<T> : BaseRepository<T>
      where T : class
   {
      #region Constructors and Destructors

      public MongoRepository(Option<MongoDataContext> dataContext) : this(dataContext.ReduceOrDefault(), None.Value)
      {
      }

      public MongoRepository(Option<MongoDataContext> dataContext, Option<IQueryable<T>> customQuery) : base(
         dataContext.ReduceOrDefault(),
         customQuery)
      {
         MongoDataContext = dataContext;
      }

      #endregion

      #region Properties

      private Option<MongoDataContext> MongoDataContext { get; }

      #endregion

      #region Public Methods and Operators

      public override Option<Dictionary<string, object>> Add(Option<T> entity, Func<T, string> existMessageFunc)
      {
         return MongoDataContext
            .MapFlatten(x => GetRealSession(x))
            .MapFlatten(
               x =>
               {
                  var session = x.ToOption();
                  return ProcessCreateAndGetResult(GetValidatedEntityForCreate(entity, session), existMessageFunc, session);
               });
      }

      public override Task<Option<Dictionary<string, object>>> AddAsync(
         Option<T> entity,
         Func<T, string> existMessageFunc,
         Option<CancellationToken> ctok)
      {
         return MongoDataContext
            .Combine(ctok, true, CancellationToken.None)
            .Map(x => (DataContext: x.Item1, Ctok: x.Item2))
            .MapFlattenAsync(x => GetRealSessionAsync(x.DataContext, x.Ctok))
            .MapFlattenAsync(
               async x =>
               {
                  var session = x.ToOption();
                  return await ProcessCreateAndGetResultAsync(
                     GetValidatedEntityForCreateAsync(entity, session, ctok),
                     existMessageFunc,
                     session,
                     ctok);
               });
      }

      public override Option<Dictionary<string, object>> Change(Option<T> entity, Func<T, string> existMessageFunc)
      {
         return MongoDataContext
            .MapFlatten(x => GetRealSession(x))
            .MapFlatten(
               x =>
               {
                  var session = x.ToOption();
                  return ProcessUpdateAndGetResult(GetValidatedEntityForUpdate(entity, session), existMessageFunc, session);
               });
      }

      public override Task<Option<Dictionary<string, object>>> ChangeAsync(
         Option<T> entity,
         Func<T, string> existMessageFunc,
         Option<CancellationToken> ctok)
      {
         return MongoDataContext
            .Combine(ctok, true, CancellationToken.None)
            .Map(x => (DataContext: x.Item1, Ctok: x.Item2))
            .MapFlattenAsync(x => GetRealSessionAsync(x.DataContext, x.Ctok))
            .MapFlattenAsync(
               async x =>
               {
                  var session = x.ToOption();
                  return await ProcessUpdateAndGetResultAsync(
                     GetValidatedEntityForUpdateAsync(entity, session, ctok),
                     existMessageFunc,
                     session,
                     ctok);
               });
      }

      public override Option<T> GetFirstOrDefault(Expression<Func<T, bool>> predicate)
      {
         return GetFirstOrDefault(x => x, predicate);
      }

      public Option<T> GetFirstOrDefault(Expression<Func<T, bool>> predicate, Option<IClientSessionHandle> session)
      {
         return GetFirstOrDefault(x => x, predicate, session);
      }

      public override Option<TResult> GetFirstOrDefault<TResult>(
         Expression<Func<T, TResult>> selector,
         Expression<Func<T, bool>> predicate)
      {
         return GetFirstOrDefault(selector, predicate, GetSessionOrNone());
      }

      public Option<TResult> GetFirstOrDefault<TResult>(
         Expression<Func<T, TResult>> selector,
         Expression<Func<T, bool>> predicate,
         Option<IClientSessionHandle> session)
      {
         return InternalGetFirstOrDefault(selector.ToOption(), predicate, session);
      }

      public override Task<Option<T>> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, Option<CancellationToken> ctok)
      {
         return GetFirstOrDefaultAsync(x => x, predicate, ctok);
      }

      public override Task<Option<TResult>> GetFirstOrDefaultAsync<TResult>(
         Expression<Func<T, TResult>> selector,
         Expression<Func<T, bool>> predicate,
         Option<CancellationToken> ctok)
      {
         return GetFirstOrDefaultAsync(selector, predicate, GetSessionOrNone(), ctok);
      }

      public Task<Option<TResult>> GetFirstOrDefaultAsync<TResult>(
         Expression<Func<T, TResult>> selector,
         Expression<Func<T, bool>> predicate,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return InternalGetFirstOrDefaultAsync(selector.ToOption(), predicate, session, ctok);
      }

      public override Option<List<TResult>> GetList<TResult>(
         Expression<Func<T, TResult>> selector,
         Expression<Func<T, bool>> predicate)
      {
         return GetList(selector, predicate, null);
      }

      public Option<List<TResult>> GetList<TResult>(
         Expression<Func<T, TResult>> selector,
         Expression<Func<T, bool>> predicate,
         Func<IFindFluent<T, T>, IOrderedFindFluent<T, T>> sortBy)
      {
         return GetList(selector, predicate, sortBy, GetSessionOrNone());
      }

      public Option<List<TResult>> GetList<TResult>(
         Expression<Func<T, TResult>> selector,
         Expression<Func<T, bool>> predicate,
         Func<IFindFluent<T, T>, IOrderedFindFluent<T, T>> sortBy,
         Option<IClientSessionHandle> session)
      {
         return InternalGetList(selector.ToOption(), predicate, sortBy, session);
      }

      public override Task<Option<List<TResult>>> GetListAsync<TResult>(
         Expression<Func<T, TResult>> selector,
         Expression<Func<T, bool>> predicate,
         Option<CancellationToken> ctok)
      {
         return GetListAsync(selector, predicate, null, ctok);
      }

      public Task<Option<List<TResult>>> GetListAsync<TResult>(
         Expression<Func<T, TResult>> selector,
         Expression<Func<T, bool>> predicate,
         Func<IFindFluent<T, T>, IOrderedFindFluent<T, T>> sortBy,
         Option<CancellationToken> ctok)
      {
         return GetListAsync(selector, predicate, sortBy, GetSessionOrNone(), ctok);
      }

      public Task<Option<List<TResult>>> GetListAsync<TResult>(
         Expression<Func<T, TResult>> selector,
         Expression<Func<T, bool>> predicate,
         Func<IFindFluent<T, T>, IOrderedFindFluent<T, T>> sortBy,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return InternalGetListAsync(selector.ToOption(), predicate, sortBy, session, ctok);
      }

      public override Option<List<KeyValue>> GetLookups(
         Option<string> idProperty,
         Option<string> valueProperty,
         Option<bool> useValueAsId,
         Expression<Func<T, bool>> predicate)
      {
         return GetLookups(idProperty, valueProperty, useValueAsId, predicate, GetSessionOrNone());
      }

      public Option<List<KeyValue>> GetLookups(
         Option<string> idProperty,
         Option<string> valueProperty,
         Option<bool> useValueAsId,
         Option<Expression<Func<T, bool>>> predicate,
         Option<IClientSessionHandle> session)
      {
         return InternalGetLookups(idProperty, valueProperty, useValueAsId, predicate, session);
      }

      public override Task<Option<List<KeyValue>>> GetLookupsAsync(
         Option<string> idProperty,
         Option<string> valueProperty,
         Option<bool> useValueAsId,
         Expression<Func<T, bool>> predicate,
         Option<CancellationToken> ctok)
      {
         return GetLookupsAsync(idProperty, valueProperty, useValueAsId, predicate, GetSessionOrNone(), ctok);
      }

      public Task<Option<List<KeyValue>>> GetLookupsAsync(
         Option<string> idProperty,
         Option<string> valueProperty,
         Option<bool> useValueAsId,
         Expression<Func<T, bool>> predicate,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return InternalGetLookupsAsync(idProperty, valueProperty, useValueAsId, predicate, session, ctok);
      }

      public override Option<IPaged<T>> GetPaged(
         Option<string[]> searchFields,
         Option<int> pageIndex,
         Option<int> pageSize,
         Option<string[]> sorts,
         Option<string> keyword,
         Option<PageIndexFrom> indexFrom,
         Expression<Func<T, bool>> predicate)
      {
         return GetPaged(x => x, searchFields, pageIndex, pageSize, sorts, keyword, indexFrom, predicate);
      }

      public override Option<IPaged<TResult>> GetPaged<TResult>(
         Expression<Func<T, TResult>> selector,
         Option<string[]> searchFields,
         Option<int> pageIndex,
         Option<int> pageSize,
         Option<string[]> sorts,
         Option<string> keyword,
         Option<PageIndexFrom> indexFrom,
         Expression<Func<T, bool>> predicate)
      {
         return GetPaged(selector, searchFields, pageIndex, pageSize, sorts, keyword, indexFrom, predicate, GetSessionOrNone());
      }

      public Option<IPaged<TResult>> GetPaged<TResult>(
         Expression<Func<T, TResult>> selector,
         Option<string[]> searchFields,
         Option<int> pageIndex,
         Option<int> pageSize,
         Option<string[]> sorts,
         Option<string> keyword,
         Option<PageIndexFrom> indexFrom,
         Expression<Func<T, bool>> predicate,
         Option<IClientSessionHandle> session)
      {
         return InternalGetPaged(selector.ToOption(), searchFields, pageIndex, pageSize, sorts, keyword, indexFrom, predicate, session);
      }

      public override Task<Option<IPaged<T>>> GetPagedAsync(
         Option<string[]> searchFields,
         Option<int> pageIndex,
         Option<int> pageSize,
         Option<string[]> sorts,
         Option<string> keyword,
         Option<PageIndexFrom> indexFrom,
         Expression<Func<T, bool>> predicate,
         Option<CancellationToken> ctok)
      {
         return GetPagedAsync(
            x => x,
            searchFields,
            pageIndex,
            pageSize,
            sorts,
            keyword,
            indexFrom,
            predicate,
            ctok);
      }

      public override Task<Option<IPaged<TResult>>> GetPagedAsync<TResult>(
         Expression<Func<T, TResult>> selector,
         Option<string[]> searchFields,
         Option<int> pageIndex,
         Option<int> pageSize,
         Option<string[]> sorts,
         Option<string> keyword,
         Option<PageIndexFrom> indexFrom,
         Expression<Func<T, bool>> predicate,
         Option<CancellationToken> ctok)
      {
         return GetPagedAsync(
            selector,
            searchFields,
            pageIndex,
            pageSize,
            sorts,
            keyword,
            indexFrom,
            predicate,
            GetSessionOrNone(),
            ctok);
      }

      public Task<Option<IPaged<TResult>>> GetPagedAsync<TResult>(
         Expression<Func<T, TResult>> selector,
         Option<string[]> searchFields,
         Option<int> pageIndex,
         Option<int> pageSize,
         Option<string[]> sorts,
         Option<string> keyword,
         Option<PageIndexFrom> indexFrom,
         Expression<Func<T, bool>> predicate,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return InternalGetPagedAsync(
            selector.ToOption(),
            searchFields,
            pageIndex,
            pageSize,
            sorts,
            keyword,
            indexFrom,
            predicate,
            session,
            ctok);
      }

      public override Option<long> LongCount()
      {
         return LongCount(None.Value);
      }

      public Option<long> LongCount(Option<Expression<Func<T, bool>>> predicate)
      {
         return LongCount(predicate, GetSessionOrNone());
      }

      public Option<long> LongCount(Option<Expression<Func<T, bool>>> predicate, Option<IClientSessionHandle> session)
      {
         return InternalLongCount(predicate, session);
      }

      public override Task<Option<long>> LongCountAsync(Option<CancellationToken> ctok)
      {
         return LongCountAsync(None.Value, ctok);
      }

      public Task<Option<long>> LongCountAsync(Option<Expression<Func<T, bool>>> predicate)
      {
         return LongCountAsync(predicate, None.Value);
      }

      public Task<Option<long>> LongCountAsync(Option<Expression<Func<T, bool>>> predicate, Option<CancellationToken> ctok)
      {
         return LongCountAsync(predicate, GetSessionOrNone(), ctok);
      }

      public Task<Option<long>> LongCountAsync(
         Option<Expression<Func<T, bool>>> predicate,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return InternalLongCountAsync(predicate, session, ctok);
      }

      public override Option<bool> Remove(Option<T> entity)
      {
         return MongoDataContext
            .MapFlatten(x => GetRealSession(x))
            .MapFlatten(
               x =>
               {
                  var session = x.ToOption();
                  return ProcessDeleteAndGetResult(GetValidatedEntityForDelete(entity, session), session);
               });
      }

      public override Task<Option<bool>> RemoveAsync(Option<T> entity, Option<CancellationToken> ctok)
      {
         return MongoDataContext
            .Combine(ctok, true, CancellationToken.None)
            .Map(x => (DataContext: x.Item1, Ctok: x.Item2))
            .MapFlattenAsync(x => GetRealSessionAsync(x.DataContext, x.Ctok))
            .MapFlattenAsync(
               async x =>
               {
                  var session = x.ToOption();
                  return await ProcessDeleteAndGetResultAsync(GetValidatedEntityForDeleteAsync(entity, session, ctok), session, ctok);
               });
      }

      #endregion

      #region Methods

      /// <summary>
      ///    Not Implemented
      /// </summary>
      /// <param name="sourceQuery"></param>
      /// <param name="predicate"></param>
      /// <returns></returns>
      protected override Option<IQueryable<T>> BuildQuery(Option<IQueryable<T>> sourceQuery, Option<Expression<Func<T, bool>>> predicate)
      {
         throw new NotImplementedException();
      }

      private static Option<IFindFluent<T, TResult>> GetPerPageFindFluent<TResult>(
         Option<IMongoCollection<T>> set,
         Option<FindOptions> findOptions,
         Option<Expression<Func<T, TResult>>> selector,
         Option<IPageQuery> pageQuery,
         Option<string>[] defaultSortKeys,
         Option<Expression<Func<T, bool>>> predicate,
         Option<IClientSessionHandle> session)
      {
         return set.Combine(findOptions)
            .Combine(selector)
            .Combine(pageQuery)
            .Combine(defaultSortKeys.ToOptionOfArray())
            .Combine(predicate, true, _ => true)
            .Combine(session, true)
            .Map(
               x => (
                  Set: x.Item1.Item1.Item1.Item1.Item1.Item1,
                  FindOptions: x.Item1.Item1.Item1.Item1.Item1.Item2,
                  Selector: x.Item1.Item1.Item1.Item1.Item2,
                  PageQuery: x.Item1.Item1.Item1.Item2,
                  DefaultSortKeys: x.Item1.Item1.Item2,
                  Predicate: x.Item1.Item2,
                  Session: x.Item2
               ))
            .Map(
               x =>
               {
                  var keywordPredicate = SearchInPropertyNames<T>.BuildPredicate(x.PageQuery.Keyword, x.PageQuery.SearchFields.Pascalize());
                  var exprBody = Expression.AndAlso(x.Predicate, keywordPredicate);
                  var inPredicate = Expression.Lambda<Func<T, bool>>(exprBody, x.Predicate.Parameters[0]);
                  var findFluent = x.Session == null
                     ? x.Set.Find(inPredicate, x.FindOptions)
                     : x.Set.Find(x.Session, inPredicate, x.FindOptions);
                  var pageSize = PageQuery.DefaultPageSize;
                  var pageIndex = PageQuery.DefaultIndexFrom.Id;
                  if (x.PageQuery.PageSize > 0) pageSize = x.PageQuery.PageSize;
                  if (x.PageQuery.PageIndex > 0) pageIndex = x.PageQuery.PageIndex;
                  var sortDefinition = Builders<T>.Sort.Combine();
                  if (x.PageQuery.Sorts != null && PagedIQueryable.IsValidSorts<T>(x.PageQuery.Sorts))
                  {
                     foreach (var sort in x.PageQuery.Sorts)
                     {
                        if (sort.StartsWith("+"))
                           sortDefinition = sortDefinition.Ascending(new StringFieldDefinition<T>(sort.TrimStart('+')));
                        else if (sort.StartsWith("-"))
                           sortDefinition = sortDefinition.Descending(new StringFieldDefinition<T>(sort.TrimStart('-')));
                        else
                           sortDefinition = sortDefinition.Ascending(new StringFieldDefinition<T>(sort));
                     }
                  }
                  else
                  {
                     sortDefinition = x.DefaultSortKeys.Aggregate(
                        sortDefinition,
                        (current, defaultSort) => current.Ascending(new StringFieldDefinition<T>(defaultSort)));
                  }

                  return findFluent.Sort(sortDefinition)
                     .Skip((pageIndex - x.PageQuery.IndexFrom.Id) * pageSize)
                     .Limit(pageSize)
                     .Project(x.Selector);
               });
      }

      private static Option<IClientSessionHandle> GetRealSession(Option<MongoDataContext> context)
      {
         return context
            .Map(
               x =>
               {
                  IClientSessionHandle result;
                  var explicitSession = x.ExplicitSession.ReduceOrDefault();
                  var implicitSession = x.ImplicitSession.ReduceOrDefault();
                  if (explicitSession != null)
                  {
                     if (explicitSession.IsInTransaction)
                     {
                        result = explicitSession;
                     }
                     else
                     {
                        throw new InvalidOperationException("Please call StartTransaction() first on session");
                     }
                  }
                  else
                  {
                     if (implicitSession == null) implicitSession = x.GetImplicitMongoSession().ReduceOrDefault();
                     if (!implicitSession.IsInTransaction) implicitSession.StartTransaction();
                     result = implicitSession;
                  }

                  return result;
               });
      }

      private static Task<Option<IClientSessionHandle>> GetRealSessionAsync(
         Option<MongoDataContext> context,
         Option<CancellationToken> ctok)
      {
         return context
            .Combine(ctok, true, CancellationToken.None)
            .Map(x => (DataContext: x.Item1, Ctok: x.Item2))
            .MapAsync(
               async x =>
               {
                  IClientSessionHandle result;
                  var explicitSession = x.DataContext.ExplicitSession.ReduceOrDefault();
                  var implicitSession = x.DataContext.ImplicitSession.ReduceOrDefault();
                  if (explicitSession != null)
                  {
                     if (explicitSession.IsInTransaction)
                     {
                        result = explicitSession;
                     }
                     else
                     {
                        throw new InvalidOperationException("Please call StartTransaction() first on session");
                     }
                  }
                  else
                  {
                     if (implicitSession == null)
                     {
                        implicitSession = (await x.DataContext.GetImplicitMongoSessionAsync(x.Ctok)).ReduceOrDefault();
                     }

                     if (!implicitSession.IsInTransaction) implicitSession.StartTransaction();
                     result = implicitSession;
                  }

                  return result;
               });
      }

      private Option<Dictionary<string, object>> CreateAndGetKeyValues(
         Option<(T MatchValidatedEntity, string[] PropertyNames, T InputEntity, Func<T, string> MessageFunc)> validated,
         Option<IClientSessionHandle> session)
      {
         return validated
            .Map(
               x => (
                  IsSuccess: CreateEntry(x.InputEntity, session).ReduceOrDefault(),
                  x.PropertyNames,
                  x.InputEntity
               ))
            .IfMapFlatten(
               x => x.IsSuccess,
               x => RepositoryHelper<T>.GetKeysAndValues(x.PropertyNames, x.InputEntity))
            .Output;
      }

      private async Task<Option<Dictionary<string, object>>> CreateAndGetKeyValuesAsync(
         Option<(T MatchValidatedEntity, string[] PropertyNames, T InputEntity, Func<T, string> MessageFunc)> validated,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return
            (await validated
               .Combine(ctok, true, CancellationToken.None)
               .Map(x => (Validated: x.Item1, Ctok: x.Item2))
               .MapAsync(
                  async x => (
                     IsSuccess: (await CreateEntryAsync(x.Validated.InputEntity, session, x.Ctok)).ReduceOrDefault(),
                     x.Validated.PropertyNames,
                     x.Validated.InputEntity
                  )))
            .IfMapFlatten(
               x => x.IsSuccess,
               x => RepositoryHelper<T>.GetKeysAndValues(x.PropertyNames, x.InputEntity))
            .Output;
      }

      private Option<bool> CreateEntry(Option<T> entry, Option<IClientSessionHandle> session)
      {
         return MongoDataContext
            .MapFlatten(x => x.Collection<T>())
            .Combine(session, true)
            .Map(x => (Set: x.Item1, Session: x.Item2))
            .Execute(
               x =>
               {
                  if (x.Session == null)
                  {
                     x.Set.InsertOne(entry.ReduceOrDefault());
                  }
                  else
                  {
                     x.Set.InsertOne(x.Session, entry.ReduceOrDefault());
                  }
               });
      }

      private async Task<Option<bool>> CreateEntryAsync(
         Option<T> entry,
         Option<IClientSessionHandle> sesion,
         Option<CancellationToken> ctok)
      {
         return await MongoDataContext
            .MapFlatten(x => x.Collection<T>())
            .Combine(sesion, true)
            .Combine(ctok, true, CancellationToken.None)
            .Map(x => (Set: x.Item1.Item1, Session: x.Item1.Item2, Ctok: x.Item2))
            .ExecuteAsync(
               async x =>
               {
                  if (x.Session == null)
                  {
                     await x.Set.InsertOneAsync(entry.ReduceOrDefault(), cancellationToken: x.Ctok);
                  }
                  else
                  {
                     await x.Set.InsertOneAsync(x.Session, entry.ReduceOrDefault(), cancellationToken: x.Ctok);
                  }
               });
      }

      private Option<bool> DeleteEntry(Option<T> exist, Option<IClientSessionHandle> session)
      {
         return MongoDataContext
            .MapFlatten(x => x.Collection<T>())
            .Combine(session, true)
            .Combine(MongoDataContext.MapFlatten(y => y.BuildPrimaryKeyPredicate(exist.ReduceOrDefault())))
            .Map(x => (Set: x.Item1.Item1, Session: x.Item1.Item2, x.Item2.Predicate))
            .Execute(
               x =>
               {
                  if (x.Session == null)
                  {
                     x.Set.DeleteOne(x.Predicate);
                  }
                  else
                  {
                     x.Set.DeleteOne(x.Session, x.Predicate);
                  }
               });
      }

      private async Task<Option<bool>> DeleteEntryAsync(
         Option<T> exist,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return await MongoDataContext
            .MapFlatten(x => x.Collection<T>())
            .Combine(session, true)
            .Combine(MongoDataContext.MapFlatten(y => y.BuildPrimaryKeyPredicate(exist.ReduceOrDefault())))
            .Combine(ctok, true, CancellationToken.None)
            .Map(x => (Set: x.Item1.Item1.Item1, Session: x.Item1.Item1.Item2, x.Item1.Item2.Predicate, Ctok: x.Item2))
            .ExecuteAsync(
               async x =>
               {
                  if (x.Session == null)
                  {
                     await x.Set.DeleteOneAsync(x.Predicate, x.Ctok);
                  }
                  else
                  {
                     await x.Set.DeleteOneAsync(x.Session, x.Predicate, cancellationToken: x.Ctok);
                  }
               });
      }

      private Option<IClientSessionHandle> GetSessionOrNone()
      {
         return MongoDataContext
            .MapFlatten(
               x =>
               {
                  if (x.ExplicitSession.IsSome()) return x.ExplicitSession;
                  return x.ImplicitSession.IsSome() ? x.ImplicitSession : None.Value;
               });
      }

      private Option<(T MatchValidatedEntity, string[] PropertyNames, T InputEntity)>
         GetValidatedEntityForCreate(Option<T> entity, Option<IClientSessionHandle> session)
      {
         return MongoDataContext
            .Combine(entity)
            .Map(x => (DataContext: x.Item1, InputEntity: x.Item2))
            .MapFlatten(
               x =>
               {
                  return x.DataContext.BuildPrimaryKeyPredicate(x.InputEntity)
                     .Map(
                        y => (
                           Exist: InternalGetFirstOrDefault(y.Predicate, session).ReduceOrDefault(),
                           y.PropertyNames,
                           x.InputEntity
                        ))
                     .IfMap(
                        y => y.Exist != null,
                        y => (MatchValidatedEntity: y.Exist, y.PropertyNames, y.InputEntity))
                     .ElseMapFlatten(
                        y =>
                        {
                           return x.DataContext.BuildAlternateKeyPredicate(y.InputEntity)
                              .Map(
                                 z => (
                                    Exist: InternalGetFirstOrDefault(z.Predicate, session).ReduceOrDefault(),
                                    z.PropertyNames,
                                    y.InputEntity
                                 ))
                              .IfMap(
                                 z => z.Exist != null,
                                 z => (MatchValidatedEntity: z.Exist, z.PropertyNames, z.InputEntity))
                              .ElseMap(z => (MatchValidatedEntity: (T)null, z.PropertyNames, z.InputEntity))
                              .Output;
                        })
                     .Output;
               });
      }

      private Task<Option<(T MatchValidatedEntity, string[] PropertyNames, T InputEntity)>>
         GetValidatedEntityForCreateAsync(
            Option<T> entity,
            Option<IClientSessionHandle> session,
            Option<CancellationToken> ctok)
      {
         return MongoDataContext
            .Combine(entity)
            .Combine(ctok, true, CancellationToken.None)
            .Map(x => (DataContext: x.Item1.Item1, InputEntity: x.Item1.Item2, Ctok: x.Item2))
            .MapFlattenAsync(
               async x =>
               {
                  return
                     (
                        await
                           (
                              await x.DataContext.BuildPrimaryKeyPredicate(x.InputEntity)
                                 .MapAsync(
                                    async y => (
                                       Exist: (await InternalGetFirstOrDefaultAsync(y.Predicate, session, x.Ctok)).ReduceOrDefault(),
                                       y.PropertyNames,
                                       x.InputEntity
                                    ))
                           )
                           .IfMap(
                              y => y.Exist != null,
                              y => (MatchValidatedEntity: y.Exist, y.PropertyNames, y.InputEntity))
                           .ElseMapFlattenAsync(
                              async y =>
                              {
                                 return
                                    (
                                       await x.DataContext.BuildAlternateKeyPredicate(y.InputEntity)
                                          .MapAsync(
                                             async z => (
                                                Exist: (await InternalGetFirstOrDefaultAsync(z.Predicate, session, x.Ctok))
                                                .ReduceOrDefault(),
                                                z.PropertyNames,
                                                y.InputEntity
                                             ))
                                    )
                                    .IfMap(
                                       z => z.Exist != null,
                                       z => (MatchValidatedEntity: z.Exist, z.PropertyNames, z.InputEntity))
                                    .ElseMap(
                                       z => (MatchValidatedEntity: (T)null, z.PropertyNames, z.InputEntity))
                                    .Output;
                              })
                     )
                     .Output;
               });
      }

      private Option<(T Entity, T Exist)> GetValidatedEntityForDelete(
         Option<T> entity,
         Option<IClientSessionHandle> session)
      {
         return entity
            .Combine(MongoDataContext.MapFlatten(x => x.Collection<T>()))
            .Map(x => (Entity: x.Item1, Predicate: x.Item1.BuildPredicate<T>(), Set: x.Item2))
            .Map(
               x =>
               {
                  var exist = InternalGetFirstOrDefault(x.Predicate, session).ReduceOrDefault();
                  return (x.Entity, Exist: exist);
               });
      }

      private async Task<Option<(T Entity, T Exist)>> GetValidatedEntityForDeleteAsync(
         Option<T> entity,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return await entity
            .Combine(MongoDataContext.MapFlatten(x => x.Collection<T>()))
            .Combine(ctok, true, CancellationToken.None)
            .Map(x => (Entity: x.Item1.Item1, Predicate: x.Item1.Item1.BuildPredicate<T>(), Set: x.Item1.Item2, Ctok: x.Item2))
            .MapAsync(
               async x =>
               {
                  var exist = (await InternalGetFirstOrDefaultAsync(x.Predicate, session, x.Ctok)).ReduceOrDefault();
                  return (x.Entity, Exist: exist);
               });
      }

      private Option<(T MatchValidatedEntity, string[] PropertyNames, bool Found, T InputEntity)>
         GetValidatedEntityForUpdate(Option<T> entity, Option<IClientSessionHandle> session)
      {
         return MongoDataContext
            .Combine(entity)
            .Map(x => (DataContext: x.Item1, InputEntity: x.Item2))
            .MapFlatten(
               x =>
               {
                  return RelationalRepositoryHelper<T>.GiveValidatedEntityForUpdateResult(
                     x.DataContext.BuildPrimaryKeyPredicate(x.InputEntity)
                        .Combine(x.DataContext.BuildAlternateKeyPredicate(x.InputEntity))
                        .Combine( /*Properties are PK or AK*/
                           y => RelationalRepositoryHelper<T>.GetKeyPropertyNamesInBetween(
                              y.Item1.PropertyNames,
                              y.Item2.PropertyNames))
                        .Map(y => (PkResult: y.Item1.Item1, AkResult: y.Item1.Item2, KeyProperties: y.Item2))
                        .Map(
                           y => (
                              ExistByPkEntity: InternalGetFirstOrDefault(y.PkResult.Predicate, session).ReduceOrDefault() /*Search by PK*/,
                              ExistByAkEntity: InternalGetFirstOrDefault(y.AkResult.Predicate, session).ReduceOrDefault() /*Search by AK*/,
                              RealKeyPropertyNames: y.KeyProperties,
                              PkPropertyNames: y.PkResult.PropertyNames,
                              x.InputEntity
                           )));
               });
      }

      private Task<Option<(T MatchValidatedEntity, string[] PropertyNames, bool Found, T InputEntity)>>
         GetValidatedEntityForUpdateAsync(Option<T> entity, Option<IClientSessionHandle> session, Option<CancellationToken> ctok)
      {
         return MongoDataContext
            .Combine(entity)
            .Combine(ctok, true, CancellationToken.None)
            .Map(x => (DataContext: x.Item1.Item1, InputEntity: x.Item1.Item2, Ctok: x.Item2))
            .MapFlattenAsync(
               async x =>
               {
                  return RelationalRepositoryHelper<T>.GiveValidatedEntityForUpdateResult(
                     await x.DataContext.BuildPrimaryKeyPredicate(x.InputEntity)
                        .Combine(x.DataContext.BuildAlternateKeyPredicate(x.InputEntity))
                        .Combine( /*Properties are PK or AK*/
                           y => RelationalRepositoryHelper<T>.GetKeyPropertyNamesInBetween(
                              y.Item1.PropertyNames,
                              y.Item2.PropertyNames))
                        .Map(y => (PkResult: y.Item1.Item1, AkResult: y.Item1.Item2, KeyProperties: y.Item2))
                        .MapAsync(
                           async y => (
                              ExistByPkEntity:
                              (await InternalGetFirstOrDefaultAsync(y.PkResult.Predicate, session, x.Ctok))
                              .ReduceOrDefault() /*Search by PK*/,
                              ExistByAkEntity:
                              (await InternalGetFirstOrDefaultAsync(y.AkResult.Predicate, session, x.Ctok))
                              .ReduceOrDefault() /*Search by AK*/,
                              RealKeyPropertyNames: y.KeyProperties,
                              PkPropertyNames: y.PkResult.PropertyNames,
                              x.InputEntity
                           )));
               });
      }

      private Option<T> InternalGetFirstOrDefault(
         Option<Expression<Func<T, bool>>> predicate,
         Option<IClientSessionHandle> session)
      {
         return InternalGetFirstOrDefault(new Some<Expression<Func<T, T>>>(x => x), predicate, session);
      }

      private Option<TResult> InternalGetFirstOrDefault<TResult>(
         Option<Expression<Func<T, TResult>>> selector,
         Option<Expression<Func<T, bool>>> predicate,
         Option<IClientSessionHandle> session)
      {
         return MongoDataContext.MapFlatten(x => x.Collection<T>())
            .Combine(selector)
            .Combine(predicate, true, _ => true)
            .Combine(session, true)
            .Map(
               x => (
                  Set: x.Item1.Item1.Item1,
                  Selector: x.Item1.Item1.Item2,
                  Predicate: x.Item1.Item2,
                  Session: x.Item2
               ))
            .IfMap(x => x.Session == null, x => x.Set.Find(x.Predicate).Project(x.Selector).FirstOrDefault())
            .ElseMap(x => x.Set.Find(x.Session, x.Predicate).Project(x.Selector).FirstOrDefault())
            .Output;
      }

      private Task<Option<T>> InternalGetFirstOrDefaultAsync(
         Option<Expression<Func<T, bool>>> predicate,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return InternalGetFirstOrDefaultAsync(new Some<Expression<Func<T, T>>>(x => x), predicate, session, ctok);
      }

      private async Task<Option<TResult>> InternalGetFirstOrDefaultAsync<TResult>(
         Option<Expression<Func<T, TResult>>> selector,
         Option<Expression<Func<T, bool>>> predicate,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return (
            await MongoDataContext.MapFlatten(x => x.Collection<T>())
               .Combine(selector)
               .Combine(predicate, true, _ => true)
               .Combine(session, true)
               .Combine(ctok, true, CancellationToken.None)
               .Map(
                  x => (
                     Set: x.Item1.Item1.Item1.Item1,
                     Selector: x.Item1.Item1.Item1.Item2,
                     Predicate: x.Item1.Item1.Item2,
                     Session: x.Item1.Item2,
                     Ctok: x.Item2
                  ))
               .IfMapAsync(x => x.Session == null, x => x.Set.Find(x.Predicate).Project(x.Selector).FirstOrDefaultAsync(x.Ctok))
               .ElseMapAsync(x => x.Set.Find(x.Session, x.Predicate).Project(x.Selector).FirstOrDefaultAsync(x.Ctok))
         ).Output;
      }

      private Option<List<TResult>> InternalGetList<TResult>(
         Option<Expression<Func<T, TResult>>> selector,
         Option<Expression<Func<T, bool>>> predicate,
         Option<Func<IFindFluent<T, T>, IOrderedFindFluent<T, T>>> sortBy,
         Option<IClientSessionHandle> session)
      {
         return MongoDataContext.MapFlatten(x => x.Collection<T>())
            .Combine(MongoDataContext.GetFindOptions())
            .Combine(selector)
            .Combine(predicate, true, _ => true)
            .Combine(sortBy, true)
            .Combine(session, true)
            .Map(
               x => (
                  Set: x.Item1.Item1.Item1.Item1.Item1,
                  FindOptions: x.Item1.Item1.Item1.Item1.Item2,
                  Selector: x.Item1.Item1.Item1.Item2,
                  Predicate: x.Item1.Item1.Item2,
                  SortBy: x.Item1.Item2,
                  Session: x.Item2
               ))
            .IfMap(
               x => x.SortBy != null,
               x => x.Session == null
                  ? x.SortBy(x.Set.Find(x.Predicate, x.FindOptions)).Project(x.Selector).ToList()
                  : x.SortBy(x.Set.Find(x.Session, x.Predicate, x.FindOptions)).Project(x.Selector).ToList())
            .ElseMap(
               x => x.Session == null
                  ? x.Set.Find(x.Predicate, x.FindOptions).Project(x.Selector).ToList()
                  : x.Set.Find(x.Session, x.Predicate, x.FindOptions).Project(x.Selector).ToList());
      }

      private async Task<Option<List<TResult>>> InternalGetListAsync<TResult>(
         Option<Expression<Func<T, TResult>>> selector,
         Option<Expression<Func<T, bool>>> predicate,
         Option<Func<IFindFluent<T, T>, IOrderedFindFluent<T, T>>> sortBy,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         var _ = await MongoDataContext.MapFlatten(x => x.Collection<T>())
            .Combine(MongoDataContext.GetFindOptions())
            .Combine(selector)
            .Combine(predicate, true, x => true)
            .Combine(sortBy, true)
            .Combine(session, true)
            .Combine(ctok, true, CancellationToken.None)
            .Map(
               x => (
                  Set: x.Item1.Item1.Item1.Item1.Item1.Item1,
                  FindOptions: x.Item1.Item1.Item1.Item1.Item1.Item2,
                  Selector: x.Item1.Item1.Item1.Item1.Item2,
                  Predicate: x.Item1.Item1.Item1.Item2,
                  SortBy: x.Item1.Item1.Item2,
                  Session: x.Item1.Item2,
                  Ctok: x.Item2
               ))
            .IfMapAsync(
               x => x.SortBy != null,
               x => x.Session == null
                  ? x.SortBy(x.Set.Find(x.Predicate, x.FindOptions)).Project(x.Selector).ToListAsync()
                  : x.SortBy(x.Set.Find(x.Session, x.Predicate, x.FindOptions)).Project(x.Selector).ToListAsync())
            .ElseMapAsync(
               x => x.Session == null
                  ? x.Set.Find(x.Predicate, x.FindOptions).Project(x.Selector).ToListAsync()
                  : x.Set.Find(x.Session, x.Predicate, x.FindOptions).Project(x.Selector).ToListAsync());
         return _.Output;
      }

      private Option<List<KeyValue>> InternalGetLookups(
         Option<string> idProperty,
         Option<string> valueProperty,
         Option<bool> useValueAsId,
         Option<Expression<Func<T, bool>>> predicate,
         Option<IClientSessionHandle> session)
      {
         return MongoDataContext.MapFlatten(x => x.Collection<T>())
            .Combine(MongoDataContext.GetFindOptions())
            .Combine(idProperty)
            .Combine(valueProperty)
            .Combine(useValueAsId, true)
            .Combine(predicate, true, _ => true)
            .Combine(session, true)
            .Map(
               x => (
                  Set: x.Item1.Item1.Item1.Item1.Item1.Item1,
                  FindOptions: x.Item1.Item1.Item1.Item1.Item1.Item2,
                  IdProperty: x.Item1.Item1.Item1.Item1.Item2,
                  ValueProperty: x.Item1.Item1.Item1.Item2,
                  UseValueAsId: x.Item1.Item1.Item2,
                  Predicate: x.Item1.Item2,
                  OrderByField: x.Item1.Item1.Item2 ? x.Item1.Item1.Item1.Item2 : x.Item1.Item1.Item1.Item1.Item2,
                  Session: x.Item2
               ))
            .Map(
               x =>
               {
                  var findFluent = x.Session == null
                     ? x.Set.Find(x.Predicate, x.FindOptions)
                     : x.Set.Find(x.Session, x.Predicate, x.FindOptions);
                  return findFluent
                     .Sort(Builders<T>.Sort.Ascending(x.OrderByField))
                     .Project(y => RepositoryHelper<T>.ToKeyValue(y, x.IdProperty, x.ValueProperty, x.UseValueAsId))
                     .ToList();
               });
      }

      private Task<Option<List<KeyValue>>> InternalGetLookupsAsync(
         Option<string> idProperty,
         Option<string> valueProperty,
         Option<bool> useValueAsId,
         Option<Expression<Func<T, bool>>> predicate,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return MongoDataContext.MapFlatten(x => x.Collection<T>())
            .Combine(MongoDataContext.GetFindOptions())
            .Combine(idProperty)
            .Combine(valueProperty)
            .Combine(useValueAsId, true)
            .Combine(predicate, true, x => true)
            .Combine(session, true)
            .Combine(ctok, true, CancellationToken.None)
            .Map(
               x => (
                  Set: x.Item1.Item1.Item1.Item1.Item1.Item1.Item1,
                  FindOptions: x.Item1.Item1.Item1.Item1.Item1.Item1.Item2,
                  IdProperty: x.Item1.Item1.Item1.Item1.Item1.Item2,
                  ValueProperty: x.Item1.Item1.Item1.Item1.Item2,
                  UseValueAsId: x.Item1.Item1.Item1.Item2,
                  Predicate: x.Item1.Item1.Item2,
                  OrderByField: x.Item1.Item1.Item1.Item2 ? x.Item1.Item1.Item1.Item1.Item2 : x.Item1.Item1.Item1.Item1.Item1.Item2,
                  Session: x.Item1.Item2,
                  Ctok: x.Item2
               ))
            .MapAsync(
               x =>
               {
                  var findFluent = x.Session == null
                     ? x.Set.Find(x.Predicate, x.FindOptions)
                     : x.Set.Find(x.Session, x.Predicate, x.FindOptions);
                  return findFluent
                     .Sort(Builders<T>.Sort.Ascending(x.OrderByField))
                     .Project(y => RepositoryHelper<T>.ToKeyValue(y, x.IdProperty, x.ValueProperty, x.UseValueAsId))
                     .ToListAsync(x.Ctok);
               });
      }

      private Option<IPaged<TResult>> InternalGetPaged<TResult>(
         Option<Expression<Func<T, TResult>>> selector,
         Option<string[]> searchFields,
         Option<int> pageIndex,
         Option<int> pageSize,
         Option<string[]> sorts,
         Option<string> keyword,
         Option<PageIndexFrom> indexFrom,
         Option<Expression<Func<T, bool>>> predicate,
         Option<IClientSessionHandle> session)
      {
         var pageQuery = PageQuery.Get(pageIndex, pageSize, sorts, keyword, indexFrom, searchFields);
         var defaultSortKeys = RelationalRepositoryHelper<T>.GetSortKeys(MongoDataContext.ReduceOrDefault())
            .ReduceOrDefault()
            .ToArrayOfOptions();
         return GetPerPageFindFluent(
               MongoDataContext.MapFlatten(x => x.Collection<T>()),
               MongoDataContext.MapFlatten(x => x.FindOptions),
               selector,
               pageQuery.ReduceOrDefault(),
               defaultSortKeys,
               predicate,
               session)
            .Combine(pageQuery)
            .Map(x => (FindFluent: x.Item1, PageQuery: x.Item2))
            .Map(
               x => new Paged<TResult>(
                  x.FindFluent.ToList(),
                  x.PageQuery.PageIndex,
                  x.PageQuery.PageSize,
                  x.PageQuery.IndexFrom,
                  x.FindFluent.CountDocuments()
               ))
            .ReduceOrDefault();
      }

      private async Task<Option<IPaged<TResult>>> InternalGetPagedAsync<TResult>(
         Option<Expression<Func<T, TResult>>> selector,
         Option<string[]> searchFields,
         Option<int> pageIndex,
         Option<int> pageSize,
         Option<string[]> sorts,
         Option<string> keyword,
         Option<PageIndexFrom> indexFrom,
         Option<Expression<Func<T, bool>>> predicate,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         var pageQuery = PageQuery.Get(pageIndex, pageSize, sorts, keyword, indexFrom, searchFields);
         var defaultSortKeys = RelationalRepositoryHelper<T>.GetSortKeys(MongoDataContext.ReduceOrDefault())
            .ReduceOrDefault()
            .ToArrayOfOptions();
         var _ = await GetPerPageFindFluent(
               MongoDataContext.MapFlatten(x => x.Collection<T>()),
               MongoDataContext.MapFlatten(x => x.FindOptions),
               selector,
               pageQuery.ReduceOrDefault(),
               defaultSortKeys,
               predicate,
               session)
            .Combine(pageQuery)
            .Combine(ctok, true, CancellationToken.None)
            .Map(x => (FindFluent: x.Item1.Item1, PageQuery: x.Item1.Item2, Ctok: x.Item2))
            .MapAsync(
               async x => new Paged<TResult>(
                  await x.FindFluent.ToListAsync(x.Ctok),
                  x.PageQuery.PageIndex,
                  x.PageQuery.PageSize,
                  x.PageQuery.IndexFrom,
                  await x.FindFluent.CountDocumentsAsync(x.Ctok)
               ));
         return _.ReduceOrDefault();
      }

      private Option<long> InternalLongCount(Option<Expression<Func<T, bool>>> predicate, Option<IClientSessionHandle> session)
      {
         return MongoDataContext.MapFlatten(x => x.Collection<T>())
            .Combine(predicate, true)
            .Combine(session, true)
            .Map(x => (Set: x.Item1.Item1, Predicate: x.Item1.Item2, Session: x.Item2))
            .Map(
               x =>
               {
                  var filter = x.Predicate == null ? Builders<T>.Filter.Empty : new ExpressionFilterDefinition<T>(x.Predicate);
                  return x.Session == null
                     ? x.Set.CountDocuments(filter)
                     : x.Set.CountDocuments(x.Session, filter);
               });
      }

      private async Task<Option<long>> InternalLongCountAsync(
         Option<Expression<Func<T, bool>>> predicate,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return await MongoDataContext.MapFlatten(x => x.Collection<T>())
            .Combine(predicate, true)
            .Combine(session, true)
            .Combine(ctok, true, CancellationToken.None)
            .Map(x => (Set: x.Item1.Item1.Item1, Predicate: x.Item1.Item1.Item2, Session: x.Item1.Item2, Ctok: x.Item2))
            .MapAsync(
               x =>
               {
                  var filter = x.Predicate == null ? Builders<T>.Filter.Empty : new ExpressionFilterDefinition<T>(x.Predicate);
                  return x.Session == null
                     ? x.Set.CountDocumentsAsync(filter, cancellationToken: x.Ctok)
                     : x.Set.CountDocumentsAsync(x.Session, filter, cancellationToken: x.Ctok);
               });
      }

      private Option<Dictionary<string, object>> ProcessCreateAndGetResult(
         Option<(T MatchValidatedEntity, string[] PropertyNames, T InputEntity)> validated,
         Option<Func<T, string>> existMessageFunc,
         Option<IClientSessionHandle> session)
      {
         return validated
            .Combine(existMessageFunc, true)
            .Map(
               x => (
                  x.Item1.MatchValidatedEntity,
                  x.Item1.PropertyNames,
                  x.Item1.InputEntity,
                  MessageFunc: x.Item2
               ))
            .IfMapFlatten(
               RelationalRepositoryHelper<T>.IfCreateError,
               x => RepositoryHelper<T>.ThrowCreateErrorExistingEntity(x, DefaultExistMessageFunc))
            .ElseMapFlatten(x => CreateAndGetKeyValues(x, session))
            .Output;
      }

      private async Task<Option<Dictionary<string, object>>> ProcessCreateAndGetResultAsync(
         Task<Option<(T MatchValidatedEntity, string[] PropertyNames, T InputEntity)>> asyncValidated,
         Option<Func<T, string>> existMessageFunc,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         var validated = await asyncValidated;
         var _ = await validated
            .Combine(existMessageFunc, true)
            .Map(
               x => (
                  x.Item1.MatchValidatedEntity,
                  x.Item1.PropertyNames,
                  x.Item1.InputEntity,
                  MessageFunc: x.Item2
               ))
            .IfMapFlatten(
               RelationalRepositoryHelper<T>.IfCreateError,
               x => RepositoryHelper<T>.ThrowCreateErrorExistingEntity(x, DefaultExistMessageFunc))
            .ElseMapFlattenAsync(x => CreateAndGetKeyValuesAsync(x, session, ctok));
         return _.Output;
      }

      private Option<bool> ProcessDeleteAndGetResult(Option<(T Entity, T Exist)> validated, Option<IClientSessionHandle> session)
      {
         return validated
            .IfMapFlatten(
               x => x.Exist == null,
               x =>
               {
                  var properties = PropertyHelper.GetProperties(x.Entity)
                     .Where(y => y.GetValue(x.Entity) != null)
                     .Select(y => y.Name)
                     .ToArray();
                  return Fail<bool>.Throw(new KeyNotFoundException(DefaultNotFoundMessageFunc(x.Entity, properties)));
               })
            .ElseMapFlatten(x => DeleteEntry(x.Exist, session))
            .Output;
      }

      private async Task<Option<bool>> ProcessDeleteAndGetResultAsync(
         Task<Option<(T Entity, T Exist)>> asyncValidated,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         var validated = await asyncValidated;
         var _ = await validated
            .IfMapFlatten(
               x => x.Exist == null,
               x =>
               {
                  var properties = PropertyHelper.GetProperties(x.Entity)
                     .Where(y => y.GetValue(x.Entity) != null)
                     .Select(y => y.Name)
                     .ToArray();
                  return Fail<bool>.Throw(new KeyNotFoundException(DefaultNotFoundMessageFunc(x.Entity, properties)));
               })
            .ElseMapFlattenAsync(x => DeleteEntryAsync(x.Exist, session, ctok));
         return _.Output;
      }

      private Option<Dictionary<string, object>> ProcessUpdateAndGetResult(
         Option<(T MatchValidatedEntity, string[] PropertyNames, bool Found, T InputEntity)> validated,
         Option<Func<T, string>> existMessageFunc,
         Option<IClientSessionHandle> session)
      {
         return validated.Combine(existMessageFunc, true)
            .Map(
               x => (
                  x.Item1.MatchValidatedEntity,
                  x.Item1.PropertyNames,
                  x.Item1.Found,
                  x.Item1.InputEntity,
                  MessageFunc: x.Item2
               ))
            .IfMapFlatten(
               RelationalRepositoryHelper<T>.IfUpdateError,
               x => RepositoryHelper<T>.ThrowUpdateError(x, DefaultExistMessageFunc))
            .ElseMapFlatten(x => UpdateAndGetKeyValues(x, session))
            .Output;
      }

      private async Task<Option<Dictionary<string, object>>> ProcessUpdateAndGetResultAsync(
         Task<Option<(T MatchValidatedEntity, string[] PropertyNames, bool Found, T InputEntity)>> asyncValidated,
         Option<Func<T, string>> existMessageFunc,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         var validated = await asyncValidated;
         var _ = await validated
            .Combine(existMessageFunc, true)
            .Map(
               x => (
                  x.Item1.MatchValidatedEntity,
                  x.Item1.PropertyNames,
                  x.Item1.Found,
                  x.Item1.InputEntity,
                  MessageFunc: x.Item2
               ))
            .IfMapFlatten(
               RelationalRepositoryHelper<T>.IfUpdateError,
               x => RepositoryHelper<T>.ThrowUpdateError(x, DefaultExistMessageFunc))
            .ElseMapFlattenAsync(x => UpdateAndGetKeyValuesAsync(x, session, ctok));
         return _.Output;
      }

      private Option<Dictionary<string, object>> UpdateAndGetKeyValues(
         Option<(T MatchValidatedEntity, string[] PropertyNames, bool Found, T InputEntity, Func<T, string> MessageFunc)> validated,
         Option<IClientSessionHandle> session)
      {
         return validated
            .Map(
               x => (
                  IsSuccess: UpdateEntry(x.InputEntity, x.MatchValidatedEntity, session).ReduceOrDefault(),
                  x.PropertyNames,
                  x.InputEntity
               ))
            .IfMapFlatten(
               y => y.IsSuccess,
               y => RepositoryHelper<T>.GetKeysAndValues(y.PropertyNames, y.InputEntity))
            .Output;
      }

      private async Task<Option<Dictionary<string, object>>> UpdateAndGetKeyValuesAsync(
         Option<(T MatchValidatedEntity, string[] PropertyNames, bool Found, T InputEntity, Func<T, string> MessageFunc)> validated,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return
            (
               await validated
                  .MapAsync(
                     async x => (
                        IsSuccess: (await UpdateEntryAsync(x.InputEntity, x.MatchValidatedEntity, session, ctok)).ReduceOrDefault(),
                        x.PropertyNames,
                        x.InputEntity
                     ))
            )
            .IfMapFlatten(
               y => y.IsSuccess,
               y => RepositoryHelper<T>.GetKeysAndValues(y.PropertyNames, y.InputEntity))
            .Output;
      }

      private Option<bool> UpdateEntry(Option<T> entity, Option<T> exist, Option<IClientSessionHandle> session)
      {
         return MongoDataContext
            .MapFlatten(x => x.Collection<T>())
            .Combine(session, true)
            .Map(x => (Set: x.Item1, Session: x.Item2))
            .Execute(
               x =>
               {
                  MongoDataContext
                     .MapFlatten(y => y.BuildPrimaryKeyPredicate(exist.ReduceOrDefault()))
                     .Execute(
                        y =>
                        {
                           if (x.Session == null)
                           {
                              x.Set.UpdateOne(y.Predicate, new ObjectUpdateDefinition<T>(entity));
                           }
                           else
                           {
                              x.Set.UpdateOne(x.Session, y.Predicate, new ObjectUpdateDefinition<T>(entity));
                           }
                        });
               });
      }

      private Task<Option<bool>> UpdateEntryAsync(
         Option<T> entity,
         Option<T> exist,
         Option<IClientSessionHandle> session,
         Option<CancellationToken> ctok)
      {
         return MongoDataContext
            .MapFlatten(x => x.Collection<T>())
            .Combine(session, true)
            .Combine(ctok, true, CancellationToken.None)
            .Map(x => (Set: x.Item1.Item1, Session: x.Item1.Item2, Ctok: x.Item2))
            .ExecuteAsync(
               async x =>
               {
                  await MongoDataContext
                     .MapFlatten(y => y.BuildPrimaryKeyPredicate(exist.ReduceOrDefault()))
                     .ExecuteAsync(
                        async y =>
                        {
                           if (x.Session == null)
                           {
                              await x.Set.UpdateOneAsync(y.Predicate, new ObjectUpdateDefinition<T>(entity), cancellationToken: x.Ctok);
                           }
                           else
                           {
                              await x.Set.UpdateOneAsync(
                                 x.Session,
                                 y.Predicate,
                                 new ObjectUpdateDefinition<T>(entity),
                                 cancellationToken: x.Ctok);
                           }
                        });
               });
      }

      #endregion
   }
}