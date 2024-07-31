﻿using System;
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

    new public static IEntity FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      if (propertiesForSearch.ContainsKey(Constants.KeyAttributes.OutgoingDocumentBase) &&
        int.TryParse(propertiesForSearch[Constants.KeyAttributes.OutgoingDocumentBase], out int OutgoingDocumentBaseId))
      {
        return BusinessLogic.GetEntityWithFilter<IOutgoingLetters>(x => x.Id == OutgoingDocumentBaseId, exceptionList, logger);
      }

      IOutgoingLetters incomingLetters = null;
      var docRegisterId = propertiesForSearch[Constants.KeyAttributes.DocumentRegister];
      var regNumber = propertiesForSearch[Constants.KeyAttributes.RegistrationNumber];

      if (GetDate(propertiesForSearch[Constants.KeyAttributes.RegistrationDate], out var registrationDate) &&
        int.TryParse(docRegisterId, out int documentRegisterId))
      {
        //HACK: если искать без расширенных свойств, то сущность можеть быть не найдена.
        incomingLetters = BusinessLogic.GetEntityWithFilter<IOutgoingLetters>(x => x.RegistrationNumber == regNumber &&
          x.RegistrationDate.Value.ToString("d") == registrationDate.ToString("d") &&
          x.DocumentRegister.Id == documentRegisterId,
        exceptionList, logger, true);
      }

      //HACK: Сервис интеграции при расширенном размере свойств сущности может свалиться ошибку.
      if (incomingLetters != null)
        return BusinessLogic.GetEntityWithFilter<IOutgoingLetters>(x => x.Id == incomingLetters.Id, exceptionList, logger);

      return incomingLetters;
    }
    new public static IEntityBase CreateOrUpdate(IEntity entity, bool isNewEntity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      if (isNewEntity)
      {
        entity = BusinessLogic.CreateEntity((IOutgoingLetters)entity, exceptionList, logger);
        return((IOutgoingLetters)entity)?.UpdateLifeCycleState(Constants.AttributeValue[Constants.KeyAttributes.Status]);
      }
      else
        return BusinessLogic.UpdateEntity((IOutgoingLetters)entity, exceptionList, logger);
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
