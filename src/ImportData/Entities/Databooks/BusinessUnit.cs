using System;
using System.Collections.Generic;
using NLog;
using ImportData.IntegrationServicesClient.Models;

namespace ImportData
{
  class BusinessUnit : Entity
  {
    protected override Type EntityType { get { return typeof(IBusinessUnits); } }
    public override int PropertiesCount { get { return 20; } }

    public override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      if (ResultValues[Constants.KeyAttributes.HeadCompany] != null && 
         ((IBusinessUnits)ResultValues[Constants.KeyAttributes.HeadCompany]).Name == (string)ResultValues[Constants.KeyAttributes.Name])
           ResultValues[Constants.KeyAttributes.HeadCompany] = null;

      ResultValues["Status"] = "Active";

      return false;
    }
  }
}
