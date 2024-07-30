using ImportData.IntegrationServicesClient.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImportData.Entities.EDocs
{
  public class DocumentEntity : Entity
  {
    public override IEnumerable<Structures.ExceptionsStruct> SaveToRX(NLog.Logger logger, bool supplementEntity, string ignoreDuplicates)
    {
      var exceptionList = new List<Structures.ExceptionsStruct>();

      if (CheckNeedRequiredDocumentBody(EntityType, out var exceptions))
      {
        if (exceptions.Count > 0)
        {
          exceptionList.AddRange(exceptions);
          return exceptionList;
        }
      }

      exceptionList.AddRange(base.SaveToRX(logger, supplementEntity, ignoreDuplicates));

      if (NamingParameters.ContainsKey(Constants.CellNameFile))
      {
        IEntityBase entity = null;
        var propertiesForCreate = GetPropertiesForSearch(EntityType, exceptionList, logger);
        entity = (IEntityBase)MethodCall(EntityType, "FindEntity", propertiesForCreate, this, true, exceptionList, logger);
        var filePath = NamingParameters[Constants.CellNameFile];
        if (!string.IsNullOrWhiteSpace(filePath) && entity != null)
          exceptionList.AddRange(BusinessLogic.ImportBody((IElectronicDocuments)entity, filePath, logger));
      }

      return exceptionList;
    }

    protected override string GetName()
    {
      var documentKind = ResultValues[Constants.KeyAttributes.DocumentKind];
      var subject = ResultValues[Constants.KeyAttributes.Subject];

      return string.Format("{0} \"{1}\"", documentKind, subject);
    }

    protected override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      ResultValues[Constants.KeyAttributes.Name] = GetName();
      ResultValues[Constants.KeyAttributes.Created] = ResultValues[Constants.KeyAttributes.RegistrationDate];
      ResultValues[Constants.KeyAttributes.RegistrationState] = BusinessLogic.GetRegistrationsState((string)ResultValues[Constants.KeyAttributes.RegistrationState]);
      ResultValues[Constants.KeyAttributes.LifeCycleState] = BusinessLogic.GetPropertyLifeCycleState((string)ResultValues[Constants.KeyAttributes.LifeCycleState]);

      return false;
    }
  }
}