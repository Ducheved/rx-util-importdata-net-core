using System;
using System.Collections.Generic;
using System.Globalization;
using NLog;
using ImportData.IntegrationServicesClient.Models;
using System.IO;
using System.Diagnostics.Contracts;
using ImportData.Entities.Databooks;
using System.Linq;
using ImportData.Dto;
using ImportData.Dto.Edocs;

namespace ImportData
{
    class SupAgreement : Entity
    {
        public int PropertiesCount = 22;
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

            var regNumber = this.Parameters[shift + 0];
            DateTimeOffset regDate = DateTimeOffset.MinValue;
            var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
            var culture = CultureInfo.CreateSpecificCulture("en-GB");

            try
            {
                regDate = ParseDate(this.Parameters[shift + 1], style, culture);
            }
            catch (Exception)
            {
                var message = string.Format("Не удалось обработать дату регистрации \"{0}\".", this.Parameters[shift + 1]);
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                logger.Error(message);

                return exceptionList;
            }

            var regNumberLeadingDocument = this.Parameters[shift + 2];

            var regDateLeadingDocument = DateTimeOffset.MinValue;
            try
            {
                regDateLeadingDocument = ParseDate(this.Parameters[shift + 3], style, culture);
            }
            catch (Exception)
            {
                var message = string.Format("Не удалось обработать дату регистрации ведущего документа \"{0}\".", this.Parameters[shift + 3]);
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
            var documentKind = BusinessLogic.GetEntityWithFilter<IDocumentKinds>(c => c.Name == variableForParameters, exceptionList, logger);

            if (documentKind == null)
            {
                var message = string.Format("Не найден вид документа \"{0}\".", this.Parameters[shift + 5]);
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                logger.Error(message);

                return exceptionList;
            }

            var subject = this.Parameters[shift + 6];

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

            var filePath = this.Parameters[shift + 9];
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            DateTimeOffset validFrom = DateTimeOffset.MinValue;
            try
            {
                validFrom = ParseDate(this.Parameters[shift + 10], style, culture);
            }
            catch (Exception)
            {
                var message = string.Format("Не удалось обработать значение в поле \"Действует с\" \"{0}\".", this.Parameters[shift + 10]);
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                logger.Error(message);

                return exceptionList;
            }

            DateTimeOffset validTill = DateTimeOffset.MinValue;
            try
            {
                validTill = ParseDate(this.Parameters[shift + 11], style, culture);
            }
            catch (Exception)
            {
                var message = string.Format("Не удалось обработать значение в поле \"Действует по\" \"{0}\".", this.Parameters[shift + 11]);
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                logger.Error(message);

                return exceptionList;
            }

            var totalAmount = 0.0;

            if (!string.IsNullOrWhiteSpace(this.Parameters[shift + 12]) && !double.TryParse(this.Parameters[shift + 12].Trim(), style, culture, out totalAmount))
            {
                var message = string.Format("Не удалось обработать значение в поле \"Сумма\" \"{0}\".", this.Parameters[shift + 12]);
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                logger.Error(message);

                return exceptionList;
            }

            variableForParameters = this.Parameters[shift + 13].Trim();
            var currency = BusinessLogic.GetEntityWithFilter<ICurrencies>(c => c.Name == variableForParameters, exceptionList, logger);

            if (!string.IsNullOrEmpty(this.Parameters[shift + 13].Trim()) && currency == null)
            {
                var message = string.Format("Не найдено соответствующее наименование валюты \"{0}\".", this.Parameters[shift + 13]);
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                logger.Error(message);

                return exceptionList;
            }

            var lifeCycleState = BusinessLogic.GetPropertyLifeCycleState(this.Parameters[shift + 14]);

            if (!string.IsNullOrEmpty(this.Parameters[shift + 14].Trim()) && lifeCycleState == null)
            {
                var message = string.Format("Не найдено соответствующее значение состояния \"{0}\".", this.Parameters[shift + 14]);
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                logger.Error(message);
                return exceptionList;
            }

            variableForParameters = this.Parameters[shift + 15].Trim();
            var responsibleEmployee = BusinessLogic.GetEntityWithFilter<IEmployees>(c => c.Name == variableForParameters, exceptionList, logger);

            if (!string.IsNullOrEmpty(this.Parameters[shift + 15].Trim()) && responsibleEmployee == null)
            {
                var message = string.Format("Не найден Ответственный \"{3}\". Доп. соглашение: \"{0} {1} {2}\". ", regNumber, regDate.ToString(), counterparty, this.Parameters[shift + 15].Trim());
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Warn, Message = message });
                logger.Warn(message);
            }

            variableForParameters = this.Parameters[shift + 16].Trim();
            var ourSignatory = BusinessLogic.GetEntityWithFilter<IEmployees>(c => c.Name == variableForParameters, exceptionList, logger);

            if (!string.IsNullOrEmpty(this.Parameters[shift + 16].Trim()) && ourSignatory == null)
            {
                var message = string.Format("Не найден Подписывающий \"{3}\". Доп. соглашение: \"{0} {1} {2}\". ", regNumber, regDate.ToString(), counterparty, this.Parameters[shift + 16].Trim());
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Warn, Message = message });
                logger.Warn(message);
            }

            var note = this.Parameters[shift + 17];

            var documentRegisterIdStr = this.Parameters[shift + 18].Trim();
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

		    var leadDocResearchResult = IOfficialDocuments.GetLeadingDocument(logger, regNumberLeadingDocument, regDateLeadingDocument, counterparty.Id);
		    var leadingDocument = leadDocResearchResult.leadingDocument;
		    if (!string.IsNullOrEmpty(leadDocResearchResult.errorMessage))
		    {
			    var message = leadDocResearchResult.errorMessage;
			    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
			    logger.Error(message);

			    return exceptionList;
		    }

		    var regState = this.Parameters[shift + 19].Trim();

            var caseFileStr = this.Parameters[shift + 20].Trim();
            var caseFile = BusinessLogic.GetEntityWithFilter<ICaseFiles>(x => x.Name == caseFileStr, exceptionList, logger);
            if (!string.IsNullOrEmpty(caseFileStr) && caseFile == null)
            {
                var message = string.Format("Не найдено Дело по наименованию \"{0}\"", caseFileStr);
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Warn, Message = message });
                logger.Error(message);
            }

            var placedToCaseFileDateStr = this.Parameters[shift + 21].Trim();
            DateTimeOffset placedToCaseFileDate = DateTimeOffset.MinValue;
            try
            {
                if (caseFile != null)
                    placedToCaseFileDate = ParseDate(placedToCaseFileDateStr, style, culture);
            }
            catch (Exception)
            {
                var message = string.Format("Не удалось обработать значение поля \"Дата помещения\" \"{0}\".", placedToCaseFileDateStr);
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Warn, Message = message });
                logger.Error(message);
            }

            try
            {
                var isNewSupAgreement = false;
                var regDateBeginningOfDay = BeginningOfDay(regDate.UtcDateTime);
                var supAgreements = BusinessLogic.GetEntitiesWithFilter<ISupAgreements>(x => x.RegistrationNumber == regNumber &&
		            x.RegistrationDate.Value.ToString("d") == regDate.ToString("d") &&
		            x.Counterparty.Id == counterparty.Id &&
                    x.DocumentRegister.Id == documentRegisters.Id, exceptionList, logger, true);

                var supAgreement = (ISupAgreements)IOfficialDocuments.GetDocumentByRegistrationDate(supAgreements, regDate, logger, exceptionList);
                if (supAgreement == null)
                {
                    supAgreement = new ISupAgreements();
                    isNewSupAgreement = true;
                }

                supAgreement.Name = fileNameWithoutExtension;
                supAgreement.Created = DateTimeOffset.UtcNow;
                supAgreement.LeadingDocument = leadingDocument;
                supAgreement.Counterparty = counterparty;
                supAgreement.DocumentKind = documentKind;
                supAgreement.Subject = subject;
                supAgreement.BusinessUnit = businessUnit;
                supAgreement.Department = department;

                if (validFrom != DateTimeOffset.MinValue)
                    supAgreement.ValidFrom = validFrom;
                else
                    supAgreement.ValidFrom = null;
                if (validTill != DateTimeOffset.MinValue)
                    supAgreement.ValidTill = validTill;
                else
                    supAgreement.ValidTill = null;
                supAgreement.TotalAmount = totalAmount;
                supAgreement.Currency = currency;
                supAgreement.LifeCycleState = lifeCycleState;
                supAgreement.ResponsibleEmployee = responsibleEmployee;
                supAgreement.OurSignatory = ourSignatory;
                supAgreement.Note = note;

                supAgreement.DocumentRegister = documentRegisters;
                supAgreement.RegistrationNumber = regNumber;
                if (regDate != DateTimeOffset.MinValue)
                    supAgreement.RegistrationDate = regDate.UtcDateTime;
                else
                    supAgreement.RegistrationDate = null;

                if (!string.IsNullOrEmpty(supAgreement.RegistrationNumber) && supAgreement.DocumentRegister != null)
                    supAgreement.RegistrationState = BusinessLogic.GetRegistrationsState(regState);

                supAgreement.CaseFile = caseFile;
                if (placedToCaseFileDate != DateTimeOffset.MinValue)
                    supAgreement.PlacedToCaseFileDate = placedToCaseFileDate;
                else
                    supAgreement.PlacedToCaseFileDate = null;

                ISupAgreements createdSupAgreement;
                if (isNewSupAgreement)
                {
                    createdSupAgreement = BusinessLogic.CreateEntity(supAgreement, exceptionList, logger);
                    // Дополнительно обновляем свойство Состояние, так как после установки регистрационного номера Состояние сбрасывается в значение "В разработке"
                    createdSupAgreement?.UpdateLifeCycleState(lifeCycleState);
                }
                else
                {
                    // Карточку не обновляем, там ошибка, если у документа есть версия.
                    createdSupAgreement = supAgreement;//BusinessLogic.UpdateEntity(contract, exceptionList, logger);
                }

                if (createdSupAgreement == null)
                    return exceptionList;

                var update_body = ExtraParameters.ContainsKey("update_body") && ExtraParameters["update_body"] == "true";
                if (!string.IsNullOrWhiteSpace(filePath))
                    exceptionList.AddRange(BusinessLogic.ImportBody(createdSupAgreement, filePath, logger, update_body));
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

            // 1. № доп. соглашения.
            var regNumber = Parameters[shift + 0].Trim();

            // 2. Дата доп. соглашения.
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

            // 11. Действует с.
            DateTimeOffset? validFrom = null;
            try
            {
                validFrom = ParseDate(Parameters[shift + 10].Trim(), style, culture);
            }
            catch (Exception)
            {
                errorMessage = string.Format("Не удалось обработать значение в поле \"Действует с\" \"{0}\".", validFrom);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 12. Действует по.
            DateTimeOffset? validTill = null;
            try
            {
                validTill = ParseDate(Parameters[shift + 11].Trim(), style, culture);
            }
            catch (Exception)
            {
                errorMessage = string.Format("Не удалось обработать значение в поле \"Действует по\" \"{0}\".", validTill);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 13. Сумма.
            var totalAmount = 0.0;
            cellValue = Parameters[shift + 12].Trim();
            if (!string.IsNullOrWhiteSpace(cellValue) && !double.TryParse(cellValue, style, culture, out totalAmount))
            {
                errorMessage = string.Format("Не удалось обработать значение в поле \"Сумма\" \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 14. Валюта.
            cellValue = Parameters[shift + 13].Trim();
            var currency = BusinessLogic.GetEntityWithFilter<ICurrencies>(c => c.Name == cellValue, exceptions, logger);

            if (currency == null)
            {
                errorMessage = string.Format("Не найдено соответствующее наименование валюты \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 15. Состояние.
            cellValue = Parameters[shift + 14].Trim();
            var lifeCycleState = BusinessLogic.GetPropertyLifeCycleState(cellValue);

            if (lifeCycleState == null)
            {
                errorMessage = string.Format("Не найдено соответствующее значение состояния \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 16. Ответственный.
            cellValue = Parameters[shift + 15].Trim();
            var responsibleEmployee = BusinessLogic.GetEntityWithFilter<IEmployees>(c => c.Name == cellValue, exceptions, logger);

            if (responsibleEmployee == null)
            {
                errorMessage = string.Format("Не найден Ответственный \"{3}\". Доп. соглашение: \"{0} {1} {2}\". ", regNumber, regDate.ToString(), counterparty, cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // 17. Подписал.
            cellValue = Parameters[shift + 16].Trim();
            var ourSignatory = BusinessLogic.GetEntityWithFilter<IEmployees>(c => c.Name == cellValue, exceptions, logger);

            if (ourSignatory == null)
            {
                errorMessage = string.Format("Не найден Подписывающий \"{3}\". Доп. соглашение: \"{0} {1} {2}\". ", regNumber, regDate.ToString(), counterparty, cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // 18. Примечание.
            var note = Parameters[shift + 17].Trim();

            // 19. Журнал регистрации.
            var documentRegisterIdStr = Parameters[shift + 18].Trim();
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

            // 20. Регистрация.
            var regState = Parameters[shift + 19].Trim();

            // 21. Дело.
            var caseFileStr = Parameters[shift + 20].Trim();
            var caseFile = BusinessLogic.GetEntityWithFilter<ICaseFiles>(x => x.Name == caseFileStr, exceptions, logger);
            if (caseFile == null)
            {
                errorMessage = string.Format("Не найдено Дело по наименованию \"{0}\"", caseFileStr);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // 22. Дата помещения.
            var placedToCaseFileDateStr = Parameters[shift + 21].Trim();
            DateTimeOffset? placedToCaseFileDate = null;
            try
            {
                if (caseFile != null)
                    placedToCaseFileDate = ParseDate(placedToCaseFileDateStr, style, culture);
            }
            catch (Exception)
            {
                errorMessage = string.Format("Не удалось обработать значение поля \"Дата помещения\" \"{0}\".", placedToCaseFileDateStr);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // Допсоглашение.
            var regDateBeginningOfDay = BeginningOfDay(regDate.Value.UtcDateTime);
            var supAgreements = BusinessLogic.GetEntitiesWithFilter<ISupAgreements>(x => x.RegistrationNumber == regNumber &&
                x.RegistrationDate.Value.ToString("d") == regDate.Value.ToString("d") &&
                x.Counterparty.Id == counterparty.Id &&
                x.DocumentRegister.Id == documentRegisters.Id, exceptions, logger, true);

            var supAgreement = (ISupAgreements)IOfficialDocuments.GetDocumentByRegistrationDate(supAgreements, regDate.Value, logger, exceptions);

            // Запись в DTO.
            var dtoSupAgreement = new DtoSupAgreement();
            dtoSupAgreement.RowNumber = rowNumber;
            dtoSupAgreement.Name = fileNameWithoutExtension;
            dtoSupAgreement.Created = DateTimeOffset.UtcNow;
            dtoSupAgreement.LeadingDocument = leadingDocument;
            dtoSupAgreement.Counterparty = counterparty;
            dtoSupAgreement.DocumentKind = documentKind;
            dtoSupAgreement.Subject = subject;
            dtoSupAgreement.BusinessUnit = businessUnit;
            dtoSupAgreement.Department = department;

            if (validFrom.HasValue)
                dtoSupAgreement.ValidFrom = validFrom.Value;

            if (validTill.HasValue)
                dtoSupAgreement.ValidTill = validTill.Value;

            dtoSupAgreement.TotalAmount = totalAmount;
            dtoSupAgreement.Currency = currency;
            dtoSupAgreement.LifeCycleState = lifeCycleState;
            dtoSupAgreement.ResponsibleEmployee = responsibleEmployee;
            dtoSupAgreement.OurSignatory = ourSignatory;
            dtoSupAgreement.Note = note;

            dtoSupAgreement.DocumentRegister = documentRegisters;
            dtoSupAgreement.RegistrationNumber = regNumber;

            if (regDate.HasValue)
                dtoSupAgreement.RegistrationDate = regDate.Value.UtcDateTime;

            if (!string.IsNullOrEmpty(dtoSupAgreement.RegistrationNumber) && dtoSupAgreement.DocumentRegister != null)
                dtoSupAgreement.RegistrationState = BusinessLogic.GetRegistrationsState(regState);

            dtoSupAgreement.CaseFile = caseFile;
            if (placedToCaseFileDate.HasValue)
                dtoSupAgreement.PlacedToCaseFileDate = placedToCaseFileDate.Value;

            dtoSupAgreement.SupAgreement = supAgreement;
            dtoSupAgreement.LastVersion = supAgreement?.LastVersion();

            dtoSupAgreement.Attachment = new Attachment()
            {
                AttachmentPath = filePath,
                AssociatedApplication = associatedApplication,
                Extension = extention,
                Body = File.ReadAllBytes(filePath)
            };

            return dtoSupAgreement;
        }

        public override void SaveToRX(IDtoEntity dtoEntity, List<Structures.ExceptionsStruct> exceptions, Logger logger)
        {
            // ExtraParameters["update_body"] = "true";

            var dtoSupAgreement = dtoEntity as DtoSupAgreement;
            var supAgreement = dtoSupAgreement.SupAgreement;
            var isNewSupAgreement = false;

            if (supAgreement == null)
            {
                supAgreement = new ISupAgreements();
                isNewSupAgreement = true;
            }

            supAgreement.Name = dtoSupAgreement.Name;
            supAgreement.Created = DateTimeOffset.UtcNow;
            supAgreement.Counterparty = dtoSupAgreement.Counterparty;
            supAgreement.DocumentKind = dtoSupAgreement.DocumentKind;
            supAgreement.LeadingDocument = dtoSupAgreement.LeadingDocument;
            supAgreement.Subject = dtoSupAgreement.Subject;
            supAgreement.BusinessUnit = dtoSupAgreement.BusinessUnit;
            supAgreement.Department = dtoSupAgreement.Department;
            supAgreement.ValidFrom = dtoSupAgreement.ValidFrom;
            supAgreement.ValidTill = dtoSupAgreement.ValidTill;
            supAgreement.TotalAmount = dtoSupAgreement.TotalAmount;
            supAgreement.Currency = dtoSupAgreement.Currency;
            supAgreement.ResponsibleEmployee = dtoSupAgreement.ResponsibleEmployee;
            supAgreement.OurSignatory = dtoSupAgreement.OurSignatory;
            supAgreement.Note = dtoSupAgreement.Note;
            supAgreement.DocumentRegister = dtoSupAgreement.DocumentRegister;
            supAgreement.RegistrationDate = dtoSupAgreement.RegistrationDate;
            supAgreement.RegistrationNumber = dtoSupAgreement.RegistrationNumber;
            supAgreement.RegistrationState = dtoSupAgreement.RegistrationState;
            supAgreement.CaseFile = dtoSupAgreement.CaseFile;
            supAgreement.PlacedToCaseFileDate = dtoSupAgreement.PlacedToCaseFileDate;
            supAgreement.LifeCycleState = dtoSupAgreement.LifeCycleState;

            ISupAgreements createdSupAgreement;

            if (isNewSupAgreement)
            {
                createdSupAgreement = BusinessLogic.CreateEntity(supAgreement, dtoEntity.RowNumber, exceptions, logger);
                // Дополнительно обновляем свойство Состояние, так как после установки регистрационного номера Состояние сбрасывается в значение "В разработке"
                createdSupAgreement?.UpdateLifeCycleState(dtoSupAgreement.LifeCycleState);
            }
            else
            {
                // Карточку не обновляем, там ошибка, если у документа есть версия.
                createdSupAgreement = supAgreement;//BusinessLogic.UpdateEntity(contract, exceptionList, logger);
            }

            var update_body = ExtraParameters.ContainsKey("update_body") && ExtraParameters["update_body"] == "true";

            if (!string.IsNullOrWhiteSpace(dtoSupAgreement.Attachment.AttachmentPath))
                BusinessLogic.ImportBody(createdSupAgreement, dtoSupAgreement.Attachment, dtoEntity.RowNumber, exceptions, logger, update_body);
        }

        public override void SaveToRXBatch(IDtoEntity dtoEntity, List<Structures.ExceptionsStruct> exceptions, Logger logger)
        {
            // ExtraParameters["update_body"] = "true";

            int entityId = -1;

            var dtoSupAgreement = dtoEntity as DtoSupAgreement;
            var supAgreement = dtoSupAgreement.SupAgreement;
            var isNewSupAgreement = false;

            if (supAgreement == null)
            {
                supAgreement = new ISupAgreements() { Id = entityId };
                isNewSupAgreement = true;
            }

            supAgreement.Name = dtoSupAgreement.Name;
            supAgreement.Created = DateTimeOffset.UtcNow;
            supAgreement.Counterparty = dtoSupAgreement.Counterparty;
            supAgreement.DocumentKind = dtoSupAgreement.DocumentKind;
            supAgreement.LeadingDocument = dtoSupAgreement.LeadingDocument;
            supAgreement.Subject = dtoSupAgreement.Subject;
            supAgreement.BusinessUnit = dtoSupAgreement.BusinessUnit;
            supAgreement.Department = dtoSupAgreement.Department;
            supAgreement.ValidFrom = dtoSupAgreement.ValidFrom;
            supAgreement.ValidTill = dtoSupAgreement.ValidTill;
            supAgreement.TotalAmount = dtoSupAgreement.TotalAmount;
            supAgreement.Currency = dtoSupAgreement.Currency;
            supAgreement.ResponsibleEmployee = dtoSupAgreement.ResponsibleEmployee;
            supAgreement.OurSignatory = dtoSupAgreement.OurSignatory;
            supAgreement.Note = dtoSupAgreement.Note;
            supAgreement.DocumentRegister = dtoSupAgreement.DocumentRegister;
            supAgreement.RegistrationDate = dtoSupAgreement.RegistrationDate;
            supAgreement.RegistrationNumber = dtoSupAgreement.RegistrationNumber;
            supAgreement.RegistrationState = dtoSupAgreement.RegistrationState;
            supAgreement.CaseFile = dtoSupAgreement.CaseFile;
            supAgreement.PlacedToCaseFileDate = dtoSupAgreement.PlacedToCaseFileDate;
            supAgreement.LifeCycleState = dtoSupAgreement.LifeCycleState;

            if (isNewSupAgreement)
            {
                BusinessLogic.CreateEntityBatch(supAgreement, dtoEntity.RowNumber, exceptions, logger);
                //Client.CreateEntityBatch<IContracts>(properties);

                // Дополнительно обновляем свойство Состояние, так как после установки регистрационного номера Состояние сбрасывается в значение "В разработке"
                BusinessLogic.UpdatePropertiesBatch<ISupAgreements>(entityId, dtoEntity.RowNumber, new Dictionary<string, object>() { { nameof(dtoSupAgreement.LifeCycleState), dtoSupAgreement.LifeCycleState } }, exceptions, logger);

                supAgreement = null;
            }
            else
            {
                // Карточку не обновляем, там ошибка, если у документа есть версия.
                //BusinessLogic.UpdateEntityBatch(contract, exceptions, logger);
            }

            var update_body = ExtraParameters.ContainsKey("update_body") && ExtraParameters["update_body"] == "true";

            if (!string.IsNullOrWhiteSpace(dtoSupAgreement.Attachment.AttachmentPath))
                BusinessLogic.ImportBodyBatch(supAgreement, entityId, dtoSupAgreement.Attachment, dtoSupAgreement.RowNumber, exceptions, logger, update_body);
        }


    }
}
