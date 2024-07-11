using System;
using System.Linq;
using System.Collections.Generic;
using NLog;
using System.IO;
using ImportData.IntegrationServicesClient;
using ImportData.IntegrationServicesClient.Models;
using Simple.OData.Client;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using ImportData.IntegrationServicesClient.Exceptions;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ImportData.Dto;

namespace ImportData
{
    public class BusinessLogic
    {
        /// <summary>
        /// Чтение атрибута EntityName.
        /// </summary>
        /// <param name="t">Тип класса.</param>
        /// <returns>Значение атрибута EntityName.</returns>
        internal static string PrintInfo(Type t)
        {
            Attribute[] attrs = Attribute.GetCustomAttributes(t);

            foreach (Attribute attr in attrs)
            {
                if (attr is EntityName)
                {
                    EntityName a = (EntityName)attr;

                    return a.GetName();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Получение экземпляра клиента OData.
        /// </summary>
        /// <returns>ODataClient.</returns>
        /// <remarks></remarks>
        public static ODataClient InstanceOData()
        {
            return Client.Instance();
        }

        #region Работа с сервисом интеграции.
        /// <summary>
        /// Получение сущности по фильтру.
        /// </summary>
        /// <typeparam name="T">Тип сущности.</typeparam>
        /// <param name="expression">Условие фильтрации.</param>
        /// <param name="exceptionList">Список ошибок.</param>
        /// <param name="logger">Логгер</param>
        /// <returns>Сущность.</returns>
        public static T GetEntityWithFilter<T>(Expression<Func<T, bool>> expression, List<Structures.ExceptionsStruct> exceptions, Logger logger, bool isExpand = false) where T : class
        {
            Expression<Func<T, bool>> condition = expression;
            var filter = new ODataExpression(condition);

            logger.Info(string.Format("Получение сущности {0}", PrintInfo(typeof(T))));

            try
            {
                var entities = Client.GetEntitiesByFilter<T>(filter, isExpand);

                if (entities.Count() > 1)
                {
                    var message = string.Format("Найдено несколько записей типа сущности \"{0}\" с именем \"{1}\". Проверьте, что выбрана верная запись.", PrintInfo(typeof(T)), entities.FirstOrDefault().ToString());
                    //exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Warn, Message = message });
                    exceptions.Add(new Structures.ExceptionsStruct { ExceptionType = ExceptionType.Warn, Message = message });
                    logger.Warn(message);
                }

                return entities.FirstOrDefault();
            }
            catch (Exception ex)
            {
                if (ex.InnerException is WebRequestException webEx)
                {
                    var message = $"Ошибка на стороне Directum RX. Код ошибки: {webEx.Code}, Причина: {webEx.ReasonPhrase}, Ответ сервиса интеграции: {webEx.Response}";
                    logger.Error(message);
                    //exceptions.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    exceptions.Add(new Structures.ExceptionsStruct { ExceptionType = ExceptionType.Error, Message = message });
                }

                if (ex.Message.Contains("(Not Found)"))
                    throw new FoundMatchesException("Проверьте коррекность адреса службы интеграции Directum RX.");

                if (ex.Message.Contains("(Unauthorized)"))
                    throw new FoundMatchesException("Проверьте коррекность указанной учетной записи.");

            }

            return null;
        }

        /// <summary>
        /// Получение сущностей по фильтру.
        /// </summary>
        /// <typeparam name="T">Тип сущности.</typeparam>
        /// <param name="expression">Условие фильтрации.</param>
        /// <param name="exceptionList">Список ошибок.</param>
        /// <param name="logger">Логгер</param>
        /// <returns>Сущности.</returns>
        public static IEnumerable<T> GetEntitiesWithFilter<T>(Expression<Func<T, bool>> expression, List<Structures.ExceptionsStruct> exceptions, Logger logger, bool isExpand = false) where T : class
        {
	        Expression<Func<T, bool>> condition = expression;
	        var filter = new ODataExpression(condition);

	        logger.Info(string.Format("Получение сущностей {0}", PrintInfo(typeof(T))));

	        try
	        {
		        var entities = Client.GetEntitiesByFilter<T>(filter, isExpand);

		        return entities ?? Enumerable.Empty<T>();
	        }
	        catch (Exception ex)
	        {
		        if (ex.InnerException is WebRequestException webEx)
		        {
			        var message = $"Ошибка на стороне Directum RX. Код ошибки: {webEx.Code}, Причина: {webEx.ReasonPhrase}, Ответ сервиса интеграции: {webEx.Response}";
			        logger.Error(message);
			        //exceptions.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    exceptions.Add(new Structures.ExceptionsStruct { ExceptionType = ExceptionType.Error, Message = message });
                }

		        if (ex.Message.Contains("(Not Found)"))
			        throw new FoundMatchesException("Проверьте коррекность адреса службы интеграции Directum RX.");

		        if (ex.Message.Contains("(Unauthorized)"))
			        throw new FoundMatchesException("Проверьте коррекность указанной учетной записи.");
	        }

	        return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Получение сущностей.
        /// </summary>
        /// <typeparam name="T">Тип сущности.</typeparam>
        /// <returns>Список сущностей.</returns>
        public static IEnumerable<T> GetEntities<T>(List<Structures.ExceptionsStruct> exceptions, Logger logger) where T : class
        {
            try
            {
                var entities = Client.GetEntities<T>();
                return entities;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is WebRequestException webEx)
                {
                    var message = $"Ошибка на стороне Directum RX. Код ошибки: {webEx.Code}, Причина: {webEx.ReasonPhrase}, Ответ сервиса интеграции: {webEx.Response}";
                    logger.Error(message);
                    //exceptions.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    exceptions.Add(new Structures.ExceptionsStruct { ExceptionType = ExceptionType.Error, Message = message });
                }

                if (ex.Message.Contains("(Not Found)"))
                    throw new FoundMatchesException("Проверьте коррекность адреса службы интеграции Directum RX.");

                if (ex.Message.Contains("(Unauthorized)"))
                    throw new FoundMatchesException("Проверьте коррекность указанной учетной записи.");
            }

            return null;
        }

        /// <summary>
        /// Создать сущность.
        /// </summary>
        /// <typeparam name="T">Тип сущности.</typeparam>
        /// <param name="entity">Экземпляр сущности.</param>
        /// <returns>Созданная сущность.</returns>
        public static T CreateEntity<T>(T entity, uint rowNumber, List<Structures.ExceptionsStruct> exceptions, Logger logger) where T : class
        {
            logger.Info(string.Format("Создание сущности {0}", PrintInfo(typeof(T))));
            try
            {
                var createdEntity = Client.CreateEntity(entity, logger);

                if (createdEntity == null)
                {
                    var message = $"Ошибка при создании сущности";
                    //exceptions.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ExceptionType = ExceptionType.Error, Message = message });
                }

                return createdEntity;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is WebRequestException webEx)
                {
                    var message = $"Ошибка на стороне Directum RX. Код ошибки: {webEx.Code}, Причина: {webEx.ReasonPhrase}, Ответ сервиса интеграции: {webEx.Response}";
                    logger.Error(message);
                    //exceptions.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ExceptionType = ExceptionType.Error, Message = message });
                }

                if (ex.Message.Contains("(Not Found)"))
                    throw new FoundMatchesException("Проверьте коррекность адреса службы интеграции Directum RX.");

                if (ex.Message.Contains("(Unauthorized)"))
                    throw new FoundMatchesException("Проверьте коррекность указанной учетной записи.");
            }

            return null;
        }

        public static T CreateEntity<T>(T entity, List<Structures.ExceptionsStruct> exceptions, Logger logger) where T : class
        {
            logger.Info(string.Format("Создание сущности {0}", PrintInfo(typeof(T))));
            try
            {
                var createdEntity = Client.CreateEntity(entity, logger);

                if (createdEntity == null)
                {
                    var message = $"Ошибка при создании сущности";
                    //exceptions.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    exceptions.Add(new Structures.ExceptionsStruct { ExceptionType = ExceptionType.Error, Message = message });
                }

                return createdEntity;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is WebRequestException webEx)
                {
                    var message = $"Ошибка на стороне Directum RX. Код ошибки: {webEx.Code}, Причина: {webEx.ReasonPhrase}, Ответ сервиса интеграции: {webEx.Response}";
                    logger.Error(message);
                    //exceptions.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    exceptions.Add(new Structures.ExceptionsStruct { ExceptionType = ExceptionType.Error, Message = message });
                }

                if (ex.Message.Contains("(Not Found)"))
                    throw new FoundMatchesException("Проверьте коррекность адреса службы интеграции Directum RX.");

                if (ex.Message.Contains("(Unauthorized)"))
                    throw new FoundMatchesException("Проверьте коррекность указанной учетной записи.");
            }

            return null;
        }

        public static void CreateEntityBatch<T>(T entity, uint rowNumber, List<Structures.ExceptionsStruct> exceptions, Logger logger) where T : class
        {
            logger.Info(string.Format("Создание сущности {0}", PrintInfo(typeof(T))));

            string message;

            try
            {
                Client.CreateEntityBatch(entity);
            }
            catch (Exception ex)
            {
                message = $"Ошибка при создании сущности";
                //exceptions.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ExceptionType = ExceptionType.Error, Message = message });

                if (ex.InnerException is WebRequestException webEx)
                {
                    message = $"Ошибка на стороне Directum RX. Код ошибки: {webEx.Code}, Причина: {webEx.ReasonPhrase}, Ответ сервиса интеграции: {webEx.Response}";
                    logger.Error(message);
                    //exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ExceptionType = ExceptionType.Error, Message = message });
                }

                if (ex.Message.Contains("(Not Found)"))
                    throw new FoundMatchesException("Проверьте коррекность адреса службы интеграции Directum RX.");

                if (ex.Message.Contains("(Unauthorized)"))
                    throw new FoundMatchesException("Проверьте коррекность указанной учетной записи.");
            }
        }

        /// <summary>
        /// Обновить сущность.
        /// </summary>
        /// <typeparam name="T">Тип сущности.</typeparam>
        /// <param name="entity">Экземпляор сущности.</param>
        /// <returns>Обновленная сущность.</returns>
        public static T UpdateEntity<T>(T entity, List<Structures.ExceptionsStruct> exceptionList, Logger logger) where T : class
        {
            try
            {
                var entities = Client.UpdateEntity<T>(entity);

                logger.Info(string.Format("Тип сущности {0} обновлен.", PrintInfo(typeof(T))));

                return entities;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is WebRequestException webEx)
                {
                    var message = $"Ошибка на стороне Directum RX. Код ошибки: {webEx.Code}, Причина: {webEx.ReasonPhrase}, Ответ сервиса интеграции: {webEx.Response}";
                    logger.Error(message);
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                }

                if (ex.Message.Contains("(Not Found)"))
                    throw new FoundMatchesException("Проверьте коррекность адреса службы интеграции Directum RX.");

                if (ex.Message.Contains("(Unauthorized)"))
                    throw new FoundMatchesException("Проверьте коррекность указанной учетной записи.");
            }

            return null;
        }

        public static T UpdateEntity<T>(T entity, uint rowNumber, List<Structures.ExceptionsStruct> exceptionList, Logger logger) where T : class
        {
            try
            {
                var entities = Client.UpdateEntity<T>(entity);

                logger.Info(string.Format("Тип сущности {0} обновлен.", PrintInfo(typeof(T))));

                return entities;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is WebRequestException webEx)
                {
                    var message = $"Ошибка на стороне Directum RX. Код ошибки: {webEx.Code}, Причина: {webEx.ReasonPhrase}, Ответ сервиса интеграции: {webEx.Response}";
                    logger.Error(message);
                    exceptionList.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ErrorType = Constants.ErrorTypes.Error, Message = message });
                }

                if (ex.Message.Contains("(Not Found)"))
                    throw new FoundMatchesException("Проверьте коррекность адреса службы интеграции Directum RX.");

                if (ex.Message.Contains("(Unauthorized)"))
                    throw new FoundMatchesException("Проверьте коррекность указанной учетной записи.");
            }

            return null;
        }

        public static void UpdateEntityBatch<T>(T entity, uint rowNumber, List<Structures.ExceptionsStruct> exceptions, Logger logger) where T : class
        {
            logger.Info(string.Format("Обновление сущности {0}", PrintInfo(typeof(T))));

            string message;

            try
            {
                Client.UpdateEntityBatch(entity);

                logger.Info(string.Format("Тип сущности {0} обновлен.", PrintInfo(typeof(T))));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is WebRequestException webEx)
                {
                    message = $"Ошибка на стороне Directum RX. Код ошибки: {webEx.Code}, Причина: {webEx.ReasonPhrase}, Ответ сервиса интеграции: {webEx.Response}";
                    logger.Error(message);
                    exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ExceptionType = ExceptionType.Error, Message = message });
                }

                if (ex.Message.Contains("(Not Found)"))
                    throw new FoundMatchesException("Проверьте коррекность адреса службы интеграции Directum RX.");

                if (ex.Message.Contains("(Unauthorized)"))
                    throw new FoundMatchesException("Проверьте коррекность указанной учетной записи.");
            }
        }

        public static void UpdatePropertiesBatch<T>(int entityId, uint rowNumber, IDictionary<string, object> properties, List<Structures.ExceptionsStruct> exceptions, Logger logger) where T : class
        {
            try
            {
                Client.UpdatePropertiesBatch<T>(entityId, properties);
            }
            catch (Exception ex)
            {
                var propertiesString = string.Join(", ", properties.Select(p => p.Key + ": " + p.Value));
                var message = $"Ошибка обновления полей сущности с ИД: {entityId}. Поля: {propertiesString}. Ошибка: {ex.Message}";
                logger.Error(message);
                exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ExceptionType = ExceptionType.Error, Message = message });
            }

        }

        #endregion

        #region Работа с документами.
        /// <summary>
        /// Импорт тела документа.
        /// </summary>
        /// <param name="edoc">Экземпляр документа.</param>
        /// <param name="pathToBody">Путь к телу документа.</param>
        /// <param name="logger">Логировщик.</param>
        /// <returns>Список ошибок.</returns>
        public static IEnumerable<Structures.ExceptionsStruct> ImportBody(IElectronicDocuments edoc, string pathToBody, Logger logger, bool update_body = false)
        {
            var exceptionList = new List<Structures.ExceptionsStruct>();
            logger.Info("Импорт тела документа");

            try
            {
                if (!File.Exists(pathToBody))
                {
                    var message = string.Format("Не найден файл по заданому пути: \"{0}\"", pathToBody);
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Warn(message);

                    return exceptionList;
                }

                // GetExtension возвращает расширение в формате ".<расширение>". Убираем точку.
                var extention = Path.GetExtension(pathToBody).Replace(".", "");
                var associatedApplication = GetEntityWithFilter<IAssociatedApplications>(a => a.Extension == extention, exceptionList, logger);

                if (associatedApplication != null)
                {
                    var lastVersion = edoc.LastVersion();
                    if (lastVersion == null || !update_body || lastVersion.AssociatedApplication.Extension != extention)
                    lastVersion = edoc.CreateVersion(edoc.Name, associatedApplication);

                    lastVersion.Body ??= new IBinaryData();

                    lastVersion.Body.Value = File.ReadAllBytes(pathToBody);
                    lastVersion.AssociatedApplication = associatedApplication;

                    bool isFillBody = edoc.FillBody(lastVersion);
                }
                else
                {
                    var message = string.Format("Не обнаружено соответствующее приложение-обработчик для файлов с расширением \"{0}\"", extention);
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Warn(message);

                    return exceptionList;
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("Не удается создать тело документа. Ошибка: \"{0}\"", ex.Message);
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                logger.Warn(message);

                return exceptionList;
            }

            return exceptionList;
        }

        public static void ImportBody(IElectronicDocuments edoc, Attachment attachment, uint rowNumber, List<Structures.ExceptionsStruct> exceptions, Logger logger, bool update_body = false)
        {
            logger.Info("Импорт тела документа");

            try
            {
                var lastVersion = edoc.LastVersion();
                if (lastVersion == null || !update_body || lastVersion.AssociatedApplication.Extension != attachment.Extension)
                    lastVersion = edoc.CreateVersion(edoc.Name, attachment.AssociatedApplication);

                lastVersion.Body ??= new IBinaryData();

                lastVersion.Body.Value = File.ReadAllBytes(attachment.AttachmentPath);
                lastVersion.AssociatedApplication = attachment.AssociatedApplication;

                bool isFillBody = edoc.FillBody(lastVersion);
            }
            catch (Exception ex)
            {
                var message = string.Format("Не удается создать тело документа. Ошибка: \"{0}\"", ex.Message);
                exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ExceptionType = ExceptionType.Error, Message = message });
                logger.Error(message);
            }
        }

        public static void ImportBodyBatch<T>(T eDoc, int entityId, Attachment attachment, uint rowNumber, List<Structures.ExceptionsStruct> exceptions, Logger logger, bool update_body = false) where T : IElectronicDocuments
        {
            logger.Info($"Строка {rowNumber}. Импорт тела документа.");

            string message = string.Empty;

            try
            {
                var edocId = entityId;
                var versionId = entityId - 1;
                int verNumber = 1;

                if (eDoc != null)
                {
                    edocId = eDoc.Id;

                    var lastVersion = eDoc.LastVersion();
                    if (lastVersion != null)
                    {
                        if (!update_body || lastVersion.AssociatedApplication.Extension != attachment.Extension)
                            verNumber = ++lastVersion.Number;
                        else
                            versionId = eDoc.LastVersion().Id;
                    }
                }

                if (versionId < 0)
                {
                    try
                    {
                        Client.CreateVersion<T>(edocId, versionId, verNumber, attachment);
                    }
                    catch (Exception ex)
                    {
                        message = string.Format("Строка {0}. Не удается создать версию документа. Ошибка: \"{1}\"", rowNumber, ex.Message);
                        throw new Exception(message);
                    }
                }

                Client.CreateBody<T>(edocId, versionId, attachment);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(message))
                    message = string.Format("Строка {0}. Не удается создать тело документа. Ошибка: \"{1}\"", rowNumber, ex.Message);

                exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ExceptionType = ExceptionType.Error, Message = message });
                logger.Error(message);
            }
        }

        /// <summary>
        /// Регистрация документа.
        /// </summary>
        /// <param name="edoc">Экземпляр документа.</param>
        /// <param name="documentRegisterId">ИД журнала регистрации.</param>
        /// <param name="regNumber">Рег. №</param>
        /// <param name="regDate">Дата регистрации.</param>
        /// <param name="logger">Логировщик.</param>
        /// <returns>Список ошибок.</returns>
        public static void RegisterDocument(IOfficialDocuments edoc, int documentRegisterId, string regNumber, DateTimeOffset regDate,
            Guid defaultRegistrationRoleGuid, uint rowNumber, List<Structures.ExceptionsStruct> exceptions, Logger logger)
        {
            IRegistrationGroups regGroup = null;

            // TODO Кэшировать.
            var documentRegister = GetEntityWithFilter<IDocumentRegisters>(d => d.Id == documentRegisterId, exceptions, logger);

            if (documentRegister != null && regDate != null && !string.IsNullOrEmpty(regNumber))
            {
                edoc.RegistrationDate = regDate;
                edoc.RegistrationNumber = regNumber;
                edoc.DocumentRegister = documentRegister;
                edoc.RegistrationState = GetRegistrationsState("Registerd");
                regGroup = documentRegister.RegistrationGroup;
            }
            else
            {
                var message = string.Format("Не удалось найти соответствующий реестр с ИД \"{0}\".", documentRegisterId);
                LogException(exceptions, rowNumber, ExceptionType.Warn, message, logger);
            }

            try
            {
                UpdateEntity(edoc, rowNumber, exceptions, logger);
            }
            catch (Exception ex)
            {
                exceptions.Add(new Structures.ExceptionsStruct { ExceptionType = ExceptionType.Warn, Message = ex.Message });
            }
        }

        public static void SaveBatch(Logger logger)
        {
            try
            {
                Client.SaveBatchAsync().Wait();
            }
            catch (Exception ex)
            {
                string message;

                if (ex.InnerException is WebRequestException webEx)
                {
                    message = $"Ошибка на стороне Directum RX. Код ошибки: {webEx.Code}, Причина: {webEx.ReasonPhrase}, Ответ сервиса интеграции: {webEx.Response}";
                    logger.Error(message);
                }

                if (ex.Message.Contains("(Not Found)"))
                    throw new FoundMatchesException("Проверьте коррекность адреса службы интеграции Directum RX.");

                if (ex.Message.Contains("(Unauthorized)"))
                    throw new FoundMatchesException("Проверьте коррекность указанной учетной записи.");

                message = $"Не удалось импортировать сущности. Ошибка: {ex.Message}";
                throw new Exception(message);
            }
        }

        #endregion

        #region Словари с перечислениями.
        /// <summary>
        /// Получение состояние регистрации.
        /// </summary>
        /// <param name="registrationState">Наименование состояния регистрации.</param>
        /// <returns>Состояние регистрации.</returns>
        public static string GetRegistrationsState(string key)
        {
            Dictionary<string, string> RegistrationState = new Dictionary<string, string>
            {
                {"Зарегистрирован", "Registered"},
                {"Зарезервирован", "Reserved"},
                {"Не зарегистрирован", "NotRegistered"},
                {"", null}
            };

            try
            {
                return RegistrationState[key.Trim()];
            }
            catch (KeyNotFoundException ex)
            {
                throw new WellKnownKeyNotFoundException(key, ex.Message, ex.InnerException);
            }
        }

        /// <summary>
        /// Получение ЖЦ документа.
        /// </summary>
        /// <param name="lifeCycleStateName">Наименование ЖЦ документа.</param>
        /// <returns>ЖЦ.</returns>
        public static string GetPropertyLifeCycleState(string key)
        {
            Dictionary<string, string> LifeCycleStates = new Dictionary<string, string>
            {
                {"В разработке", "Draft"},
                {"Действующий", "Active"},
                {"Аннулирован", "Obsolete"},
                {"Расторгнут", "Terminated"},
                {"Исполнен", "Closed"},
                {"", null}
            };

            try
            {
                return LifeCycleStates[key];
            }
            catch (KeyNotFoundException ex)
            {
                throw new WellKnownKeyNotFoundException(key, ex.Message, ex.InnerException);
            }
        }

        /// <summary>
        /// Получение пола.
        /// </summary>
        /// <param name="sexPropertyName">Наименование пола.</param>
        /// <returns>Экземпляр записи "Пол".</returns>
        public static string GetPropertySex(string key)
        {
            Dictionary<string, string> sexProperty = new Dictionary<string, string>
            {
                {"Мужской", "Male"},
                {"Женский", "Female"},
                {"Male", "Male"},
                {"Female", "Female"},
                {"", null}
            };

            try
            {
                return sexProperty[key];
            }
            catch (KeyNotFoundException ex)
            {
                throw new WellKnownKeyNotFoundException(key, ex.Message, ex.InnerException);
            }
        }
        #endregion

        #region Проверка валидации.
        /// <summary>
        /// Проверка введенного ОГРН по количеству символов.
        /// </summary>
        /// <param name="psrn">ОГРН.</param>
        /// <param name="nonresident">Нерезидент.</param>
        /// <returns>Пустая строка, если длина ОГРН в порядке.
        /// Иначе текст ошибки.</returns>
        public static string CheckPsrnLength(string psrn, bool nonresident)
        {
            if (string.IsNullOrWhiteSpace(psrn))
                return string.Empty;

            if (nonresident)
                return string.Empty;

            psrn = psrn.Trim();

            return Regex.IsMatch(psrn, @"(^\d{13}$)|(^\d{15}$)") ? string.Empty : Constants.Resources.IncorrecPsrnLength;
        }

        /// <summary>
        /// Проверка введенного КПП по количеству символов.
        /// </summary>
        /// <param name="trrc">КПП.</param>
        /// <param name="nonresident">Нерезидент.</param>
        /// <returns>Пустая строка, если длина КПП в порядке.
        /// Иначе текст ошибки.</returns>
        public static string CheckTrrcLength(string trrc, bool nonresident)
        {
            if (string.IsNullOrWhiteSpace(trrc))
                return string.Empty;

            if (nonresident)
                return string.Empty;

            trrc = trrc.Trim();
     
            return Regex.IsMatch(trrc, @"(^\d{9}$)") ? string.Empty : Constants.Resources.IncorrecTrrcLength;

        }

        /// <summary>
        /// Проверка введенного кода подразделения по количеству символов.
        /// </summary>
        /// <param name="codeDepartment">Код подразделения.</param>
        /// <returns>Пустая строка, если длина кода подразделения в порядке.
        /// Иначе текст ошибки.</returns>
        public static string CheckCodeDepartmentLength(string codeDepartment)
        {
            if (string.IsNullOrWhiteSpace(codeDepartment))
                return string.Empty;

            codeDepartment = codeDepartment.Trim();

            return codeDepartment.Length <= 10 ? string.Empty : Constants.Resources.IncorrecCodeDepartmentLength;
        }

        /// <summary>
        /// Проверка ИНН на валидность.
        /// </summary>
        /// <param name="tin">Строка с ИНН.</param>
        /// <param name="forCompany">Признак того, что проверяется ИНН для компании.</param>
        /// <returns>Текст ошибки. Пустая строка для верного ИНН.</returns>
        public static string CheckTin(string tin, bool forCompany, bool nonresident)
        {
            if (string.IsNullOrWhiteSpace(tin))
                return string.Empty;

            tin = tin.Trim();

            if (nonresident)
                return string.Empty;
      

            // Проверить содержание ИНН. Должен состоять только из цифр. (Bug 87755)
            if (!Regex.IsMatch(tin, @"^\d*$"))
                return Constants.Resources.NotOnlyDigitsTin;

            // Проверить длину ИНН. Для компаний допустимы ИНН длиной 10 или 12 символов, для персон - только 12.
            if (forCompany && tin.Length != 10 && tin.Length != 12)
            return Constants.Resources.CompanyIncorrectTinLength;

            if (!forCompany && tin.Length != 12)
                return Constants.Resources.PeopleIncorrectTinLength;

            // Проверить контрольную сумму.
            if (!CheckTinSum(tin))
                return Constants.Resources.NotValidTin;


            // Проверить значения первых 2х цифр на нули.
            // 1 и 2 цифры - код субъекта РФ (99 для межрегиональной ФНС для физлиц и ИП или код иностранной организации).
            if (tin.StartsWith("00"))
                return Constants.Resources.NotValidTinRegionCode;

            return string.Empty;
        }

        /// <summary>
        /// Проверка контрольной суммы ИНН. Вызывается из CheckTinSum.
        /// </summary>
        /// <param name="tin">Строка ИНН. Передавать ИНН длиной 10-12 символов.</param>
        /// <param name="coefficients">Массив коэффициентов для умножения.</param>
        /// <returns>True, если контрольная сумма сошлась.</returns>
        private static bool CheckTinSum(string tin, int[] coefficients)
        {
            var sum = 0;
            for (var i = 0; i < coefficients.Count(); i++)
            sum += (int)char.GetNumericValue(tin[i]) * coefficients[i];
            sum = (sum % 11) % 10;
            return sum == (int)char.GetNumericValue(tin[coefficients.Count()]);
        }

        /// <summary>
        /// Проверка контрольной суммы ИНН.
        /// </summary>
        /// <param name="tin">ИНН.</param>
        /// <returns>True, если контрольная сумма сошлась.</returns>
        /// <remarks>Информация по ссылке: http://ru.wikipedia.org/wiki/Идентификационный_номер_налогоплательщика.</remarks>
        private static bool CheckTinSum(string tin)
        {
            var coefficient10 = new int[] { 2, 4, 10, 3, 5, 9, 4, 6, 8 };
            var coefficient11 = new int[] { 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };
            var coefficient12 = new int[] { 3, 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };
            return tin.Length == 10 ? CheckTinSum(tin, coefficient10) : (CheckTinSum(tin, coefficient11) && CheckTinSum(tin, coefficient12));
        }

        /// <summary>
        /// Проверка введенного ОКПО по количеству символов.
        /// </summary>
        /// <param name="psrn">ОКПО.</param>
        /// <returns>Пустая строка, если длина ОКПО в порядке.
        /// Иначе текст ошибки.</returns>
        public static string CheckNceoLength(string nceo, bool nonresident)
        {
            if (string.IsNullOrWhiteSpace(nceo))
            return string.Empty;

            if (nonresident)
            return string.Empty;

            nceo = nceo.Trim();

            return Regex.IsMatch(nceo, @"(^\d{8}$)|(^\d{10}$)") ? string.Empty : Constants.Resources.IncorrecNceoLength;
        }
        #endregion

        public static void LogException(List<Structures.ExceptionsStruct> exceptions, uint rowNumber, ExceptionType exceptionType, string message, Logger logger)
        {
            switch (exceptionType)
            {
                case ExceptionType.Error:
                    exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ExceptionType = ExceptionType.Error, Message = message });
                    logger.Error(message);
                    break;
                case ExceptionType.Warn:
                    exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ExceptionType = ExceptionType.Warn, Message = message });
                    logger.Warn(message);
                    break;
                default:
                    exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ExceptionType = ExceptionType.Info, Message = message });
                    break;
            }
        }

    }
}
