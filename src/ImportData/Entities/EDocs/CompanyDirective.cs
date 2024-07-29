using ImportData.Entities.EDocs;
using ImportData.IntegrationServicesClient.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ImportData
{
  class CompanyDirective : DocumentEntity
  {
    public override int PropertiesCount { get { return 14; } }
    protected override Type EntityType { get { return typeof(ICompanyDirective); } }

    public override string GetName()
    {
      var documentKind = ResultValues[Constants.KeyAttributes.DocumentKind];
      var subject = ResultValues[Constants.KeyAttributes.Subject];
      return string.Format("{0} \"{1}\"", documentKind, subject);
    }

  }
}
