using ImportData.IntegrationServicesClient.Models;
using NLog;
using System;
using System.Collections.Generic;

namespace ImportData.Entities.Databooks
{
  public class Login : Entity
  {
    public override int PropertiesCount { get { return 4; } }
    protected override Type EntityType { get { return typeof(ILogins); } }

    protected override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      ResultValues[Constants.KeyAttributes.NeedChangePassword] = false;
      ResultValues[Constants.KeyAttributes.TypeAuthentication] = Constants.AttributeValue[Constants.KeyAttributes.TypeAuthentication];
      ResultValues[Constants.KeyAttributes.Status] = Constants.AttributeValue[Constants.KeyAttributes.Status];
      
      return false;
    }
  }
}
