namespace IntoItIf.Base.Services.Entities.Args
{
   using System;
   using System.Linq;
   using System.Linq.Expressions;
   using System.Threading.Tasks;
   using Base.Helpers;
   using Domain.Entities;
   using Domain.Options;

   public class ReadOneInterceptorArgs<T>
   {
      #region Constructors and Destructors

      public ReadOneInterceptorArgs(
         Func<Option<T>, object, Task> oneValidation,
         Option<Expression<Func<T, bool>>> predicate,
         Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
         Func<IQueryable<T>, IQueryable<T>> include)
      {
         OneValidation = oneValidation;
         Predicate = predicate;
         OrderBy = orderBy;
         Include = include;
      }

      #endregion

      #region Public Properties

      public Func<Option<T>, object, Task> OneValidation { get; }
      public Option<Expression<Func<T, bool>>> Predicate { get; }
      public Func<IQueryable<T>, IOrderedQueryable<T>> OrderBy { get; }
      public Func<IQueryable<T>, IQueryable<T>> Include { get; }

      public bool IsView
      {
         get
         {
            var typeofT = typeof(T);
            return typeofT.IsAssignableTo<IViewEntity>();
         }
      }

      #endregion
   }
}