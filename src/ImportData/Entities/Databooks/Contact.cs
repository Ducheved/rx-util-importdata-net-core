using System;
using System.Collections.Generic;
using NLog;
using ImportData.IntegrationServicesClient.Models;

namespace ImportData.Entities.Databooks
{
  public class Contact : Entity
  {
    public override int PropertiesCount { get { return 10; } }
    protected override Type EntityType { get { return typeof(IContacts); } }

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
