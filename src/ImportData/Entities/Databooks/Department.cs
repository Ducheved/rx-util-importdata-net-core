using System;
using System.Collections.Generic;
using ImportData.IntegrationServicesClient.Models;
using NLog;

namespace ImportData
{
  class Department : Entity
  {
    public override int PropertiesCount { get { return 8; } }
    protected override Type EntityType { get { return typeof(IDepartments); } }

    public override string GetName()
    {
      var manager = (IEmployees)ResultValues["Manager"];
      if (manager == null)
        return string.Empty;
      return manager.Name;
    }
    public override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var managerName = GetName();
      ResultValues["Manager"] = BusinessLogic.GetEntityWithFilter<IEmployees>(x => x.Name == managerName, exceptionList, logger);

      if (ResultValues[Constants.KeyAttributes.HeadOffice] != null && 
         ((IDepartments)ResultValues[Constants.KeyAttributes.HeadOffice]).Name == (string)ResultValues[Constants.KeyAttributes.Name])
            ResultValues[Constants.KeyAttributes.HeadOffice] = null;

      ResultValues["Status"] = "Active";
      return false;
    }
  }
}
