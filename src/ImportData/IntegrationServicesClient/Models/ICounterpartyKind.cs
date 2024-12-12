using System;
using System.Collections.Generic;

namespace ImportData.IntegrationServicesClient.Models
{
    [EntityName("Вид контрагента")]
    public class ICounterpartyKind : IEntity
    {
        public string Name { get; set; }

        [PropertyOptions("Статус", RequiredType.Required, PropertyType.Simple)]
        public string Status { get; set; }

        [PropertyOptions("Примечание", RequiredType.NotRequired, PropertyType.Simple)]
        public string Note { get; set; }

        [PropertyOptions("Системный идентификатор", RequiredType.NotRequired, PropertyType.Simple)]
        public string Sid { get; set; }

        public int Id { get; set; }

        // Добавляем статические методы для поиска и создания
        public static ICounterpartyKind FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
        {
            var name = propertiesForSearch[Constants.KeyAttributes.CustomFieldName];
            return BusinessLogic.GetEntityWithFilter<ICounterpartyKind>(
                x => x.Name == name && x.Status == "Active",
                exceptionList,
                logger);
        }

        public static ICounterpartyKind CreateEntity(Dictionary<string, string> propertiesForSearch, Entity entity, List<Structures.ExceptionsStruct> exceptionList, bool isBatch, NLog.Logger logger)
        {
            var name = propertiesForSearch[Constants.KeyAttributes.CustomFieldName];
            return BusinessLogic.CreateEntity<ICounterpartyKind>(new ICounterpartyKind
            {
                Name = name,
                Status = "Active",
                Note = string.Empty,
                Sid = string.Empty
            }, exceptionList, logger);
        }

        public static IEntityBase CreateOrUpdate(IEntity entity, bool isNewEntity, bool isBatch, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
        {
            if (isNewEntity)
                return BusinessLogic.CreateEntity((ICounterpartyKind)entity, exceptionList, logger);
            else
                return BusinessLogic.UpdateEntity((ICounterpartyKind)entity, exceptionList, logger);
        }
    }
}