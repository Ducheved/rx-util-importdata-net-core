using ImportData.Entities.EDocs;
using ImportData.IntegrationServicesClient.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ImportData
{
  class OutgoingLetter : IncomingLetter
  {
    public override int PropertiesCount { get { return 12; } }
    protected override Type EntityType { get { return typeof(IOutgoingLetters); } }
  }
}
