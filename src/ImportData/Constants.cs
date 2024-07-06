using ImportData.IntegrationServicesClient.Models;
using Microsoft.Data.Edm.Library;
using System;
using System.Collections.Generic;

namespace ImportData
{
  public class Constants
  {
    public class RolesGuides
    {
      public static readonly Guid RoleContractResponsible = new Guid("25C48B40-6111-4283-A94E-7D50E68DECC1");
      public static readonly Guid RoleIncomingDocumentsResponsible = new Guid("63EBE616-8780-4CBB-9AF7-C16251B38A84");
      public static readonly Guid RoleOutgoingDocumentsResponsible = new Guid("372D8FDB-316E-4F3C-9F6D-C2C1292BBFAE");
    }

    public class ErrorTypes
    {
      public const string Error = "Error";
      public const string Warn = "Warn";
      public const string Debug = "Debug";
    }

    public class KeyAttributes
    {
      public const string Name = "Name";
      public const string BusinessUnit = "BusinessUnit";
      public const string Counterparty = "Counterparty";
      public const string Department = "Department";
      public const string HeadCompany = "HeadCompany";
      public const string HeadOffice = "HeadOffice";
      public const string Manager = "Manager";
      public const string CEO = "CEO";
      public const string Region = "Region";
      public const string City = "City";
      public const string RegistrationNumber = "RegistrationNumber";
      public const string RegistrationDate = "RegistrationDate";
      public const string DocumentGroup = "DocumentGroup";
      public const string DocumentKind = "DocumentKind";
      public const string Subject = "Subject";
      public const string DocumentRegister = "DocumentRegister";
      public const string DeliveryMethod = "DeliveryMethod";
      public const string Correspondent = "Correspondent";
      public const string LeadingDocument = "LeadingDocument";
      public const string DocumentDate = "DocumentDate";
      public const string OutgoingDocumentBase = "OutgoingDocumentBase";
    }

    public class SheetNames
    {
      public const string BusinessUnits = "НашиОрганизации";
      public const string Departments = "Подразделения";
      public const string Employees = "Сотрудники";
      public const string Companies = "Контрагенты";
      public const string Persons = "Персоны";
      public const string Contracts = "Договоры";
      public const string SupAgreements = "Доп.Соглашения";
      public const string IncomingLetters = "ВходящиеПисьма";
      public const string OutgoingLetters = "ИсходящиеПисьма";
      public const string OutgoingLettersAddressees = "ИсходящиеПисьмаАдресаты";
      public const string Orders = "Приказы";
      public const string Addendums = "Приложения";
      public const string Contact = "Контактные лица";
      public const string CompanyDirectives = "Распоряжения";
      public const string Logins = "Логины";
      public const string Substitutions = "Замещения";
      public const string CaseFiles = "Номенклатура дел";
      public const string Countries = "Страны";
      public const string Currencies = "Валюты";

    }

    public class Actions
    {
      public const string ImportCompany = "importcompany";
      public const string ImportCompanies = "importcompanies";
      public const string ImportPersons = "importpersons";
      public const string ImportContracts = "importcontracts";
      public const string ImportSupAgreements = "importsupagreements";
      public const string ImportIncomingLetters = "importincomingletters";
      public const string ImportOutgoingLetters = "importoutgoingletters";
      public const string ImportOutgoingLettersAddressees = "importoutgoinglettersaddressees";
      public const string ImportOrders = "importorders";
      public const string ImportAddendums = "importaddendums";
      public const string ImportDepartments = "importdepartments";
      public const string ImportEmployees = "importemployees";
      public const string ImportContacts = "importcontacts";
      public const string ImportCompanyDirectives = "importcompanydirectives";
      public const string ImportLogins = "importlogins";
      public const string ImportSubstitutions = "importsubstitutions";
      public const string ImportCaseFiles = "importcasefiles";
      public const string ImportCountries = "importсountries";
      public const string ImportCurrencies = "importcurrencies";

      // Инициализация клиента, для тестов.
      public const string InitForTests = "init";

      public static Dictionary<string, string> dictActions = new Dictionary<string, string>
            {
                {ImportCompany, ImportCompany},
                {ImportCompanies, ImportCompanies},
                {ImportPersons, ImportPersons},
                {ImportContracts, ImportContracts},
                {ImportSupAgreements, ImportSupAgreements},
                {ImportIncomingLetters, ImportIncomingLetters},
                {ImportOutgoingLetters, ImportOutgoingLetters},
                {ImportOutgoingLettersAddressees, ImportOutgoingLettersAddressees},
                {ImportOrders, ImportOrders},
                {ImportAddendums, ImportAddendums},
                {ImportDepartments, ImportDepartments},
                {ImportEmployees, ImportEmployees},
                {ImportContacts, ImportContacts},
                {ImportCompanyDirectives, ImportCompanyDirectives},
                {ImportLogins, ImportLogins},
                {ImportCaseFiles, ImportCaseFiles},
                {ImportCountries, ImportCountries},
                {ImportCurrencies, ImportCurrencies},
                {ImportSubstitutions, ImportSubstitutions},

                // Инициализация клиента, для тестов.
                {InitForTests, InitForTests}
            };
    }

    public class Resources
    {
      public const string IncorrecPsrnLength = "ОГРН должен содержать 13 или 15 цифр.";
      public const string IncorrecTrrcLength = "КПП должен содержать 9 цифр.";
      public const string IncorrecCodeDepartmentLength = "Код подраздленения не должен содержать больше 10 цифр.";
      public const string NotOnlyDigitsTin = "Введите ИНН, состоящий только из цифр.";
      public const string CompanyIncorrectTinLength = "Введите правильное число цифр в ИНН, для организаций - 10, для ИП - 12.";
      public const string PeopleIncorrectTinLength = "Введите правильное число цифр в ИНН, для физических лиц - 12.";
      public const string NonresidentIncorectTinLength = "Введите правильное число цифр в ИНН, для нерезидента - до 12.";
      public const string NotValidTinRegionCode = "Введите ИНН с корректным кодом региона";
      public const string NotValidTin = "Введите ИНН с корректной контрольной цифрой.";
      public const string IncorrecNceoLength = "ОКПО должен содержать 8 или 10 цифр";
      public const string EmptyColumn = "Не заполнено поле {0}.";
      public const string EmptyProperty = "Не найдено значение для свойства {0}.";
      public const string FileNotExist = "Не найден файл по пути {0}.";
      public const string NeedRequiredDocumentBody = "Импортирумая сущность должна содержать тело документа, столбец с наименованием {0}.";
    }

    public class ConfigServices
    {
      public const string IntegrationServiceUrlParamName = "INTEGRATION_SERVICE_URL";
      public const string RequestTimeoutParamName = "INTEGRATION_SERVICE_REQUEST_TIMEOUT";
    }

    public const string ignoreDuplicates = "ignore";
    public const string CellNameFile = "Файл";
    public static Dictionary<string, string> dictIgnoreDuplicates = new Dictionary<string, string>
    {
      { ignoreDuplicates, ignoreDuplicates}
    };
    public static readonly List<Type> RequiredDocumentBody = new List<Type>
    {
      typeof(IContracts),
      typeof(IIncomingLetters),
      typeof(ISupAgreements),
      typeof(IAddendums)
    };
  }

  public enum RequiredType
  {
    NotRequired = 0,
    Required = 1
  }

  public enum PropertyType
  {
    Simple = 0,
    Entity = 1,
    EntityWithCreate = 2
  }

  public enum AdditionalCharacters
  {
    Default = 0,
    ForSearch = 1,
    CreateFromOtherProperties = 2
  }
}
