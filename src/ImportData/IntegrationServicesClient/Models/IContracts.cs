using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace ImportData.IntegrationServicesClient.Models
{
  [EntityName("Договор")]
  public class IContracts : IContractualDocuments
  {
    private DateTimeOffset? registrationDate;
    [PropertyOptions("№ договора", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    new public string RegistrationNumber { get; set; }
    [PropertyOptions("Дата договора", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    new public DateTimeOffset? RegistrationDate
    {
      get { return registrationDate; }
      set { registrationDate = value.HasValue ? new DateTimeOffset(value.Value.Date, TimeSpan.Zero) : new DateTimeOffset?(); }
    }
    [PropertyOptions("ИД журнала регистрации", RequiredType.Required, PropertyType.Entity, AdditionalCharacters.ForSearch)]
    new public IDocumentRegisters DocumentRegister { get; set; }
    new public static IContracts FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var subject = propertiesForSearch[Constants.KeyAttributes.Subject];
      var documentKind = propertiesForSearch[Constants.KeyAttributes.DocumentKind];
      var counterparty = propertiesForSearch[Constants.KeyAttributes.Counterparty];
      var registrationNumber = propertiesForSearch[Constants.KeyAttributes.RegistrationNumber];
      if (GetDate(propertiesForSearch[Constants.KeyAttributes.RegistrationDate], out var registrationDate))
      {
        var name = $"{documentKind} №{registrationNumber} от {registrationDate.ToString("dd.MM.yyyy")} с {counterparty} \"{subject}\"";
        return BusinessLogic.GetEntityWithFilter<IContracts>(x => x.Name == name, exceptionList, logger);
      }
      return null;
    }
    new public static string GetName(Entity entity)
    {
      var subject = entity.ResultValues[Constants.KeyAttributes.Subject];
      var documentKind = entity.ResultValues[Constants.KeyAttributes.DocumentKind];
      var counterparty = entity.ResultValues[Constants.KeyAttributes.Counterparty];
      var registrationNumber = entity.ResultValues[Constants.KeyAttributes.RegistrationNumber];
      var registrationDate = (DateTimeOffset)entity.ResultValues[Constants.KeyAttributes.RegistrationDate];
      return $"{documentKind} №{registrationNumber} от {registrationDate.ToString("dd.MM.yyyy")} с {counterparty} \"{subject}\"";
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
        var lifeCycleState = ((IContracts)entity).LifeCycleState;
        entity = BusinessLogic.CreateEntity((IContracts)entity, exceptionList, logger);
        ((IContracts)entity)?.UpdateLifeCycleState(lifeCycleState);
      }
      else
        BusinessLogic.UpdateEntity((IContracts)entity, exceptionList, logger);
    }
  }

}
