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
    [PropertyOptions("Эл.почта", RequiredType.Required, PropertyType.Simple)]
    public string Email { get; set; }
    public bool? NeedNotifyExpiredAssignments { get; set; }
    public bool? NeedNotifyNewAssignments { get; set; }
    public bool? NeedNotifyAssignmentsSummary { get; set; }
    [PropertyOptions("Табельный номер", RequiredType.NotRequired, PropertyType.Simple)]
    public string PersonnelNumber { get; set; }

    new public static IEntity FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch[Constants.KeyAttributes.Name];
      return BusinessLogic.GetEntityWithFilter<IJobTitles>(x => x.Name == name, exceptionList, logger);
    }
    new public static IEmployees CreateEntity(Dictionary<string, string> propertiesForSearch, Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch["Name"];
      var person = (IPersons)entity.ResultValues["Person"];
      var department = (IDepartments)entity.ResultValues["Department"];
      return BusinessLogic.CreateEntity<IEmployees>(
        new IEmployees()
        {
          Name = name,
          Person = person,
          Department = department,
          Status = "Active"
        },
        exceptionList,
        logger
        );
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
