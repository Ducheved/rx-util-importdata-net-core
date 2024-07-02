using System;
using System.Collections.Generic;

namespace ImportData.IntegrationServicesClient.Models
{
  [EntityName("Список рассылки исходящего письма")]
  public class IOutgoingLetterAddresseess : IEntityBase
  {
    [PropertyOptions("Id исходящего письма в DirectumRX", RequiredType.Required, PropertyType.Entity, AdditionalCharacters.ForSearch)]
    public IOutgoingLetters OutgoingDocumentBase { get; set; }
    [PropertyOptions("Корреспондент", RequiredType.Required, PropertyType.Entity, AdditionalCharacters.ForSearch)]
    public ICounterparties Correspondent { get; set; }
    [PropertyOptions("Адресат", RequiredType.NotRequired, PropertyType.Entity)]
    public IContacts Addressee { get; set; }
    [PropertyOptions("Способ доставки", RequiredType.NotRequired, PropertyType.Entity)]
    public IMailDeliveryMethods DeliveryMethod { get; set; }
    new public static IOutgoingLetterAddresseess CreateEntity(Dictionary<string, string> propertiesForSearch, Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      return null;
    }
    new public static IEntityBase FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      if (int.TryParse(propertiesForSearch["name"], out int outgoingDocumentId))
        return BusinessLogic.GetEntityWithFilter<IOutgoingLetterAddresseess>(x => x.OutgoingDocumentBase.Id == outgoingDocumentId, exceptionList, logger);
      return null;
    }
  }
}
