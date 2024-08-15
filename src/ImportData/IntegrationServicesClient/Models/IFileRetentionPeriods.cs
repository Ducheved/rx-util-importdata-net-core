using DocumentFormat.OpenXml.Office2013.Word;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImportData.IntegrationServicesClient.Models
{
  [EntityName("Срок хранения дела")]
  public class IFileRetentionPeriods : IEntity
  {
    [PropertyOptions("Наименование срока хранения", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    new public string Name { get; set; }

    [PropertyOptions("Срок хранения", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    public int? RetentionPeriod { get; set; }

    public string Note { get; set; }
    public string Status { get; set; }

    new public static IEntity CreateEntity(Dictionary<string, string> propertiesForSearch, Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch.ContainsKey(Constants.KeyAttributes.CustomFieldName) ?
        propertiesForSearch[Constants.KeyAttributes.CustomFieldName] : propertiesForSearch[Constants.KeyAttributes.Name];
      var period = int.Parse(propertiesForSearch[Constants.KeyAttributes.RetentionPeriod]);

      return BusinessLogic.CreateEntity(new IFileRetentionPeriods()
      {
        Name = name,
        RetentionPeriod = period,
        Status = Constants.AttributeValue[Constants.KeyAttributes.Status]
      }, exceptionList, logger);
    }

    new public static IEntity FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch.ContainsKey(Constants.KeyAttributes.CustomFieldName) ?
        propertiesForSearch[Constants.KeyAttributes.CustomFieldName] : propertiesForSearch[Constants.KeyAttributes.Name];
      var period = int.Parse(propertiesForSearch[Constants.KeyAttributes.RetentionPeriod]);

      return BusinessLogic.GetEntityWithFilter<IFileRetentionPeriods>(x => x.Name == name &&
        x.RetentionPeriod == period, exceptionList, logger);
    }

    new public static void CreateOrUpdate(IEntity entity, bool isNewEntity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      ((IFileRetentionPeriods)entity).Status = Constants.AttributeValue[Constants.KeyAttributes.Status];
      if (isNewEntity)
        BusinessLogic.CreateEntity((IFileRetentionPeriods)entity, exceptionList, logger);
      else
        BusinessLogic.UpdateEntity((IFileRetentionPeriods)entity, exceptionList, logger);
    }
  }
}
