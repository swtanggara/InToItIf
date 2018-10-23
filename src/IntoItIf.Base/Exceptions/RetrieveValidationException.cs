﻿namespace IntoItIf.Base.Exceptions
{
   using System;

   public class RetrieveValidationException : Exception
   {
      #region Constructors and Destructors

      public RetrieveValidationException(string message) : base(message)
      {
      }

      #endregion
   }
}