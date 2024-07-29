using System;
using System.Collections.Generic;
using NLog;
using ImportData.IntegrationServicesClient.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ImportData.Entities.Databooks
{
  internal class CaseFile : Entity
  {
    public override int PropertiesCount { get { return 10; } } 
    protected override Type EntityType { get { return typeof(ICaseFiles); } }

    public override string GetName()
    {
      var title = ResultValues["Title"];
      var index = ResultValues["Index"];
      return string.Format("{0}. {1}", index, title);
    }
    public DateTimeOffset? GetDateTime(string name)
    {
      var date = (DateTimeOffset)ResultValues[name];
      if (date == DateTimeOffset.MinValue)
        return null;

      return date;
    }

    public override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      ResultValues["Name"] = GetName();
      ResultValues["StartDate"] = GetDateTime("StartDate");
      ResultValues["EndDate"] = GetDateTime("EndDate");
      ResultValues["LongTerm"] = false;
      ResultValues["Status"] = "Active";
      return false;
    }
  }
}
