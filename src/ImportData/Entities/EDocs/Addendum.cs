using ImportData.Dto;
using ImportData.Dto.Edocs;
using ImportData.IntegrationServicesClient.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ImportData
{
    class Addendum : Entity
    {
        public int PropertiesCount = 16;
        /// <summary>
        /// Получить наименование число запрашиваемых параметров.
        /// </summary>
        /// <returns>Число запрашиваемых параметров.</returns>
        public override int GetPropertiesCount()
        {
            return PropertiesCount;
        }

        /// <summary>
        /// Сохранение сущности в RX.
        /// </summary>
        /// <param name="shift">Сдвиг по горизонтали в XLSX документе. Необходим для обработки документов, составленных из элементов разных сущностей.</param>
        /// <param name="logger">Логировщик.</param>
        /// <returns>Число запрашиваемых параметров.</returns>
        public override IEnumerable<Structures.ExceptionsStruct> SaveToRX_OLD(Logger logger, bool supplementEntity, string ignoreDuplicates, int shift = 0)
        {
            var exceptionList = new List<Structures.ExceptionsStruct>();
            var variableForParameters = this.Parameters[shift + 0].Trim();
            var regNumber = variableForParameters;

            DateTimeOffset regDateLeadingDocument = DateTimeOffset.MinValue;
            DateTimeOffset regDate = DateTimeOffset.MinValue;
            var regNumberLeadingDocument = string.Empty;
            var documentKind = new IDocumentKinds();
            var subject = string.Empty;
            string lifeCycleState;
            var note = string.Empty;
            var leadingDocument = new IOfficialDocuments();
            var filePath = string.Empty;

            try
            {
                try
                {
                    regDate = ParseDate(this.Parameters[shift + 1], NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CreateSpecificCulture("en-GB"));
                }
                catch (Exception)
                {
                    var message = string.Format("Не удалось обработать дату регистрации \"{0}\".", this.Parameters[shift + 1]);
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Warn(message);

					return exceptionList;
				}

                regNumberLeadingDocument = this.Parameters[shift + 2];
                try
                {
                    regDateLeadingDocument = ParseDate(this.Parameters[shift + 3], NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CreateSpecificCulture("en-GB"));
                }
                catch (Exception)
                {
                    var message = string.Format("Не удалось обработать дату ведущего документа \"{0}\".", this.Parameters[shift + 3]);
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Error(message);

                    return exceptionList;
                }

                variableForParameters = this.Parameters[shift + 4].Trim();
                var counterparty = BusinessLogic.GetEntityWithFilter<ICounterparties>(c => c.Name == variableForParameters, exceptionList, logger);

                if (counterparty == null)
                {
                    var message = string.Format("Не найден контрагент \"{0}\".", this.Parameters[shift + 4]);
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Error(message);

                    return exceptionList;
                }

                variableForParameters = this.Parameters[shift + 5].Trim();
                documentKind = BusinessLogic.GetEntityWithFilter<IDocumentKinds>(d => d.Name == variableForParameters, exceptionList, logger);

                if (documentKind == null)
                {
                    var message = string.Format("Не найден вид документа \"{0}\".", this.Parameters[shift + 5]);
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Error(message);

                    return exceptionList;
                }

                subject = this.Parameters[shift + 6];

                variableForParameters = this.Parameters[shift + 7].Trim();
                var businessUnit = BusinessLogic.GetEntityWithFilter<IBusinessUnits>(c => c.Name == variableForParameters, exceptionList, logger);

                if (businessUnit == null)
                {
                    var message = string.Format("Не найдена НОР \"{0}\".", this.Parameters[shift + 7]);
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Error(message);

                    return exceptionList;
                }

                variableForParameters = this.Parameters[shift + 8].Trim();
                IDepartments department = null;
                if (businessUnit != null)
                    department = BusinessLogic.GetEntityWithFilter<IDepartments>(d => d.Name == variableForParameters &&
                    (d.BusinessUnit == null || d.BusinessUnit.Id == businessUnit.Id), exceptionList, logger, true);
                else
                    department = BusinessLogic.GetEntityWithFilter<IDepartments>(d => d.Name == variableForParameters, exceptionList, logger);

                if (department == null)
                {
                    var message = string.Format("Не найдено подразделение \"{0}\".", this.Parameters[shift + 8]);
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Error(message);

                    return exceptionList;
                }

                filePath = this.Parameters[shift + 9];
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

                lifeCycleState = BusinessLogic.GetPropertyLifeCycleState(this.Parameters[shift + 10]);

                if (!string.IsNullOrEmpty(this.Parameters[shift + 10].Trim()) && lifeCycleState == null)
                {
                    var message = string.Format("Не найдено соответствующее значение состояния \"{0}\".", this.Parameters[shift + 10]);
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Error(message);

                    return exceptionList;
                }

                note = this.Parameters[shift + 13].Trim();

                var leadDocResearchResult = IOfficialDocuments.GetLeadingDocument(logger, regNumberLeadingDocument, regDateLeadingDocument, counterparty.Id);
                leadingDocument = leadDocResearchResult.leadingDocument;
                if (!string.IsNullOrEmpty(leadDocResearchResult.errorMessage))
                {
                    var message = leadDocResearchResult.errorMessage;
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Error(message);

                    return exceptionList;
                }

                var documentRegisterIdStr = this.Parameters[shift + 14].Trim();
                if (!int.TryParse(documentRegisterIdStr, out var documentRegisterId))
                    if (ExtraParameters.ContainsKey("doc_register_id"))
                        int.TryParse(ExtraParameters["doc_register_id"], out documentRegisterId);

                var documentRegisters = documentRegisterId != 0 ? BusinessLogic.GetEntityWithFilter<IDocumentRegisters>(r => r.Id == documentRegisterId, exceptionList, logger) : null;

                if (documentRegisters == null)
                {
                    var message = string.Format("Не найден журнал регистрации по ИД \"{0}\"", documentRegisterIdStr);
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Error(message);

                    return exceptionList;
                }

                var regState = this.Parameters[shift + 15].Trim();

                try
                {
                    var isNewAddendum = false;
                    var addendum = BusinessLogic.GetEntityWithFilter<IAddendums>(c => c.LeadingDocument.Id == leadingDocument.Id &&
                        c.DocumentKind.Id == documentKind.Id &&
                        c.Subject == subject &&
                        c.DocumentRegister.Id == documentRegisters.Id, exceptionList, logger);
                    if (addendum == null)
                    {
                        addendum = new IAddendums();
                        isNewAddendum = true;
                    }

                    // Обязательные поля.
                    addendum.Name = fileNameWithoutExtension;
                    addendum.Department = department;

                    addendum.Created = DateTimeOffset.UtcNow;
                    addendum.LeadingDocument = leadingDocument;
                    addendum.DocumentKind = documentKind;
                    addendum.Subject = subject;
                    addendum.LifeCycleState = lifeCycleState;
                    addendum.Note = note;

                    addendum.DocumentRegister = documentRegisters;
                    addendum.RegistrationNumber = regNumber;

                    if (regDate != DateTimeOffset.MinValue)
                        addendum.RegistrationDate = regDate.UtcDateTime;
                    else
                        addendum.RegistrationDate = null;

                    if (!string.IsNullOrEmpty(addendum.RegistrationNumber) && addendum.DocumentRegister != null)
                        addendum.RegistrationState = BusinessLogic.GetRegistrationsState(regState);

                    IAddendums createdAddendum;
                    if (isNewAddendum)
                    {
                        createdAddendum = BusinessLogic.CreateEntity(addendum, exceptionList, logger);
                        createdAddendum?.UpdateLifeCycleState(lifeCycleState);

                    }
                    else
                    {
                        // Карточку не обновляем, там ошибка, если у документа есть версия.
                        createdAddendum = addendum;//BusinessLogic.UpdateEntity(contract, exceptionList, logger);
                    }

                    if (createdAddendum == null)
                        return exceptionList;

                    var update_body = ExtraParameters.ContainsKey("update_body") && ExtraParameters["update_body"] == "true";
                    if (!string.IsNullOrWhiteSpace(filePath))
                        exceptionList.AddRange(BusinessLogic.ImportBody(createdAddendum, filePath, logger, update_body));
                }
                catch (Exception ex)
                {
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Warn, Message = ex.Message });
                    logger.Error(ex.Message);
                }
            }
            catch (Exception ex)
            {
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = ex.Message });

                return exceptionList;
            }

            return exceptionList;
        }

        public override IDtoEntity Validate(List<Structures.ExceptionsStruct> exceptions, uint rowNumber, Logger logger, int shift = 0)
        {
            string cellValue;
            var errorMessage = string.Empty;
            var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
            var culture = CultureInfo.CreateSpecificCulture("en-GB");

            // 1. Рег. №.
            var regNumber = Parameters[shift + 0].Trim();

            // 2. Дата регистрации.
            DateTimeOffset? regDate = null;
            try
            {
                regDate = ParseDate(Parameters[shift + 1].Trim(), style, culture);
            }
            catch (Exception)
            {
                errorMessage = string.Format("Не удалось обработать дату регистрации \"{0}\".", regDate);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 3. № договора.
            var regNumberLeadingDocument = Parameters[shift + 2].Trim();

            // 4. Дата договора.
            DateTimeOffset? regDateLeadingDocument = null;
            try
            {
                regDateLeadingDocument = ParseDate(Parameters[shift + 3].Trim(), style, culture);
            }
            catch (Exception)
            {
                errorMessage = string.Format("Не удалось обработать дату регистрации ведущего документа \"{0}\".", regDateLeadingDocument);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 5. Контрагент.
            cellValue = Parameters[shift + 4].Trim();
            var counterparty = BusinessLogic.GetEntityWithFilter<ICounterparties>(c => c.Name == cellValue, exceptions, logger);

            if (counterparty == null)
            {
                errorMessage = string.Format("Не найден контрагент \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 6. Вид документа.
            cellValue = Parameters[shift + 5].Trim();
            var documentKind = BusinessLogic.GetEntityWithFilter<IDocumentKinds>(c => c.Name == cellValue, exceptions, logger);
            if (documentKind == null)
            {
                errorMessage = string.Format("Не найден вид документа \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 7. Содержание.
            var subject = Parameters[shift + 6].Trim();

            // 8. Наша организация.
            cellValue = Parameters[shift + 7].Trim();
            var businessUnit = BusinessLogic.GetEntityWithFilter<IBusinessUnits>(c => c.Name == cellValue, exceptions, logger);
            if (businessUnit == null)
            {
                errorMessage = string.Format("Не найдена НОР \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 9.Подразделение.
            cellValue = Parameters[shift + 8].Trim();
            IDepartments department = null;
            if (businessUnit != null)
                department = BusinessLogic.GetEntityWithFilter<IDepartments>(d => d.Name == cellValue &&
                (d.BusinessUnit == null || d.BusinessUnit.Id == businessUnit.Id), exceptions, logger, true);
            else
                department = BusinessLogic.GetEntityWithFilter<IDepartments>(d => d.Name == cellValue, exceptions, logger);

            if (department == null)
            {
                errorMessage = string.Format("Не найдено подразделение \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 10. Файл.
            var filePath = Parameters[shift + 9].Trim();
            if (!File.Exists(filePath))
            {
                errorMessage = string.Format("Не найден файл по заданому пути: \"{0}\"", filePath);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            var extention = Path.GetExtension(filePath).Replace(".", "");
            var associatedApplication = BusinessLogic.GetEntityWithFilter<IAssociatedApplications>(a => a.Extension == extention, exceptions, logger);

            if (associatedApplication == null)
            {
                errorMessage = string.Format("Не обнаружено соответствующее приложение-обработчик для файлов с расширением \"{0}\"", extention);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            // 14. Примечание.
            var note = Parameters[shift + 13].Trim();

            // 15. Журнал регистрации.
            var documentRegisterIdStr = Parameters[shift + 14].Trim();
            if (!int.TryParse(documentRegisterIdStr, out var documentRegisterId))
                if (ExtraParameters.ContainsKey("doc_register_id"))
                    int.TryParse(ExtraParameters["doc_register_id"], out documentRegisterId);

            var documentRegisters = documentRegisterId != 0 ? BusinessLogic.GetEntityWithFilter<IDocumentRegisters>(r => r.Id == documentRegisterId, exceptions, logger) : null;

            if (documentRegisters == null)
            {
                errorMessage = string.Format("Не найден журнал регистрации по ИД \"{0}\"", documentRegisterIdStr);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            var leadDocResearchResult = IOfficialDocuments.GetLeadingDocument(logger, regNumberLeadingDocument, regDateLeadingDocument.Value, counterparty.Id);
            var leadingDocument = leadDocResearchResult.leadingDocument;
            if (!string.IsNullOrEmpty(leadDocResearchResult.errorMessage))
            {
                errorMessage = leadDocResearchResult.errorMessage;
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 16. Регистрация.
            var regState = Parameters[shift + 15].Trim();

            // Приложение.
            var regDateBeginningOfDay = BeginningOfDay(regDate.Value.UtcDateTime);
            var addendums = BusinessLogic.GetEntitiesWithFilter<ISupAgreements>(x => x.RegistrationNumber == regNumber &&
                x.RegistrationDate.Value.ToString("d") == regDate.Value.ToString("d") &&
                x.Counterparty.Id == counterparty.Id &&
                x.DocumentRegister.Id == documentRegisters.Id, exceptions, logger, true);

            var addendum = (IAddendums)IOfficialDocuments.GetDocumentByRegistrationDate(addendums, regDate.Value, logger, exceptions);

            // Запись в DTO.
            var dtoAddendum = new DtoAddendum();
            dtoAddendum.RowNumber = rowNumber;
            dtoAddendum.Name = fileNameWithoutExtension;
            dtoAddendum.Created = DateTimeOffset.UtcNow;
            dtoAddendum.LeadingDocument = leadingDocument;
            dtoAddendum.DocumentKind = documentKind;
            dtoAddendum.Subject = subject;
            dtoAddendum.BusinessUnit = businessUnit;
            dtoAddendum.Department = department;
            dtoAddendum.Note = note;
            dtoAddendum.DocumentRegister = documentRegisters;
            dtoAddendum.RegistrationNumber = regNumber;

            if (regDate.HasValue)
                dtoAddendum.RegistrationDate = regDate.Value.UtcDateTime;

            if (!string.IsNullOrEmpty(dtoAddendum.RegistrationNumber) && dtoAddendum.DocumentRegister != null)
                dtoAddendum.RegistrationState = BusinessLogic.GetRegistrationsState(regState);

            dtoAddendum.Addendum = addendum;
            dtoAddendum.LastVersion = addendum?.LastVersion();

            dtoAddendum.Attachment = new Attachment()
            {
                AttachmentPath = filePath,
                AssociatedApplication = associatedApplication,
                Extension = extention,
                Body = File.ReadAllBytes(filePath)
            };

            return dtoAddendum;
        }

        public override void SaveToRX(IDtoEntity dtoEntity, List<Structures.ExceptionsStruct> exceptions, Logger logger)
        {
            var dtoAddendum = dtoEntity as DtoAddendum;
            var addendum = dtoAddendum.Addendum;
            var isNewAddendum = false;

            if (addendum == null)
            {
                addendum = new IAddendums();
                isNewAddendum = true;
            }

            addendum.Name = dtoAddendum.Name;
            addendum.Created = DateTimeOffset.UtcNow;
            addendum.DocumentKind = dtoAddendum.DocumentKind;
            addendum.LeadingDocument = dtoAddendum.LeadingDocument;
            addendum.Subject = dtoAddendum.Subject;
            addendum.BusinessUnit = dtoAddendum.BusinessUnit;
            addendum.Department = dtoAddendum.Department;
            addendum.Note = dtoAddendum.Note;
            addendum.DocumentRegister = dtoAddendum.DocumentRegister;
            addendum.RegistrationDate = dtoAddendum.RegistrationDate;
            addendum.RegistrationNumber = dtoAddendum.RegistrationNumber;
            addendum.RegistrationState = dtoAddendum.RegistrationState;

            IAddendums createdAddendum;

            if (isNewAddendum)
                createdAddendum = BusinessLogic.CreateEntity(addendum, dtoEntity.RowNumber, exceptions, logger);
            else
            {
                // Карточку не обновляем, там ошибка, если у документа есть версия.
                createdAddendum = addendum;//BusinessLogic.UpdateEntity(contract, exceptionList, logger);
            }

            var update_body = ExtraParameters.ContainsKey("update_body") && ExtraParameters["update_body"] == "true";

            if (!string.IsNullOrWhiteSpace(dtoAddendum.Attachment.AttachmentPath))
                BusinessLogic.ImportBody(createdAddendum, dtoAddendum.Attachment, dtoEntity.RowNumber, exceptions, logger, update_body);
        }

        public override void SaveToRXBatch(IDtoEntity dtoEntity, List<Structures.ExceptionsStruct> exceptions, Logger logger)
        {
            int entityId = -1;

            var dtoAddendum = dtoEntity as DtoAddendum;
            var addendum = dtoAddendum.Addendum;
            var isNewAddendum = false;

            if (addendum == null)
            {
                addendum = new IAddendums() { Id = entityId };
                isNewAddendum = true;
            }

            addendum.Name = dtoAddendum.Name;
            addendum.Created = DateTimeOffset.UtcNow;
            addendum.DocumentKind = dtoAddendum.DocumentKind;
            addendum.LeadingDocument = dtoAddendum.LeadingDocument;
            addendum.Subject = dtoAddendum.Subject;
            addendum.BusinessUnit = dtoAddendum.BusinessUnit;
            addendum.Department = dtoAddendum.Department;
            addendum.Note = dtoAddendum.Note;
            addendum.DocumentRegister = dtoAddendum.DocumentRegister;
            addendum.RegistrationDate = dtoAddendum.RegistrationDate;
            addendum.RegistrationNumber = dtoAddendum.RegistrationNumber;
            addendum.RegistrationState = dtoAddendum.RegistrationState;

            if (isNewAddendum)
            {
                BusinessLogic.CreateEntityBatch(addendum, dtoEntity.RowNumber, exceptions, logger);
                //Client.CreateEntityBatch<IContracts>(properties);

                addendum = null;
            }
            else
            {
                // Карточку не обновляем, там ошибка, если у документа есть версия.
                //BusinessLogic.UpdateEntityBatch(contract, exceptions, logger);
            }

            var update_body = ExtraParameters.ContainsKey("update_body") && ExtraParameters["update_body"] == "true";

            if (!string.IsNullOrWhiteSpace(dtoAddendum.Attachment.AttachmentPath))
                BusinessLogic.ImportBodyBatch(addendum, entityId, dtoAddendum.Attachment, dtoAddendum.RowNumber, exceptions, logger, update_body);
        }
    }
}
