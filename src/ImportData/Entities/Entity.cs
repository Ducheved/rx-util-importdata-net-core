using ImportData.IntegrationServicesClient;
using ImportData.IntegrationServicesClient.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImportData
{
    public class Entity
    {
        public string[] Parameters;
        public Dictionary<string, string> ExtraParameters;
        public int PropertiesCount = 0;

		public Dictionary<string, string> NamingParameters;
        protected Dictionary <string, object> ResultValues;
        protected Type EntityType {  get; }

		/// <summary>
		/// Получить наименование число запрашиваемых параметров.
		/// </summary>
		/// <returns>Число запрашиваемых параметров.</returns>
		public virtual int GetPropertiesCount()
        {
            return PropertiesCount;
        }

        /// <summary>
        /// Сохранение сущности в RX.
        /// </summary>
        /// <param name="logger">Логировщик.</param>
        /// <param name="shift">Сдвиг по горизонтали в XLSX документе. Необходим для обработки документов, составленных из элементов разных сущностей.</param>
        /// <returns>Список ошибок.</returns>
        public virtual IEnumerable<Structures.ExceptionsStruct> SaveToRX(NLog.Logger logger, bool supplementEntity, string ignoreDuplicates, int shift = 0)
        {
            return new List<Structures.ExceptionsStruct>();
        }

        public virtual IEnumerable<Structures.ExceptionsStruct> Save(NLog.Logger logger, bool supplementEntity, string ignoreDuplicates, int shift = 0)
        {
			// Инициализация структур
			var exceptionList = new List<Structures.ExceptionsStruct>();
			ResultValues = new Dictionary<string, object>();

			var properties = EntityType.GetProperties();
			foreach (var property in properties)
			{
				var options = GetPropertyOptions(property);
				if (options == null || !NamingParameters.ContainsKey(options.ExcelName))
					continue;

				var variableForParameters = NamingParameters[options.ExcelName].Trim();
				// Подумать: их можно обработать первыми, возможно неск. циклов, сперва только по обязательным св-вам.
				if (options.IsRequired())
				{
					// Работа с обязательными полями (чтобы выдать ошибки)
					if (string.IsNullOrEmpty(variableForParameters))
					{
						var message = string.Format("Не заполнено поле {0}.", options.ExcelName);
						exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = "Error", Message = message });
						logger.Error(message);

						return exceptionList;
					}
				}

				// работа с полями-сущностями
				if (options.Type == PropertyType.Entity || options.Type == PropertyType.EntityWithCreate)
				{
					// специфика поиска, не всегда по имени
					// может поиск запрятать в нужную сущность?
					// можно ли преобразовать тип в Т? 
				}

				// работа с остальными полями (заполнить as is)
				if (options.IsSimple())
					ResultValues.Add(options.ExcelName, variableForParameters);

				ResultValues.Add(options.ExcelName, null);
			}
			// поиск сущности (если не нужны дубли)

			// специфичная донастройка

			// заполнение полей
			

			return new List<Structures.ExceptionsStruct>();
        }

		/// <summary>
		/// Заполнение /обновление полей сущности.
		/// </summary>
		/// <param name="entity">Сущность RX для заполнения.</param>
		private void UpdateProperties(IEntity entity)
		{
			var entityProperties = EntityType.GetProperties();
			foreach (var property in entityProperties)
			{
				if (ResultValues.ContainsKey(property.Name))
					property.SetValue(entity, ResultValues[property.Name]);
			}
		}

		/// <summary>
		/// Получить значения атрибутов свойства.
		/// </summary>
		/// <param name="p">Свойство.</param>
		/// <returns>Значения атрибутов.</returns>
		PropertyOptions GetPropertyOptions(PropertyInfo p)
		{
			Attribute[] attrs = Attribute.GetCustomAttributes(p);

			foreach (Attribute attr in attrs)
			{
				if (attr is PropertyOptions)
					return (PropertyOptions)attr;
			}

			return null;
		}

		/// <summary>
		/// Преобразовать зачение в дату.
		/// </summary>
		/// <param name="value">Значение.</param>
		/// <param name="style">Стиль преобразования числовой строки.</param>
		/// <param name="culture">Культура.</param>
		/// <returns>Преобразованная дата.</returns>
		/// <exception cref="FormatException" />
		public DateTimeOffset ParseDate(string value, NumberStyles style, CultureInfo culture)
        {
            if (!string.IsNullOrEmpty(value))
            {
                DateTimeOffset date;
                if (DateTimeOffset.TryParse(value.Trim(), culture.DateTimeFormat, DateTimeStyles.AssumeUniversal, out date))
                    return date;

                var dateDouble = 0.0;
                if (double.TryParse(value.Trim(), style, culture, out dateDouble))
                    return new DateTimeOffset(DateTime.FromOADate(dateDouble), TimeSpan.Zero);

                throw new FormatException("Неверный формат строки.");
            }
            else
                return DateTimeOffset.MinValue;
        }

        /// <summary>
        /// Получить начало дня.
        /// </summary>
        /// <param name="dateTimeOffset">Дата-время.</param>
        /// <returns>Начало дня.</returns>
        public DateTimeOffset BeginningOfDay(DateTimeOffset dateTimeOffset)
        {
            return new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 0, 0, 0, dateTimeOffset.Offset);
        }
    }
}
