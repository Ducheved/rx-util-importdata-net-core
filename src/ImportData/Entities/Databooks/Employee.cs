using System;
using System.Collections.Generic;
using ImportData.IntegrationServicesClient.Models;
using NLog;

namespace ImportData
{
  class Employee : Entity
  {
    public override int PropertiesCount { get { return 20; } }
    protected override Type EntityType { get { return typeof(IEmployees); } }

    public override string GetName()
    {
      var person = (IPersons)ResultValues["Person"];
      return person.Name;
    }

    public override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      ResultValues["Name"] = GetName();
      ResultValues["Status"] = "Active";
      return false;
    }
  }
}
