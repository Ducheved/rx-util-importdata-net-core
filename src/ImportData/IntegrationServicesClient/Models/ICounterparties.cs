﻿namespace ImportData.IntegrationServicesClient.Models
{
    [EntityName("Контрагент")]
    public class ICounterparties : IEntity
    {
		[PropertyOptions("ИНН", RequiredType.NotRequired, PropertyType.Simple, AdditionalCharacters.ForSearch)]
		public string TIN { get; set; }

		// ВАЖНО: в организациях зовется по-другому
		[PropertyOptions("Адрес регистрации", RequiredType.NotRequired, PropertyType.Simple)]
		public string LegalAddress { get; set; }

		[PropertyOptions("Почтовый адрес", RequiredType.NotRequired, PropertyType.Simple)]
		public string PostalAddress { get; set; }

		[PropertyOptions("Телефоны", RequiredType.NotRequired, PropertyType.Simple)]
		public string Phones { get; set; }

		[PropertyOptions("Эл. почта", RequiredType.NotRequired, PropertyType.Simple)]
		public string Email { get; set; }

		[PropertyOptions("Сайт", RequiredType.NotRequired, PropertyType.Simple)]
		public string Homepage { get; set; }

		[PropertyOptions("Примечание", RequiredType.NotRequired, PropertyType.Simple)]
		public string Note { get; set; }

		[PropertyOptions("Нерезидент", RequiredType.NotRequired, PropertyType.Simple)]
		public bool Nonresident { get; set; }

		[PropertyOptions("ОГРН", RequiredType.NotRequired, PropertyType.Simple, AdditionalCharacters.ForSearch)]
		public string PSRN { get; set; }

		[PropertyOptions("ОКПО", RequiredType.NotRequired, PropertyType.Simple)]
		public string NCEO { get; set; }

		[PropertyOptions("ОКВЭД", RequiredType.NotRequired, PropertyType.Simple)]
		public string NCEA { get; set; }

		[PropertyOptions("Номер счета", RequiredType.NotRequired, PropertyType.Simple)]
		public string Account { get; set; }

		[PropertyOptions("", RequiredType.NotRequired, PropertyType.Simple)]
		public string CanExchange { get; set; }
		
		[PropertyOptions("", RequiredType.NotRequired, PropertyType.Simple)]
		public string Code { get; set; }
		
		[PropertyOptions("", RequiredType.Required, PropertyType.Simple)]
		public string Status { get; set; }

		[PropertyOptions("Населенный пункт", RequiredType.NotRequired, PropertyType.Entity)]
		public ICities City { get; set; }

		[PropertyOptions("Регион", RequiredType.NotRequired, PropertyType.Entity)]
		public IRegions Region { get; set; }

		[PropertyOptions("Банк", RequiredType.NotRequired, PropertyType.Entity)]
		public IBanks Bank { get; set; }

		[PropertyOptions("Ответственный", RequiredType.NotRequired, PropertyType.Entity)]
		public IEmployees Responsible { get; set; }
    }
}
