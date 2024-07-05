using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Word;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Xml.Linq;

namespace ImportData.IntegrationServicesClient.Models
{
  [EntityName("Приказ")]
  public class IOrders : IInternalDocumentBases
  {
    new public static IOrders CreateEntity(Dictionary<string, string> propertiesForSearch, Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var subject = propertiesForSearch["Subject"];
      var documentKindName = propertiesForSearch["DocumentKind"];
      var businessUnitName = propertiesForSearch["BusinessUnit"];
      var departmentName = propertiesForSearch["Department"];
      var preparedByName = propertiesForSearch["PreparedBy"];
      var name = $"{documentKindName} \"{subject}\"";
      var registrationNumber = propertiesForSearch["RegistrationNumber"];
      var documentKind = BusinessLogic.GetEntityWithFilter<IDocumentKinds>(x => x.Name == documentKindName, exceptionList, logger);
      var businessUnit = BusinessLogic.GetEntityWithFilter<IBusinessUnits>(x => x.Name == businessUnitName, exceptionList, logger);
      var department = BusinessLogic.GetEntityWithFilter<IDepartments>(x => x.Name == departmentName, exceptionList, logger);
      var preparedBy = BusinessLogic.GetEntityWithFilter<IEmployees>(x => x.Name == preparedByName, exceptionList, logger);
      if (GetDate(propertiesForSearch["RegistrationDate"], out var registrationDate)
        && documentKind != null
        && businessUnit != null
        && department != null
        && preparedBy != null
        )
      {
        return BusinessLogic.CreateEntity<IOrders>(new IOrders()
        {
          Name = name,
          Subject = subject,
          DocumentKind = documentKind,
          BusinessUnit = businessUnit,
          Department = department,
          RegistrationDate = registrationDate,
          RegistrationNumber = registrationNumber,
          Created = registrationDate
        }, exceptionList, logger);
      }
      return null;
    }
    new public static IEntity FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var subject = propertiesForSearch[Constants.KeyAttributes.Subject];
      var documentKindName = propertiesForSearch[Constants.KeyAttributes.DocumentKind];
      var registrationNumber = propertiesForSearch[Constants.KeyAttributes.RegistrationNumber];
      var department = propertiesForSearch[Constants.KeyAttributes.Department];
      if (GetDate(propertiesForSearch[Constants.KeyAttributes.RegistrationDate], out var registrationDate))
      {
        var name = $"{documentKindName} №{registrationNumber} от {registrationDate.ToString("dd.MM.yyyy")} \"{subject}\"";
        return BusinessLogic.GetEntityWithFilter<IOrders>(x => x.Name == name, exceptionList, logger);
      }
      return null;
    }
    new public static string GetName(Entity entity)
    {
      var subject = entity.ResultValues[Constants.KeyAttributes.Subject];
      var documentKind = entity.ResultValues[Constants.KeyAttributes.DocumentKind];
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
        BusinessLogic.CreateEntity((IOrders)entity, exceptionList, logger);
      else
        BusinessLogic.UpdateEntity((IOrders)entity, exceptionList, logger);
    }
  }
}
