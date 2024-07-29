using System;
using System.Collections.Generic;
using NLog;
using ImportData.IntegrationServicesClient.Models;

namespace ImportData
{
  class OutgoingLetterAddressees : Entity
  {
    public override int PropertiesCount { get { return 4; } }
    protected override Type EntityType { get { return typeof(IOutgoingLetterAddresseess); } }

    public override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      ResultValues["IsManyAddressees"] = true;
      return false;
    }
  }
}
