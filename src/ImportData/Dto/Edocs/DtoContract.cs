using System;
using System.Collections.Generic;
using System.Text;
using ImportData.IntegrationServicesClient.Models;

namespace ImportData.Dto.Edocs
{
    public class DtoContract : IDtoEntity
    {
        public uint RowNumber { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset? Created { get; set; }
        public ICounterparties Counterparty { get; set; }
        public IDocumentKinds DocumentKind { get; set; }
        public IDocumentGroupBases DocumentGroup { get; set; }
        public string Subject { get; set; }
        public IBusinessUnits BusinessUnit { get; set; }
        public IDepartments Department { get; set; }
        public DateTimeOffset? ValidFrom { get; set; }
        public DateTimeOffset? ValidTill { get; set; }
        public double TotalAmount { get; set; }
        public ICurrencies Currency { get; set; }
        public IEmployees ResponsibleEmployee { get; set; }
        public IEmployees OurSignatory { get; set; }
        public string Note { get; set; }
        public IDocumentRegisters DocumentRegisters { get; set; }
        public DateTimeOffset? RegistrationDate { get; set; }
        public string RegistrationNumber { get; set; }
        public string RegistrationState { get; set; }
        public ICaseFiles CaseFile { get; set; }
        public DateTimeOffset? PlacedToCaseFileDate { get; set; }
        public string LifeCycleState { get; set; }
        public string Attachment { get; set; }
        public IContracts Contract { get; set; }
    }
}
