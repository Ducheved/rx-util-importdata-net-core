﻿using System;
using System.Collections.Generic;
using System.Globalization;
using NLog;
using ImportData.IntegrationServicesClient.Models;
using System.IO;
using ImportData.Entities.EDocs;

namespace ImportData
{
  class IncomingLetter : DocumentEntity
  {
    public override int PropertiesCount { get { return 14; } }
    protected override Type EntityType { get { return typeof(IIncomingLetters); } }

    public override string GetName()
    {
      var subject = ResultValues[Constants.KeyAttributes.Subject];
      var documentKind = ResultValues[Constants.KeyAttributes.DocumentKind];
      var counterparty = ResultValues[Constants.KeyAttributes.Correspondent];
      var registrationNumber = ResultValues[Constants.KeyAttributes.RegistrationNumber];
      var registrationDate = (DateTimeOffset)ResultValues[Constants.KeyAttributes.RegistrationDate];
      return $"{documentKind} №{registrationNumber} от {registrationDate.ToString("dd.MM.yyyy")} с {counterparty} \"{subject}\"";
    }

  }
}
