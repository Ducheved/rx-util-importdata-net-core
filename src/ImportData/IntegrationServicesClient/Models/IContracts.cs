using System;
using System.Collections.Generic;

namespace ImportData.IntegrationServicesClient.Models
{
  [EntityName("Договор")]
  public class IContracts : IContractualDocuments
  {

    new public static IContracts FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch["Name"];
      return BusinessLogic.GetEntityWithFilter<IContracts>(x => x.Name == name, exceptionList, logger);
    }

    new public static IContracts CreateEntity(Dictionary<string, string> propertiesForSearch, Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch["Name"];
      return BusinessLogic.CreateEntity<IContracts>(new IContracts() { Name = name}, exceptionList, logger);
    }
    new public static bool FillProperies(Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      return false;
    }
    new public static void CreateOrUpdate(IEntity entity, bool isNewEntity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      if (isNewEntity)
        BusinessLogic.CreateEntity((ICompanies)entity, exceptionList, logger);
      else
        BusinessLogic.UpdateEntity((ICompanies)entity, exceptionList, logger);
    }
  }

}
