using System;
using System.Collections.Generic;

namespace ImportData.IntegrationServicesClient.Models
{
  [EntityName("Дополнительные соглашения")]
  public class ISupAgreements : IContractualDocuments
  {
    private DateTimeOffset? registrationDate;
    private DateTimeOffset? documentDate;
    private DateTimeOffset? validFrom;
    private DateTimeOffset? validTill;
    [PropertyOptions("№ доп. соглашения", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    new public string RegistrationNumber { get; set; }
    [PropertyOptions("Дата доп. соглашения", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    new public DateTimeOffset? RegistrationDate
    {
      get { return registrationDate; }
      set { registrationDate = value.HasValue ? new DateTimeOffset(value.Value.Date, TimeSpan.Zero) : new DateTimeOffset?(); }
    }
    [PropertyOptions("№ договора", RequiredType.Required, PropertyType.Entity, AdditionalCharacters.ForSearch)]
    new public IOfficialDocuments LeadingDocument { get; set; }
    [PropertyOptions("Дата договора", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    new public DateTimeOffset? DocumentDate
    {
      get { return documentDate; }
      set { documentDate = value.HasValue ? new DateTimeOffset(value.Value.Date, TimeSpan.Zero) : new DateTimeOffset?(); }
    }
    [PropertyOptions("Действует с", RequiredType.Required, PropertyType.Simple)]
    new public DateTimeOffset? ValidFrom
    {
      get { return validFrom; }
      set { validFrom = value.HasValue ? new DateTimeOffset(value.Value.Date, TimeSpan.Zero) : new DateTimeOffset?(); }
    }
    [PropertyOptions("Действует по", RequiredType.Required, PropertyType.Simple)]
    new public DateTimeOffset? ValidTill
    {
      get { return validTill; }
      set { validTill = value.HasValue ? new DateTimeOffset(value.Value.Date, TimeSpan.Zero) : new DateTimeOffset?(); }
    }
    [PropertyOptions("Сумма", RequiredType.Required, PropertyType.Simple)]
    new public double TotalAmount { get; set; }
    [PropertyOptions("Состояние", RequiredType.NotRequired, PropertyType.Simple)]
    new public string LifeCycleState { get; set; }
    [PropertyOptions("Валюта", RequiredType.Required, PropertyType.Entity, AdditionalCharacters.ForSearch)]
    new public ICurrencies Currency { get; set; }
    [PropertyOptions("Журнал регистрации", RequiredType.NotRequired, PropertyType.Entity, AdditionalCharacters.ForSearch)]
    new public IDocumentRegisters DocumentRegister { get; set; }

    new public static IEntity FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var supAgreement = new ISupAgreements();
      var subject = propertiesForSearch[Constants.KeyAttributes.Subject];
      var regNumber = propertiesForSearch[Constants.KeyAttributes.RegistrationNumber];
      var counterpartyName = propertiesForSearch[Constants.KeyAttributes.Counterparty];
      var docRegisterId = propertiesForSearch[Constants.KeyAttributes.DocumentRegister];
      var counterparty = BusinessLogic.GetEntityWithFilter<ICounterparties>(x => x.Name == counterpartyName, exceptionList, logger);
      if (GetDate(propertiesForSearch[Constants.KeyAttributes.RegistrationDate], out var registrationDate) &&
        int.TryParse(docRegisterId, out int documentRegisterId))
      {
        var documentRegister = BusinessLogic.GetEntityWithFilter<IDocumentRegisters>(x => x.Id == documentRegisterId, exceptionList, logger);
        supAgreement = BusinessLogic.GetEntityWithFilter<ISupAgreements>(x => x.RegistrationNumber != null &&
          x.RegistrationNumber == regNumber &&
          x.RegistrationDate.Value.ToString("d") == registrationDate.ToString("d") &&
          x.Counterparty.Id == counterparty.Id &&
          x.DocumentRegister.Id == documentRegister.Id, exceptionList, logger, true);
      }
      if (supAgreement != null)
        return BusinessLogic.GetEntityWithFilter<ISupAgreements>(x => x.Id == supAgreement.Id, exceptionList, logger);
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
        var lifeCycleState = ((ISupAgreements)entity).LifeCycleState;
        entity = BusinessLogic.CreateEntity((ISupAgreements)entity, exceptionList, logger);
        ((ISupAgreements)entity)?.UpdateLifeCycleState(lifeCycleState);
      }
      else
        BusinessLogic.UpdateEntity((ISupAgreements)entity, exceptionList, logger);
    }
  }
}
