using System.Collections.Generic;

namespace ImportData.IntegrationServicesClient.Models
{
  [EntityName("Подразделение")]
  public class IDepartments : IRecipients
  {
    [PropertyOptions("Телефон", RequiredType.NotRequired, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    public string Phone { get; set; }
    [PropertyOptions("Краткое наименование", RequiredType.NotRequired, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    public string ShortName { get; set; }
    [PropertyOptions("Примечание", RequiredType.NotRequired, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    public string Note { get; set; }
    [PropertyOptions("Код", RequiredType.NotRequired, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    public string Code { get; set; }
    [PropertyOptions("Наша организация", RequiredType.NotRequired, PropertyType.EntityWithCreate)]
    public IBusinessUnits BusinessUnit { get; set; }
    [PropertyOptions("Головное подразделение", RequiredType.NotRequired, PropertyType.EntityWithCreate)]
    public IDepartments HeadOffice { get; set; }
    [PropertyOptions("Руководитель", RequiredType.NotRequired, PropertyType.EntityWithCreate, AdditionalCharacters.CreateFromOtherProperties)]
    public IEmployees Manager { get; set; }
    new public static IDepartments CreateEntity(Dictionary<string, string> propertiesForSearch, Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch["Name"];
      return BusinessLogic.CreateEntity<IDepartments>(new IDepartments() { Name = name, Status = "Active" }, exceptionList, logger);
    }
    new public static IEntity FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch[Constants.KeyAttributes.Name];
      return BusinessLogic.GetEntityWithFilter<IDepartments>(x => x.Name == name, exceptionList, logger);
    }
    new public static string GetName(Entity entity)
    {
      var manager = (IEmployees)entity.ResultValues["Manager"];
      return manager.Name;
    }
    new public static bool FillProperies(Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      //entity.ResultValues["Name"] = GetName(entity);
      entity.ResultValues["Status"] = "Active";
      return false;
    }
    new public static void CreateOrUpdate(IEntity entity, bool isNewEntity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      if (isNewEntity)
        BusinessLogic.CreateEntity((IDepartments)entity, exceptionList, logger);
      else
        BusinessLogic.UpdateEntity((IDepartments)entity, exceptionList, logger);
    }
  }
}
