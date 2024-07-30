using System;
using System.Collections.Generic;
using System.Globalization;
using NLog;
using ImportData.IntegrationServicesClient.Models;
using System.IO;
using System.Diagnostics.Contracts;
using ImportData.Entities.Databooks;
using System.Linq;
using ImportData.Entities.EDocs;

namespace ImportData
{
  class SupAgreement : Contract
  {
    public override int PropertiesCount { get { return 22; } }
    protected override Type EntityType { get { return typeof(ISupAgreements); } }
  }
}
