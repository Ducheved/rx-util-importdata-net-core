using System.Collections.Generic;

namespace ImportData.IntegrationServicesClient.Models
{
  [EntityName("Город")]
  public class ICities : IEntity
  {
    [PropertyOptions("Населенный пункт", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    new public string Name { get; set; }
    public string Status { get; set; }

    new public static ICities FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch[Constants.KeyAttributes.Name];
      return BusinessLogic.GetEntityWithFilter<ICities>(x => x.Name == name, exceptionList, logger);
    }
  }
}
