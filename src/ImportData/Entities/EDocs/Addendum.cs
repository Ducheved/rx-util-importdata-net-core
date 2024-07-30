using ImportData.Entities.EDocs;
using ImportData.IntegrationServicesClient.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ImportData
{
  class Addendum : DocumentEntity
  {
    public override int PropertiesCount { get { return 16; } }
    protected override Type EntityType { get { return typeof(IAddendums); } }
  }
}
