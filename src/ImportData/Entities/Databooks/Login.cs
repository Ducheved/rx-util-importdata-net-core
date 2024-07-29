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

    public override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      ResultValues["NeedChangePassword"] = false;
      ResultValues["TypeAuthentication"] = "Windows";
      ResultValues["Status"] = "Active";
      return false;
    }
  }
}
