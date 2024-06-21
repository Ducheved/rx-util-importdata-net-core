using System.Collections.Generic;

namespace ImportData.IntegrationServicesClient.Models
{
  [EntityName("Организация")]
  public class ICompanies : ICounterparties
  {
    [PropertyOptions("КПП", RequiredType.NotRequired, PropertyType.Simple, AdditionalCharacters.ForSearch)]
    public string TRRC { get; set; }
    public bool IsCardReadOnly { get; set; }
    [PropertyOptions("Юрид. наименование", RequiredType.NotRequired, PropertyType.Simple)]
    public string LegalName { get; set; }
    [PropertyOptions("Головная орг.", RequiredType.NotRequired, PropertyType.EntityWithCreate)]
    public ICompanies HeadCompany { get; set; }

    public static ICompanies CastCounterpartyToCompany(ICounterparties counterparty)
    {
      var company = new ICompanies
      {
        Name = counterparty.Name,
        TIN = counterparty.TIN,
        LegalAddress = counterparty.LegalAddress,
        PostalAddress = counterparty.PostalAddress,
        Phones = counterparty.Phones,
        Email = counterparty.Email,
        Homepage = counterparty.Homepage,
        Note = counterparty.Note,
        Nonresident = counterparty.Nonresident,
        PSRN = counterparty.PSRN,
        NCEO = counterparty.NCEO,
        NCEA = counterparty.NCEA,
        Account = counterparty.Account,
        CanExchange = counterparty.CanExchange,
        Code = counterparty.Code,
        Status = counterparty.Status,
        Id = counterparty.Id,
        City = counterparty.City,
        Region = counterparty.Region,
        Bank = counterparty.Bank,
        Responsible = counterparty.Responsible,

        TRRC = string.Empty,
        IsCardReadOnly = false,
        LegalName = string.Empty,
        HeadCompany = null,
      };

      return company;
    }

    new public static ICompanies FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch["Name"];
      return BusinessLogic.GetEntityWithFilter<ICompanies>(x => x.Name == name, exceptionList, logger);
    }

    new public static ICompanies CreateEntity(Dictionary<string, string> propertiesForSearch, Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      var name = propertiesForSearch["Name"];
      return BusinessLogic.CreateEntity<ICompanies>(new ICompanies() { Name = name, Status = "Active" }, exceptionList, logger);
    }
    new public static bool FillProperies(Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      entity.ResultValues["Nonresident"] = BusinessLogic.GetPropertyResident((string)entity.ResultValues["Nonresident"]);
      entity.ResultValues["Status"] = "Active";
      return false;
    }
    new public static void CreateOrUpdate(IEntity entity, bool isNewEntity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      if (isNewEntity)
        BusinessLogic.CreateEntity((ICompanies)entity, exceptionList, logger);
      else
        BusinessLogic.UpdateEntity((ICompanies)entity, exceptionList, logger);
    }
  }
}
