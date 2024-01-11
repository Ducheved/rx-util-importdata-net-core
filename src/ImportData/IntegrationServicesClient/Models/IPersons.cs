using System;

namespace ImportData.IntegrationServicesClient.Models
{
    [EntityName("Персоны")]
    public class IPersons : ICounterparties
    {
		private DateTimeOffset? dateOfBirth;

        [PropertyOptions("Фамилия", RequiredType.ForSearch, PropertyType.Simple)]
        public string LastName { get; set; }

		[PropertyOptions("Имя", RequiredType.ForSearch, PropertyType.Simple)]
		public string FirstName { get; set; }

		[PropertyOptions("Отчество", RequiredType.ForSearch, PropertyType.Simple)]
		public string MiddleName { get; set; }

		[PropertyOptions("Дата рождения", RequiredType.NotRequired, PropertyType.WithTransformation)]
		public DateTimeOffset? DateOfBirth
		{
		    get { return dateOfBirth; }
		    set { dateOfBirth = value.HasValue ? new DateTimeOffset(value.Value.Date, TimeSpan.Zero) : new DateTimeOffset?(); }
		}

		[PropertyOptions("СНИЛС", RequiredType.NotRequired, PropertyType.WithTransformation)]
		public string INILA { get; set; }

        public string ShortName { get; set; }

		[PropertyOptions("Пол", RequiredType.NotRequired, PropertyType.WithTransformation)]
		public string Sex { get; set; }
    }
}
