﻿namespace IntoItIf.Tests.Preparation.Preparation
{
   using System.Collections.Generic;
   using Base.Domain.Entities;
   using Base.Domain.Options;

   public class MyDto : BaseDto<MyDto, MyDtoFluentValidator>
   {
      #region Constructors and Destructors

      public MyDto(Option<int> id, Option<string> name)
      {
         Id = id.ReduceOrDefault();
         Name = name.ReduceOrDefault();
      }

      private MyDto()
      {
      }

      #endregion

      #region Public Properties

      public int Id { get; set; }
      public string Name { get; set; }

      #endregion

      #region Methods

      protected override IEnumerable<object> GetEqualityComponents()
      {
         yield return Id;
         yield return Name;
      }

      #endregion
   }
}