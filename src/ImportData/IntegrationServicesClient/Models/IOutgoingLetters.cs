using System;
using System.Collections.Generic;

using NLog;
using Simple.OData.Client;

namespace ImportData.IntegrationServicesClient.Models
{
  [EntityName("Исходящее письмо")]
  public class IOutgoingLetters : IOfficialDocuments
  {
    private DateTimeOffset? registrationDate;
    [PropertyOptions("№", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    new public string RegistrationNumber { get; set; }
    [PropertyOptions("Дата регистрации", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    new public DateTimeOffset? RegistrationDate
    {
      get { return registrationDate; }
      set { registrationDate = value.HasValue ? new DateTimeOffset(value.Value.Date, TimeSpan.Zero) : new DateTimeOffset?(); }
    }

    public bool IsManyAddressees { get; set; }
    public IEmployees Addressee { get; set; }
    public IEmployees Assignee { get; set; }
    public IBusinessUnits BusinessUnit { get; set; }
    [PropertyOptions("Корреспондент", RequiredType.Required, PropertyType.EntityWithCreate, AdditionalCharacters.ForSearch)]
    public ICounterparties Correspondent { get; set; }
    public IEmployees ResponsibleForReturnEmployee { get; set; }
    [PropertyOptions("Способ доставки", RequiredType.NotRequired, PropertyType.Entity, AdditionalCharacters.ForSearch)]
    public IMailDeliveryMethods DeliveryMethod { get; set; }
    public IEnumerable<IOutgoingLetterAddresseess> Addressees { get; set; }

    new public static string GetName(Entity entity)
    {
      var subject = entity.ResultValues[Constants.KeyAttributes.Subject];
      var documentKind = entity.ResultValues[Constants.KeyAttributes.DocumentKind];
      var counterparty = entity.ResultValues[Constants.KeyAttributes.Correspondent];
      var registrationNumber = entity.ResultValues[Constants.KeyAttributes.RegistrationNumber];
      var registrationDate = (DateTimeOffset)entity.ResultValues[Constants.KeyAttributes.RegistrationDate];
      return $"{documentKind} №{registrationNumber} от {registrationDate.ToString("dd.MM.yyyy")} с {counterparty} \"{subject}\"";
    }

    new public static bool FillProperies(Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      entity.ResultValues["Name"] = GetName(entity);
      entity.ResultValues["Created"] = entity.ResultValues["RegistrationDate"];
      entity.ResultValues["RegistrationState"] = BusinessLogic.GetRegistrationsState((string)entity.ResultValues["RegistrationState"]);
      entity.ResultValues["LifeCycleState"] = "Active";
      return false;
    }

    new public static IEntity FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      if (propertiesForSearch.ContainsKey(Constants.KeyAttributes.OutgoingDocumentBase)
        && int.TryParse(propertiesForSearch[Constants.KeyAttributes.OutgoingDocumentBase], out int OutgoingDocumentBaseId))
      {
        return BusinessLogic.GetEntityWithFilter<IOutgoingLetters>(x => x.Id == OutgoingDocumentBaseId, exceptionList, logger);
      }
      var incomingLetters = new IOutgoingLetters();
      var subject = propertiesForSearch[Constants.KeyAttributes.Subject];
      var docRegisterId = propertiesForSearch[Constants.KeyAttributes.DocumentRegister];
      var regNumber = propertiesForSearch[Constants.KeyAttributes.RegistrationNumber];
      if (GetDate(propertiesForSearch[Constants.KeyAttributes.RegistrationDate], out var registrationDate)
        && int.TryParse(docRegisterId, out int documentRegisterId))
      {
        incomingLetters = BusinessLogic.GetEntityWithFilter<IOutgoingLetters>(x => x.RegistrationNumber == regNumber &&
          x.RegistrationDate.Value.ToString("d") == registrationDate.ToString("d") &&
          x.DocumentRegister.Id == documentRegisterId,
        exceptionList, logger, true);
      }
      if (incomingLetters != null)
        return BusinessLogic.GetEntityWithFilter<IOutgoingLetters>(x => x.Id == incomingLetters.Id, exceptionList, logger);
      return null;
    }
    new public static void CreateOrUpdate(IEntity entity, bool isNewEntity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      if (isNewEntity)
      {
        entity = BusinessLogic.CreateEntity((IOutgoingLetters)entity, exceptionList, logger);
        ((IOutgoingLetters)entity)?.UpdateLifeCycleState("Active");
      }
      else
        BusinessLogic.UpdateEntity((IOutgoingLetters)entity, exceptionList, logger);
    }

    public void CreateAddressee(IOutgoingLetterAddresseess addressee, Logger logger)
    {
      try
      {
        if (!IsManyAddressees)
          IsManyAddressees = true;

        var result = Client.Instance().For<IOutgoingLetters>()
         .Key(this)
         .NavigateTo(nameof(Addressees))
         .Set(new IOutgoingLetterAddresseess()
         {
           Addressee = addressee.Addressee,
           DeliveryMethod = addressee.DeliveryMethod,
           Correspondent = addressee.Correspondent,
           OutgoingDocumentBase = this,
         })
         .InsertEntryAsync().Result;
      }
      catch (Exception ex)
      {
        logger.Error(ex);
        throw;
      }
    }
  }
}
