﻿using ImportData.Dto;
using ImportData.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportData
{
    public class Entity
    {
        public string[] Parameters;
        public static Dictionary<string, string> ExtraParameters;
        public int PropertiesCount = 0;

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
            return null;
        }

        //public virtual IEnumerable<Structures.ExceptionsStruct> Validate(NLog.Logger logger, int shift = 0)
        //{
        //    return new List<Structures.ExceptionsStruct>();
        //}

        public virtual IDtoEntity Validate(List<Structures.ExceptionsStruct> exceptionList, uint rowNumber, NLog.Logger logger, int shift = 0)
        {
            return null;
        }

        public static async Task SaveToRX(List<IDtoEntity> dtoEntities, List<Structures.ExceptionsStruct> exceptions, NLog.Logger logger)
        {
            await Task.Run(() => Console.WriteLine());
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

                if (double.TryParse(value.Trim(), style, culture, out double dateDouble))
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
