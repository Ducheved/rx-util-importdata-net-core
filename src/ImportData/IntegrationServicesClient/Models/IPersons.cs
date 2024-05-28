using System;
using System.Collections.Generic;

namespace ImportData.IntegrationServicesClient.Models
{
    [EntityName("Персоны")]
    public class IPersons : ICounterparties
    {
		private DateTimeOffset? dateOfBirth;

        [PropertyOptions("Фамилия", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
        public string LastName { get; set; }

		[PropertyOptions("Имя", RequiredType.Required, PropertyType.Simple, AdditionalCharacters.ForSearch)]
		public string FirstName { get; set; }

		[PropertyOptions("Отчество", RequiredType.NotRequired, PropertyType.Simple, AdditionalCharacters.ForSearch)]
		public string MiddleName { get; set; }

		[PropertyOptions("Дата рождения", RequiredType.NotRequired, PropertyType.Simple)]
		public DateTimeOffset? DateOfBirth
		{
		    get { return dateOfBirth; }
		    set { dateOfBirth = value.HasValue ? new DateTimeOffset(value.Value.Date, TimeSpan.Zero) : new DateTimeOffset?(); }
		}

		[PropertyOptions("СНИЛС", RequiredType.NotRequired, PropertyType.Simple)]
		public string INILA { get; set; }

        public string ShortName { get; set; }

		[PropertyOptions("Пол", RequiredType.NotRequired, PropertyType.Simple)]
		public string Sex { get; set; }

		new public static IPersons CreateEntity(Dictionary<string, string> propertiesForSearch, Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
		{
			var firstName = propertiesForSearch["FirstName"];
			var middleName = propertiesForSearch["MiddleName"];
			var lastName = propertiesForSearch["LastName"];

			return BusinessLogic.CreateEntity<IPersons>(new IPersons() { FirstName = firstName, MiddleName = middleName, LastName = lastName, Name = string.Format("{0} {1} {2}", lastName, firstName, middleName), Status = "Active" }, exceptionList, logger);
		}

		new public static IEntity FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
		{
			var firstName = propertiesForSearch["FirstName"];
			var middleName = propertiesForSearch["MiddleName"];
			var lastName = propertiesForSearch["LastName"];

			return BusinessLogic.GetEntityWithFilter<IPersons>(x => x.FirstName == firstName && x.MiddleName == middleName && x.LastName == lastName, exceptionList, logger);
		}

		new public static string GetName(Entity entity)
		{
			var firstName = entity.ResultValues["FirstName"];
			var middleName = entity.ResultValues["MiddleName"];
			var lastName = entity.ResultValues["LastName"];

			return string.Format("{0} {1} {2}", lastName, firstName, middleName);
		}

		new public static bool FillProperies(Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
		{
			entity.ResultValues["Name"] = GetName(entity);
			entity.ResultValues["Sex"] = BusinessLogic.GetPropertySex((string)entity.ResultValues["Sex"]);
			entity.ResultValues["Status"] = "Active";

			return false;
		}
	}
}
