using ImportData.IntegrationServicesClient.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImportData.Dto
{
    public class Attachment
    {
        public string AttachmentPath { get; set; }
        public IAssociatedApplications AssociatedApplication { get; set; }
        public string Extension { get; set; }
        public byte[] Body { get; set; }
    }
}
