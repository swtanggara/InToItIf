#if NETSTANDARD2_0
namespace IntoItIf.Ef.Helpers
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Linq.Expressions;
   using System.Reflection;
   using Base.Helpers;
   using Microsoft.EntityFrameworkCore;

   public static class EfCoreDbContextKeys
   {
      #region Methods

      public static (Expression<Func<T, bool>> Predicate, string[] PropertyNames) BuildAlternateKeyPredicate<T>(
         this DbContext dbContext,
         T entity)
         where T : class
      {
         var akProperties = dbContext.GetAlternateKeyProperties<T>()
            .Select(x => x.Name)
            .ToArray();
         var result = entity.BuildEqualPredicateFor(akProperties);
         return (result, akProperties);
      }

      public static (Expression<Func<T, bool>> Predicate, string[] PropertyNames) BuildPrimaryKeyPredicate<T>(
         this DbContext dbContext,
         T entity)
         where T : class
      {
         var pkProperties = dbContext.GetPrimaryKeyProperties<T>()
            .Select(x => x.Name)
            .ToArray();
         var result = entity.BuildEqualPredicateFor(pkProperties);
         return (result, pkProperties);
      }

      public static IEnumerable<PropertyInfo> GetAlternateKeyProperties<T>(this DbContext dbContext)
      {
         var result = dbContext.GetAlternateKeyProperties(typeof(T));
         return result;
      }

      public static IEnumerable<PropertyInfo> GetPrimaryKeyProperties<T>(this DbContext dbContext)
         where T : class
      {
         var result = dbContext.GetPrimaryKeyProperties(typeof(T));
         return result;
      }

      private static IEnumerable<PropertyInfo> GetAlternateKeyProperties(this DbContext dbContext, Type type)
      {
         var entityType = dbContext.Model.FindEntityType(type);
         var allKeys = entityType.GetKeys();
         var pkKey = entityType.FindPrimaryKey();
         var onlyAkKeys = allKeys.SelectMany(x => x.Properties)
            .Except(pkKey.Properties);
         return onlyAkKeys.Select(x => x.PropertyInfo);
      }

      private static IEnumerable<PropertyInfo> GetPrimaryKeyProperties(this DbContext dbContext, Type type)
      {
         var entityType = dbContext.Model.FindEntityType(type);
         var key = entityType.FindPrimaryKey();
         return key.Properties.Select(x => x.PropertyInfo);
      }

      #endregion
   }
}
#endif