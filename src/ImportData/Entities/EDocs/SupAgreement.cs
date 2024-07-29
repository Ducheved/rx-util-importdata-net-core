﻿using System;
using System.Collections.Generic;
using System.Globalization;
using NLog;
using ImportData.IntegrationServicesClient.Models;
using System.IO;
using System.Diagnostics.Contracts;
using ImportData.Entities.Databooks;
using System.Linq;

namespace ImportData
{
  class SupAgreement : Entity
  {
    public override int PropertiesCount { get { return 22; } }
    protected override Type EntityType { get { return typeof(ISupAgreements); } }

    public override string GetName()
    {
      var subject = ResultValues[Constants.KeyAttributes.Subject];
      var documentKind = ResultValues[Constants.KeyAttributes.DocumentKind];
      var counterparty = ResultValues[Constants.KeyAttributes.Counterparty];
      var registrationNumber = ResultValues[Constants.KeyAttributes.RegistrationNumber];
      var registrationDate = (DateTimeOffset)ResultValues[Constants.KeyAttributes.RegistrationDate];
      return $"{documentKind} №{registrationNumber} от {registrationDate.ToString("dd.MM.yyyy")} с {counterparty} \"{subject}\"";
    }
    
  }
}
