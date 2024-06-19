using System;
using System.Collections.Generic;
using System.Text;

namespace ImportData.IntegrationServicesClient.Models
{
    public class IEntityBase
    {
        public int Id { get; set; }


        /// <summary>
		/// Поиск сущности для обновления или установки значения свойства в связанную сущность.
		/// </summary>
		/// <param name="propertiesForSearch">Поля со значениями для поиска сущности.</param>
		/// <param name="entity">Сущность со всеми параметрами загрузки. (Использовать, если необходимо написать сложную логику поиска. Могут быть заполнены не все поля.)</param>
		/// <param name="isEntityForUpdate">Вызов из обновляемой сущности (могут использоваться разные наборы полей для поиска).</param>
		/// <param name="exceptionList">Список ошибок.</param>
		/// <param name="logger">Логировщик.</param>
		/// <returns>Найденная сущность.</returns>
        public static IEntityBase FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
        {
            return null;
        }

        /// <summary>
		/// Создание сущности с заполнением полей для установки значения свойства в связанную сущность.
		/// </summary>
		/// <param name="propertiesForSearch">Поля со значениями для создания сущности.</param>
		/// <param name="entity">Сущность со всеми параметрами загрузки. (Использовать, если необходимо написать сложную логику создания. Могут быть заполнены не все поля.)</param>
		/// <param name="exceptionList">Список ошибок.</param>
		/// <param name="logger">Логировщик.</param>
		/// <returns>Созданная сущность.</returns>
        public static IEntityBase CreateEntity(Dictionary<string, string> propertiesForSearch, Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
        {
            return null;
        }

        /// <summary>
        /// Получить имя сущности для заполнения (оно может составляться из нескольких столбцов шаблона).
        /// </summary>
        /// <param name="entity">Сущность со всеми параметрами загрузки. (Предполагается, что при заполнении имени все поля уже считаны.)</param>
        /// <returns>Наименование.</returns>
        public static string GetName(Entity entity)
        {
            return string.Empty;
        }

        /// <summary>
		/// Специфичное заполнение / преобразование / проверка полей сущность, которую нельзя унифицировать.
		/// </summary>
		/// <param name="entity">Сущность со всеми параметрами загрузки. (Предполагается, что при заполнении все поля уже считаны.)</param>
		/// <param name="exceptionList">Список ошибок.</param>
		/// <param name="logger">Логировщик.</param>
		/// <returns>True, если были ошибки заполнения свойств, иначе false.</returns>
        public static bool FillProperies(Entity entity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
        {
            return false;
        }

        /// <summary>
        /// Создание / обновление сущности через OData.
        /// </summary>
        /// <param name="entity">Сущность.</param>
        /// <param name="isNewEntity">True, если сущность создается, false, если обновляется.</param>
        /// <param name="exceptionList">Список ошибок.</param>
		/// <param name="logger">Логировщик.</param>
        public static void CreateOrUpdate(IEntityBase entity, bool isNewEntity, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
        {
           // Будет переопределен в дочерних классах.
        }
    }
}
