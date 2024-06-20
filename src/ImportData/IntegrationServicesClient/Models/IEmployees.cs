using DocumentFormat.OpenXml.Bibliography;
using System.Collections.Generic;

namespace ImportData.IntegrationServicesClient.Models
{
  [EntityName("Сотрудник")]
  public class IEmployees : IUsers
  {
    [PropertyOptions("Телефон", RequiredType.NotRequired, PropertyType.Simple)]
    public string Phone { get; set; }
    [PropertyOptions("Примечание", RequiredType.NotRequired, PropertyType.Simple)]
    public string Note { get; set; }
    [PropertyOptions("Эл.почта", RequiredType.NotRequired, PropertyType.Simple)]
    public string Email { get; set; }
    public bool? NeedNotifyExpiredAssignments { get; set; }
    public bool? NeedNotifyNewAssignments { get; set; }
    public bool? NeedNotifyAssignmentsSummary { get; set; }
    [PropertyOptions("Табельный номер", RequiredType.NotRequired, PropertyType.Simple)]
    public string PersonnelNumber { get; set; }

    new public static IEntity FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch[Constants.KeyAttributes.Name];
      return BusinessLogic.GetEntityWithFilter<IEmployees>(x => x.Name == name, exceptionList, logger);
    }
    new public static IEmployees CreateEntity(Dictionary<string, string> propertiesForSearch, Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch.ContainsKey(Constants.KeyAttributes.Manager) ?
                                    propertiesForSearch[Constants.KeyAttributes.Manager] :
                                    propertiesForSearch.ContainsKey(Constants.KeyAttributes.CEO) ?
                                    propertiesForSearch[Constants.KeyAttributes.CEO] :
                                    propertiesForSearch[Constants.KeyAttributes.Name];
      if (entity.ResultValues.TryGetValue("Person", out var person)
        && entity.ResultValues.TryGetValue("Department", out var department))
      {
        return BusinessLogic.CreateEntity<IEmployees>(
          new IEmployees()
          {
            Name = name,
            Person = (IPersons)person,
            Department = (IDepartments)department,
            Status = "Active"
          },
          exceptionList,
          logger
          );
      }
      else
      {
        return BusinessLogic.GetEntityWithFilter<IEmployees>(x => x.Name == name, exceptionList, logger);
      }
    }

    new public static string GetName(Entity entity)
    {
      var person = (IPersons)entity.ResultValues["Person"];
      return person.Name;
    }

    new public static bool FillProperies(Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      entity.ResultValues["Name"] = GetName(entity);
      entity.ResultValues["Status"] = "Active";
      return false;
    }

    new public static void CreateOrUpdate(IEntity entity, bool isNewEntity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      if (isNewEntity)
        BusinessLogic.CreateEntity((IEmployees)entity, exceptionList, logger);
      else
        BusinessLogic.UpdateEntity((IEmployees)entity, exceptionList, logger);
    }
  }
}
