namespace IntoItIf.Base.Services.Entities.Services
{
   using System.Collections.Generic;
   using System.Threading;
   using System.Threading.Tasks;
   using Domain.Entities;
   using Domain.Options;

   public interface IEntityUpdateService<TDto>
      where TDto : class, IDto
   {
      #region Public Methods and Operators

      Option<Dictionary<string, object>> UpdateEntity(Option<TDto> dto);

      Task<Option<Dictionary<string, object>>> UpdateEntityAsync(Option<TDto> dto, Option<CancellationToken> ctok);

      #endregion
   }
}