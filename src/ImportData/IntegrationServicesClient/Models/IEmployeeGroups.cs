using System;
using System.Collections.Generic;
using System.Linq;

namespace ImportData.IntegrationServicesClient.Models
{
    [EntityName("Группа сотрудников")]
    public class IEmployeeGroups : IEntity
    {
        public int Id { get; set; }

        [PropertyOptions("Наименование", RequiredType.Required, PropertyType.Simple)]
        public string Name { get; set; } // Убираем override

        [PropertyOptions("Код группы", RequiredType.NotRequired, PropertyType.Simple)]
        public string Code { get; set; }

        public string Status { get; set; }

        new public static IEmployeeGroups FindEntity(Dictionary<string, string> propertiesForSearch,
            Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList,
            NLog.Logger logger)
        {
            try
            {
                logger.Info($"=== Начало поиска IEmployeeGroups===");
                logger.Info($"Тип сущности: {typeof(IEmployeeGroups).FullName}");

                if (propertiesForSearch == null)
                {
                    logger.Error("propertiesForSearch is null");
                    return null;
                }

                logger.Info("Поисковые параметры:");
                foreach (var param in propertiesForSearch)
                {
                    logger.Info($"{param.Key}: {param.Value}");
                }

                if (!propertiesForSearch.ContainsKey(Constants.KeyAttributes.CustomFieldName))
                {
                    logger.Error($"Не найден ключ {Constants.KeyAttributes.CustomFieldName} в поисковых параметрах");
                    return null;
                }

                var name = propertiesForSearch[Constants.KeyAttributes.CustomFieldName];
                logger.Info($"Поиск группы по имени: {name}");

                var result = BusinessLogic.GetEntityWithFilter<IEmployeeGroups>(
                    x => x.Name.ToLower().Trim() == name.ToLower().Trim() && x.Status == "Active",
                    exceptionList,
                    logger);

                if (result != null)
                {
                    logger.Info($"Найдена группа: ID={result.Id}, Name={result.Name}");
                }
                else
                {
                    logger.Info("Группа не найдена");
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Ошибка при поиске IEmployeeGroups: {ex.Message}");
                logger.Error($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        new public static IEmployeeGroups CreateEntity(Dictionary<string, string> propertiesForSearch,
            Entity entity, List<Structures.ExceptionsStruct> exceptionList, bool isBatch,
            NLog.Logger logger)
        {
            try
            {
                logger.Info($"=== Начало создания IEmployeeGroups===");

                if (propertiesForSearch == null)
                {
                    logger.Error("propertiesForSearch is null при создании");
                    return null;
                }

                var name = propertiesForSearch[Constants.KeyAttributes.CustomFieldName];
                logger.Info($"Создание новой группы с именем: {name}");

                var employeeGroup = new IEmployeeGroups
                {
                    Name = name,
                    Code = name,
                    Status = Constants.AttributeValue[Constants.KeyAttributes.Status]
                };

                var result = BusinessLogic.CreateEntity(employeeGroup, exceptionList, logger);

                if (result != null)
                {
                    logger.Info($"Создана новая группа: ID={result.Id}, Name={result.Name}");
                }
                else
                {
                    logger.Error("Не удалось создать группу");
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Ошибка при создании IEmployeeGroups: {ex.Message}");
                logger.Error($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        new public static IEntityBase CreateOrUpdate(IEntity entity, bool isNewEntity, bool isBatch,
            List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
        {
            try
            {
                logger.Info($"=== Начало CreateOrUpdate для IEmployeeGroups ===");
                logger.Info($"isNewEntity: {isNewEntity}, entity: {entity?.GetType().Name}");

                if (entity == null)
                {
                    logger.Error("entity is null в CreateOrUpdate");
                    return null;
                }

                var employeeGroup = entity as IEmployeeGroups;
                if (employeeGroup == null)
                {
                    logger.Error($"Неверный тип сущности: {entity.GetType().Name}");
                    return null;
                }

                IEntityBase result;
                if (isNewEntity)
                {
                    logger.Info($"Создание новой группы: {employeeGroup.Name}");
                    result = BusinessLogic.CreateEntity(employeeGroup, exceptionList, logger);
                }
                else
                {
                    logger.Info($"Обновление группы: ID={employeeGroup.Id}, Name={employeeGroup.Name}");
                    result = BusinessLogic.UpdateEntity(employeeGroup, exceptionList, logger);
                }

                if (result != null)
                {
                    logger.Info($"Операция успешна: ID={result.Id}");
                }
                else
                {
                    logger.Error("Операция не удалась");
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Ошибка в CreateOrUpdate для IEmployeeGroups: {ex.Message}");
                logger.Error($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}