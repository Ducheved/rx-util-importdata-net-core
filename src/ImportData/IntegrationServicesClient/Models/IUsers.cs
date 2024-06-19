namespace ImportData.IntegrationServicesClient.Models
{
  [EntityName("Пользователи")]
  public class IUsers : IRecipients
  {
    [PropertyOptions("Логин", RequiredType.NotRequired, PropertyType.EntityWithCreate)]
    public ILogins Login { get; set; }
    [PropertyOptions("Подразделение", RequiredType.Required, PropertyType.EntityWithCreate)]
    public IDepartments Department { get; set; }
    [PropertyOptions("Персона", RequiredType.NotRequired, PropertyType.EntityWithCreate, AdditionalCharacters.CreateFromOtherProperties)]
    public IPersons Person { get; set; }
    [PropertyOptions("Должность", RequiredType.NotRequired, PropertyType.EntityWithCreate, AdditionalCharacters.CreateFromOtherProperties)]
    public IJobTitles JobTitle { get; set; }
  }
}
