﻿namespace IntoItIf.Dsl.Exceptions
{
   using System;

   public class PreSaveException : Exception
   {
      #region Constructors and Destructors

      public PreSaveException(string message) : base(message)
      {
      }

      #endregion
   }
}