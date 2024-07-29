using System;
using System.Collections.Generic;
using System.Globalization;
using NLog;
using ImportData.IntegrationServicesClient.Models;
using System.IO;
using ImportData.Entities.EDocs;

namespace ImportData
{
  class Order : DocumentEntity
  {
    public override int PropertiesCount { get { return 14; } }
    protected override Type EntityType { get { return typeof(IOrders); } }

    public override string GetName()
    {
      var subject = ResultValues[Constants.KeyAttributes.Subject];
      var documentKind = ResultValues[Constants.KeyAttributes.DocumentKind];
      return string.Format("{0} \"{1}\"", documentKind, subject);
    }
  }
}
