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
      var name = propertiesForSearch.ContainsKey(Constants.KeyAttributes.Responsible) ?
       propertiesForSearch[Constants.KeyAttributes.Responsible] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.Manager) ?
       propertiesForSearch[Constants.KeyAttributes.Manager] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.CEO) ?
       propertiesForSearch[Constants.KeyAttributes.CEO] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.ResponsibleEmployee) ?
       propertiesForSearch[Constants.KeyAttributes.ResponsibleEmployee] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.OurSignatory) ?
       propertiesForSearch[Constants.KeyAttributes.OurSignatory] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.Addressee) ?
       propertiesForSearch[Constants.KeyAttributes.Addressee] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.PreparedBy) ?
       propertiesForSearch[Constants.KeyAttributes.PreparedBy] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.Assignee) ?
       propertiesForSearch[Constants.KeyAttributes.Assignee] : propertiesForSearch[Constants.KeyAttributes.Name];

      return BusinessLogic.GetEntityWithFilter<IEmployees>(x => x.Name == name, exceptionList, logger);
    }

    new public static IEntity CreateEntity(Dictionary<string, string> propertiesForSearch, Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch.ContainsKey(Constants.KeyAttributes.Responsible) ?
       propertiesForSearch[Constants.KeyAttributes.Responsible] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.Manager) ?
       propertiesForSearch[Constants.KeyAttributes.Manager] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.CEO) ?
       propertiesForSearch[Constants.KeyAttributes.CEO] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.ResponsibleEmployee) ?
       propertiesForSearch[Constants.KeyAttributes.ResponsibleEmployee] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.OurSignatory) ?
       propertiesForSearch[Constants.KeyAttributes.OurSignatory] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.Addressee) ?
       propertiesForSearch[Constants.KeyAttributes.Addressee] : propertiesForSearch.ContainsKey(Constants.KeyAttributes.PreparedBy) ?
       propertiesForSearch[Constants.KeyAttributes.PreparedBy] : propertiesForSearch[Constants.KeyAttributes.Name];

      if (entity.ResultValues.TryGetValue(Constants.KeyAttributes.Person, out var person) &&
        entity.ResultValues.TryGetValue(Constants.KeyAttributes.Department, out var department))
      {
        return BusinessLogic.CreateEntity<IEmployees>(
          new IEmployees()
          {
            Name = name,
            Person = (IPersons)person,
            Department = (IDepartments)department,
            Status = Constants.AttributeValue[Constants.KeyAttributes.Status]
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

    new public static void CreateOrUpdate(IEntity entity, bool isNewEntity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      if (isNewEntity)
        BusinessLogic.CreateEntity((IEmployees)entity, exceptionList, logger);
      else
        BusinessLogic.UpdateEntity((IEmployees)entity, exceptionList, logger);
    }
  }
}
