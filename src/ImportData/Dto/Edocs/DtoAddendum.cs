using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using ImportData.IntegrationServicesClient.Models;

namespace ImportData.Dto.Edocs
{
    public class DtoAddendum : IDtoEntity
    {
        public uint RowNumber { get; set; }
        public string Name { get; set; }
        public DateTimeOffset? Created { get; set; }
        public IDocumentKinds DocumentKind { get; set; }
        public string Subject { get; set; }
        public IBusinessUnits BusinessUnit { get; set; }
        public IDepartments Department { get; set; }
        public IEmployees OurSignatory { get; set; }
        public string Note { get; set; }
        public IDocumentRegisters DocumentRegister { get; set; }
        public DateTimeOffset? RegistrationDate { get; set; }
        public string RegistrationNumber { get; set; }
        public string RegistrationState { get; set; }
        public string LifeCycleState { get; set; }
        public IOfficialDocuments LeadingDocument { get; set; }
        public IAddendums Addendum { get; set; }
        public IElectronicDocumentVersionss LastVersion { get; set; }
        public Attachment Attachment { get; set; }
    }
}
