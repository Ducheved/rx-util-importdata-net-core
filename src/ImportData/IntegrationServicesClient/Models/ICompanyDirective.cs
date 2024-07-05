using System;
using System.Collections.Generic;
using System.Text;

namespace ImportData.IntegrationServicesClient.Models
{
  public class ICompanyDirective : IInternalDocumentBases
  {
    private DateTimeOffset? registrationDate;
    [PropertyOptions("Дата", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    new public DateTimeOffset? RegistrationDate
    {
      get { return registrationDate; }
      set { registrationDate = value.HasValue ? new DateTimeOffset(value.Value.Date, TimeSpan.Zero) : new DateTimeOffset?(); }
    }
    [PropertyOptions("Подробности", RequiredType.Required, PropertyType.Entity, AdditionalCharacters.ForSearch)]
    new public IDocumentKinds DocumentKind { get; set; }
    new public static IEntity FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var subject = propertiesForSearch["Subject"];
      var documentKindName = propertiesForSearch["DocumentKind"];
      var registrationNumber = propertiesForSearch[Constants.KeyAttributes.RegistrationNumber];
      var department = propertiesForSearch[Constants.KeyAttributes.Department];
      if (GetDate(propertiesForSearch[Constants.KeyAttributes.RegistrationDate], out var registrationDate))
      {
        var name = $"{documentKindName} №{registrationNumber} от {registrationDate.ToString("dd.MM.yyyy")} \"{subject}\"";
        return BusinessLogic.GetEntityWithFilter<ICompanyDirective>(x => x.Name == name, exceptionList, logger);
      }
      return null;
    }
    new public static string GetName(Entity entity)
    {
      var documentKind = entity.ResultValues["DocumentKind"];
      var subject = entity.ResultValues["Subject"];
      return string.Format("{0} \"{1}\"", documentKind, subject);
    }

    new public static bool FillProperies(Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      entity.ResultValues["Name"] = GetName(entity);
      entity.ResultValues["Created"] = entity.ResultValues["RegistrationDate"];
      entity.ResultValues["RegistrationState"] = BusinessLogic.GetRegistrationsState((string)entity.ResultValues["RegistrationState"]);
      entity.ResultValues["LifeCycleState"] = BusinessLogic.GetPropertyLifeCycleState((string)entity.ResultValues["LifeCycleState"]);
      return false;
    }
    new public static void CreateOrUpdate(IEntity entity, bool isNewEntity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      if (isNewEntity)
      {
        var lifeCycleState = ((ICompanyDirective)entity).LifeCycleState;
        entity = BusinessLogic.CreateEntity((ICompanyDirective)entity, exceptionList, logger);
        ((ICompanyDirective)entity)?.UpdateLifeCycleState(lifeCycleState);
      }
      else
        BusinessLogic.UpdateEntity((ICompanyDirective)entity, exceptionList, logger);
    }
  }
}
