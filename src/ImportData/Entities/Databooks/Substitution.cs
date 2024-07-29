using System;
using System.Collections.Generic;
using NLog;
using ImportData.IntegrationServicesClient.Models;

namespace ImportData.Entities.Databooks
{
  public class Substitution : Entity
  {
    public override int PropertiesCount { get { return 4; } }
    protected override Type EntityType { get { return typeof(ISubstitutions); } }

    public override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      ResultValues["Status"] = "Active";

      if ((DateTimeOffset)ResultValues["StartDate"] == DateTimeOffset.MinValue)
        ResultValues["StartDate"] = null;

      if ((DateTimeOffset)ResultValues["EndDate"] == DateTimeOffset.MinValue)
        ResultValues["EndDate"] = null;

      return false;
    }
  }
}
