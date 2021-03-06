﻿namespace IntoItIf.Base.UnitOfWork
{
   using System;
   using System.Collections.Generic;
   using DataContexts;
   using Repositories;

   public abstract class Uow<TDataContext> : IUow
      where TDataContext : class, IDataContext
   {
      #region Fields

      private bool _disposed;

      #endregion

      #region Constructors and Destructors

      protected Uow(TDataContext dataContext)
      {
         DataContext = dataContext;
      }

      #endregion

      #region Properties

      protected internal Dictionary<Type, object> Repositories { get; set; }

      protected TDataContext DataContext { get; }

      #endregion

      #region Public Methods and Operators

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      public TRepository GetRepository<TRepository, T>()
         where TRepository : class, IRepository<T>
         where T : class
      {
         if (Repositories == null) Repositories = new Dictionary<Type, object>();

         var type = typeof(T);
         if (!Repositories.ContainsKey(type))
         {
            Repositories[type] = InitRepository<TRepository, T>();
         }

         return (TRepository)Repositories[type];
      }

      #endregion

      #region Methods

      protected abstract TRepository InitRepository<TRepository, T>()
         where TRepository : class, IRepository<T>
         where T : class;

      private void Dispose(bool disposing)
      {
         if (!_disposed && disposing)
         {
            // clear repositories
            Repositories?.Clear();
            // dispose the db context.
            DataContext?.Dispose();
         }

         _disposed = true;
      }

      #endregion
   }
}