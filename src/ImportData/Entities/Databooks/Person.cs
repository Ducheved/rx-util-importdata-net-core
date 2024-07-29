using System;
using System.Collections.Generic;
using System.Globalization;
using NLog;
using ImportData.IntegrationServicesClient.Models;
using System.Linq.Expressions;

namespace ImportData
{
  class Person : Entity
  {
    public override int PropertiesCount { get { return 17; } }
    protected override Type EntityType { get { return typeof(IPersons); } }

    public override string GetName()
    {
      var firstName = ResultValues["FirstName"];
      var middleName = ResultValues["MiddleName"];
      var lastName = ResultValues["LastName"];

      return string.Format("{0} {1} {2}", lastName, firstName, middleName);
    }

    public override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      ResultValues["Name"] = GetName();
      ResultValues["Sex"] = BusinessLogic.GetPropertySex((string)ResultValues["Sex"]);
      ResultValues["Status"] = "Active";

      return false;
    }
  }
}
