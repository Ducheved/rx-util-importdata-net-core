using System;
using System.Collections.Generic;
using System.Text;

namespace ImportData.Entities.EDocs
{
  public class DocumentEntity : Entity
  {
    public override IEnumerable<Structures.ExceptionsStruct> SaveToRX(NLog.Logger logger, bool supplementEntity, string ignoreDuplicates)
    {
      var exceptionList = base.SaveToRX(logger, supplementEntity, ignoreDuplicates);
      // TODO: специфицичное создание документа - тело, регистрация и тд...

      return exceptionList;
    }

    public override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      ResultValues[Constants.KeyAttributes.Name] = GetName();
      ResultValues["Created"] = ResultValues[Constants.KeyAttributes.RegistrationDate];
      ResultValues["RegistrationState"] = BusinessLogic.GetRegistrationsState((string)ResultValues["RegistrationState"]);
      ResultValues["LifeCycleState"] = BusinessLogic.GetPropertyLifeCycleState((string)ResultValues["LifeCycleState"]); // где-то было Active
      return false;
    }
  }
}
