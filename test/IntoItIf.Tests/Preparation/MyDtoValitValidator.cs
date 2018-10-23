﻿namespace IntoItIf.Tests.Preparation
{
   using Base.Validations;
   using Valit;

   public class MyDtoValitValidator : BaseValitValidator<MyDto>
   {
      public MyDtoValitValidator()
      {
         Valitator = ValitRules<MyDto>.Create()
            .Ensure(x => x.Id, x => x.IsNonZero())
            .Ensure(x => x.Name, x => x.Required())
            .CreateValitator();
      }

      protected override IValitator<MyDto> Valitator { get; }
   }
}