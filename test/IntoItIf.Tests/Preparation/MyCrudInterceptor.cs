﻿namespace IntoItIf.Tests.Preparation
{
   using System;
   using Base.Services.Entities.Args;
   using Base.Services.Entities.Interceptors;

   public class MyCrudInterceptor : ICrudInterceptor
   {
      #region Public Methods and Operators

      public SaveInterceptorArgs<T, TDto> OpenForCreate<T, TDto>()
      {
         throw new NotImplementedException();
      }

      public DeleteInterceptorArgs<T> OpenForDelete<T>()
      {
         throw new NotImplementedException();
      }

      public ReadLookupInterceptorArgs<T> OpenForReadLookup<T>()
      {
         throw new NotImplementedException();
      }

      public ReadOneInterceptorArgs<T> OpenForReadOne<T>()
      {
         throw new NotImplementedException();
      }

      public ReadPagedInterceptorArgs<T> OpenForReadPaged<T>()
      {
         throw new NotImplementedException();
      }

      public SaveInterceptorArgs<T, TDto> OpenForUpdate<T, TDto>()
      {
         throw new NotImplementedException();
      }

      #endregion
   }
}