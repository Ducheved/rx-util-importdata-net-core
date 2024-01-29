using System;
using System.Collections.Generic;
using System.Globalization;
using NLog;
using ImportData.IntegrationServicesClient.Models;
using System.IO;
using DocumentFormat.OpenXml.Drawing.Charts;
using ImportData.Entities.Databooks;
using System.Linq;
using ImportData.Dto.Edocs;
using ImportData.Dto;
using Simple.OData.Client;
using ImportData.IntegrationServicesClient;
using System.Threading.Tasks;

namespace ImportData
{
    public class Contract : Entity
    {
        public int PropertiesCount = 21;
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
        /// <returns>Список ошибок.</returns>
        public override IEnumerable<Structures.ExceptionsStruct> SaveToRX(Logger logger, bool supplementEntity, string ignoreDuplicates, int shift = 0)
        {
          var exceptionList = new List<Structures.ExceptionsStruct>();
          var variableForParameters = Parameters[shift + 0].Trim();

          var regNumber = Parameters[shift + 0];
          var regDate = DateTimeOffset.MinValue;
          var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
          var culture = CultureInfo.CreateSpecificCulture("en-GB");
          try
          {
            regDate = ParseDate(Parameters[shift + 1], style, culture);
          }
          catch (Exception)
          {
            var message = string.Format("Не удалось обработать дату регистрации \"{0}\".", Parameters[shift + 1]);
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
            logger.Error(message);

            return exceptionList;
          }

          variableForParameters = Parameters[shift + 2].Trim();
          var counterparty = BusinessLogic.GetEntityWithFilter<ICounterparties>(c => c.Name == variableForParameters, exceptionList, logger);

          if (counterparty == null)
          {
            var message = string.Format("Не найден контрагент \"{0}\".", Parameters[shift + 2]);
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
            logger.Error(message);

            return exceptionList;
          }

          variableForParameters = Parameters[shift + 3].Trim();
          var documentKind = BusinessLogic.GetEntityWithFilter<IDocumentKinds>(d => d.Name == variableForParameters, exceptionList, logger);

          if (documentKind == null)
          {
            var message = string.Format("Не найден вид документа \"{0}\".", Parameters[shift + 3]);
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
            logger.Error(message);

            return exceptionList;
          }

          variableForParameters = Parameters[shift + 4].Trim();
          var contractCategory = BusinessLogic.GetEntityWithFilter<IContractCategories>(c => c.Name == variableForParameters, exceptionList, logger);

          if (!string.IsNullOrEmpty(Parameters[shift + 4].ToString()))
          {
            if (contractCategory == null)
            {
              var message = string.Format("Не найдена категория договора \"{0}\".", Parameters[shift + 4]);
              exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
              logger.Error(message);

              return exceptionList;
            }
          }

          var subject = Parameters[shift + 5];

          variableForParameters = Parameters[shift + 6].Trim();
          var businessUnit = BusinessLogic.GetEntityWithFilter<IBusinessUnits>(b => b.Name == variableForParameters, exceptionList, logger);

          if (businessUnit == null)
          {
            var message = string.Format("Не найдена НОР \"{0}\".", Parameters[shift + 6]);
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
            logger.Error(message);

            return exceptionList;
          }

          variableForParameters = Parameters[shift + 7].Trim();
          IDepartments department = null;
          if (businessUnit != null)
            department = BusinessLogic.GetEntityWithFilter<IDepartments>(d => d.Name == variableForParameters &&
            (d.BusinessUnit == null || d.BusinessUnit.Id == businessUnit.Id), exceptionList, logger, true);
          else
            department = BusinessLogic.GetEntityWithFilter<IDepartments>(d => d.Name == variableForParameters, exceptionList, logger);

          if (department == null)
          {
            var message = string.Format("Не найдено подразделение \"{0}\".", Parameters[shift + 7]);
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
            logger.Error(message);

            return exceptionList;
          }

          var filePath = Parameters[shift + 8];
          var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

          DateTimeOffset validFrom = DateTimeOffset.MinValue;
          try
          {
            validFrom = ParseDate(Parameters[shift + 9], style, culture);
          }
          catch (Exception)
          {
            var message = string.Format("Не удалось обработать значение в поле \"Действует с\" \"{0}\".", Parameters[shift + 9]);
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
            logger.Error(message);

            return exceptionList;
          }

          DateTimeOffset validTill = DateTimeOffset.MinValue;
          try
          {
            validTill = ParseDate(Parameters[shift + 10], style, culture);
          }
          catch (Exception)
          {
            var message = string.Format("Не удалось обработать значение в поле \"Действует по\" \"{0}\".", Parameters[shift + 10]);
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
            logger.Error(message);

            return exceptionList;
          }

          var totalAmount = 0.0;

          if (!string.IsNullOrWhiteSpace(Parameters[shift + 11]) && !double.TryParse(Parameters[shift + 11].Trim(), style, culture, out totalAmount))
          {
            var message = string.Format("Не удалось обработать значение в поле \"Сумма\" \"{0}\".", Parameters[shift + 11]);
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
            logger.Error(message);

            return exceptionList;
          }

          variableForParameters = Parameters[shift + 12].Trim();
          var currency = BusinessLogic.GetEntityWithFilter<ICurrencies>(c => c.Name == variableForParameters, exceptionList, logger);

          if (!string.IsNullOrEmpty(Parameters[shift + 12].Trim()) && currency == null)
          {
            var message = string.Format("Не найдено соответствующее наименование валюты \"{0}\".", Parameters[shift + 12]);
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
            logger.Error(message);

            return exceptionList;
          }

          var lifeCycleState = BusinessLogic.GetPropertyLifeCycleState(Parameters[shift + 13]);

          if (!string.IsNullOrEmpty(Parameters[shift + 13].Trim()) && lifeCycleState == null)
          {
            var message = string.Format("Не найдено соответствующее значение состояния \"{0}\".", Parameters[shift + 13]);
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
            logger.Error(message);

            return exceptionList;
          }

          variableForParameters = Parameters[shift + 14].Trim();
          var responsibleEmployee = BusinessLogic.GetEntityWithFilter<IEmployees>(e => e.Name == variableForParameters, exceptionList, logger);

          if (!string.IsNullOrEmpty(Parameters[shift + 14].Trim()) && responsibleEmployee == null)
          {
            var message = string.Format("Не найден Ответственный \"{3}\". Договор: \"{0} {1} {2}\". ", regNumber, regDate.ToString(), counterparty, Parameters[shift + 14].Trim());
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Warn, Message = message });
            logger.Warn(message);
          }

          variableForParameters = Parameters[shift + 15].Trim();
          var ourSignatory = BusinessLogic.GetEntityWithFilter<IEmployees>(e => e.Name == variableForParameters, exceptionList, logger);

          if (!string.IsNullOrEmpty(Parameters[shift + 15].Trim()) && ourSignatory == null)
          {
            var message = string.Format("Не найден Подписывающий \"{3}\". Договор: \"{0} {1} {2}\". ", regNumber, regDate.ToString(), counterparty, Parameters[shift + 15].Trim());
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Warn, Message = message });
            logger.Warn(message);
          }

          var note = Parameters[shift + 16];

          var documentRegisterIdStr = Parameters[shift + 17].Trim();
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
          var regState = Parameters[shift + 18].Trim();

          var caseFileStr = Parameters[shift + 19].Trim();
          var caseFile = BusinessLogic.GetEntityWithFilter<ICaseFiles>(x => x.Name == caseFileStr, exceptionList, logger);
          if (!string.IsNullOrEmpty(caseFileStr) && caseFile == null)
          {
            var message = string.Format("Не найдено Дело по наименованию \"{0}\"", caseFileStr);
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Warn, Message = message });
            logger.Error(message);
          }

          var placedToCaseFileDateStr = Parameters[shift + 20].Trim();
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
            var isNewContract = false;
            var contracts = BusinessLogic.GetEntitiesWithFilter<IContracts>(x => x.RegistrationNumber != null &&
                x.RegistrationNumber == regNumber &&
                x.RegistrationDate.Value.ToString("d") == regDate.ToString("d") &&
                x.Counterparty.Id == counterparty.Id &&
                x.DocumentRegister.Id == documentRegisters.Id, exceptionList, logger, true);

            var contract = (IContracts)IOfficialDocuments.GetDocumentByRegistrationDate(contracts, regDate, logger, exceptionList);
            if (contract == null)
            {
              contract = new IContracts();
              isNewContract = true;
            }

            // Обязательные поля.
            contract.Name = fileNameWithoutExtension;
            contract.Created = DateTimeOffset.UtcNow;
            contract.Counterparty = counterparty;
            contract.DocumentKind = documentKind;
            contract.DocumentGroup = contractCategory;
            contract.Subject = subject;
            contract.BusinessUnit = businessUnit;
            contract.Department = department;
            if (validFrom != DateTimeOffset.MinValue)
              contract.ValidFrom = validFrom;
            else
              contract.ValidFrom = null;
            if (validTill != DateTimeOffset.MinValue)
              contract.ValidTill = validTill;
            else
              contract.ValidTill = null;
            contract.TotalAmount = totalAmount;
            contract.Currency = currency;
            contract.ResponsibleEmployee = responsibleEmployee;
            contract.OurSignatory = ourSignatory;
            contract.Note = note;

            contract.DocumentRegister = documentRegisters;
            if (regDate != DateTimeOffset.MinValue)
              contract.RegistrationDate = regDate.UtcDateTime;
            else
              contract.RegistrationDate = null;
            contract.RegistrationNumber = regNumber;
            if (!string.IsNullOrEmpty(contract.RegistrationNumber) && contract.DocumentRegister != null)
              contract.RegistrationState = BusinessLogic.GetRegistrationsState(regState);

            contract.CaseFile = caseFile;
            if (placedToCaseFileDate != DateTimeOffset.MinValue)
              contract.PlacedToCaseFileDate = placedToCaseFileDate;
            else
              contract.PlacedToCaseFileDate = null;

            IContracts createdContract;
            if (isNewContract)
            {
              createdContract = BusinessLogic.CreateEntity(contract, exceptionList, logger);
              // Дополнительно обновляем свойство Состояние, так как после установки регистрационного номера Состояние сбрасывается в значение "В разработке"
              createdContract?.UpdateLifeCycleState(lifeCycleState);
            }
            else
            {
              // Карточку не обновляем, там ошибка, если у документа есть версия.
              createdContract = contract;//BusinessLogic.UpdateEntity(contract, exceptionList, logger);
            }

            if (createdContract == null)
              return exceptionList;

            var update_body = ExtraParameters.ContainsKey("update_body") && ExtraParameters["update_body"] == "true";
            if (!string.IsNullOrWhiteSpace(filePath))
              exceptionList.AddRange(BusinessLogic.ImportBody(createdContract, filePath, logger, update_body));
          }
          catch (Exception ex)
          {
            exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = ex.Message });
            logger.Error(ex, ex.Message);
            return exceptionList;
          }

          return exceptionList;
        }

        public void LogException(List<Structures.ExceptionsStruct> exceptions, uint rowNumber, ExceptionType exceptionType, string message, Logger logger)
        {
            switch (exceptionType)
            {
                case ExceptionType.Error:
                    exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Error(message);
                    break;
                case ExceptionType.Warn:
                    exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ErrorType = Constants.ErrorTypes.Warn, Message = message });
                    logger.Error(message);
                    break;
                default:
                    exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ErrorType = "Info", Message = message });
                    break;
            }
        }

        public override IDtoEntity Validate(List<Structures.ExceptionsStruct> exceptions, uint rowNumber, Logger logger, int shift = 0)
        {
            string cellValue;
            var errorMessage = string.Empty;
            var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;

            // № договора.
            var regNumber = Parameters[shift + 0];

            // Дата регистрации.
            DateTimeOffset? regDate = null;
            cellValue = Parameters[shift + 1].Trim();
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            try
            {
                regDate = ParseDate(cellValue, style, culture);
            }
            catch (Exception)
            {
                errorMessage = string.Format("Не удалось обработать дату регистрации \"{0}\".", cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // Наша организация.
            cellValue = Parameters[shift + 2].Trim();
            var counterparty = BusinessLogic.GetEntityWithFilter<ICounterparties>(c => c.Name == cellValue, exceptions, logger);
            if (counterparty == null)
            {
                errorMessage = string.Format("Не найден контрагент \"{0}\".", cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // Вид документа.
            cellValue = Parameters[shift + 3].Trim();
            var documentKind = BusinessLogic.GetEntityWithFilter<IDocumentKinds>(d => d.Name == cellValue, exceptions, logger);
            if (documentKind == null)
            {
                errorMessage = string.Format("Не найден вид документа \"{0}\".", cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // Категория.
            cellValue = Parameters[shift + 4].Trim();
            var contractCategory = BusinessLogic.GetEntityWithFilter<IContractCategories>(c => c.Name == cellValue, exceptions, logger);
            if (!string.IsNullOrEmpty(cellValue) && contractCategory == null)
            {
                errorMessage = string.Format("Не найдена категория договора \"{0}\".", cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // Содержание.
            var subject = Parameters[shift + 5].Trim();

            // НОР.
            cellValue = Parameters[shift + 6].Trim();
            var businessUnit = BusinessLogic.GetEntityWithFilter<IBusinessUnits>(b => b.Name == cellValue, exceptions, logger);
            if (businessUnit == null)
            {
                errorMessage = string.Format("Не найдена НОР \"{0}\".", cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // Подразделение.
            cellValue = Parameters[shift + 7].Trim();
            IDepartments department = null;
            if (businessUnit != null)
                department = BusinessLogic.GetEntityWithFilter<IDepartments>(d => d.Name == cellValue &&
                (d.BusinessUnit == null || d.BusinessUnit.Id == businessUnit.Id), exceptions, logger, true);
            else
                department = BusinessLogic.GetEntityWithFilter<IDepartments>(d => d.Name == cellValue, exceptions, logger);

            if (department == null)
            {
                errorMessage = string.Format("Не найдено подразделение \"{0}\".", cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // Файл.
            var filePath = Parameters[shift + 8].Trim();
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            // Действует с.
            cellValue = Parameters[shift + 9].Trim();
            DateTimeOffset? validFrom = null;
            try
            {
                validFrom = ParseDate(cellValue, style, culture);
            }
            catch (Exception)
            {
                errorMessage = string.Format("Не удалось обработать значение в поле \"Действует с\" \"{0}\".", cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // Действует по.
            cellValue = Parameters[shift + 10].Trim();
            DateTimeOffset? validTill = null;
            try
            {
                validTill = ParseDate(cellValue, style, culture);
            }
            catch (Exception)
            {             
                errorMessage = string.Format("Не удалось обработать значение в поле \"Действует по\" \"{0}\".", cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // Сумма.
            cellValue = Parameters[shift + 11].Trim();
            var totalAmount = 0.0;

            if (!string.IsNullOrWhiteSpace(cellValue) && !double.TryParse(cellValue, style, culture, out totalAmount))
            {
                errorMessage = string.Format("Не удалось обработать значение в поле \"Сумма\" \"{0}\".", cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // Валюта.
            cellValue = Parameters[shift + 12].Trim();
            var currency = BusinessLogic.GetEntityWithFilter<ICurrencies>(c => c.Name == cellValue, exceptions, logger);

            if (!string.IsNullOrEmpty(cellValue) && currency == null)
            {
                errorMessage = string.Format("Не найдено соответствующее наименование валюты \"{0}\".", cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // ЖЦ.
            cellValue = Parameters[shift + 13];
            var lifeCycleState = BusinessLogic.GetPropertyLifeCycleState(cellValue);

            if (!string.IsNullOrEmpty(cellValue) && lifeCycleState == null)
            {
                errorMessage = string.Format("Не найдено соответствующее значение состояния \"{0}\".", cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // Ответственный.
            cellValue = Parameters[shift + 14].Trim();
            var responsibleEmployee = BusinessLogic.GetEntityWithFilter<IEmployees>(e => e.Name == cellValue, exceptions, logger);

            if (!string.IsNullOrEmpty(cellValue) && responsibleEmployee == null)
            {
                errorMessage = string.Format("Не найден Ответственный \"{3}\". Договор: \"{0} {1} {2}\". ", regNumber, regDate.ToString(), counterparty, cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // Подписывающий.
            cellValue = Parameters[shift + 15].Trim();
            var ourSignatory = BusinessLogic.GetEntityWithFilter<IEmployees>(e => e.Name == cellValue, exceptions, logger);

            if (!string.IsNullOrEmpty(cellValue) && ourSignatory == null)
            {
                errorMessage = string.Format("Не найден Подписывающий \"{3}\". Договор: \"{0} {1} {2}\". ", regNumber, regDate.ToString(), counterparty, cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // Содержание.
            var note = Parameters[shift + 16];

            // Журнал оегистрации.
            var documentRegisterIdStr = Parameters[shift + 17].Trim();
            if (!int.TryParse(documentRegisterIdStr, out var documentRegisterId))
                if (ExtraParameters.ContainsKey("doc_register_id"))
                    int.TryParse(ExtraParameters["doc_register_id"], out documentRegisterId);

            var documentRegisters = documentRegisterId != 0 ? BusinessLogic.GetEntityWithFilter<IDocumentRegisters>(r => r.Id == documentRegisterId, exceptions, logger) : null;

            if (documentRegisters == null)
            {
                errorMessage = string.Format("Не найден журнал регистрации по ИД \"{0}\"", documentRegisterIdStr);
                LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // Состояние регистрации.
            var regState = Parameters[shift + 18].Trim();

            // Дело.
            var caseFileStr = Parameters[shift + 19].Trim();
            var caseFile = BusinessLogic.GetEntityWithFilter<ICaseFiles>(x => x.Name == caseFileStr, exceptions, logger);
            if (!string.IsNullOrEmpty(caseFileStr) && caseFile == null)
            {
                errorMessage = string.Format("Не найдено Дело по наименованию \"{0}\"", caseFileStr);
                LogException(exceptions, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // Дата помещения в дело.
            cellValue = Parameters[shift + 20].Trim();
            DateTimeOffset? placedToCaseFileDate = null;
            try
            {
                if (caseFile != null)
                    placedToCaseFileDate = ParseDate(cellValue, style, culture);
            }
            catch (Exception)
            {
                errorMessage = string.Format("Не удалось обработать значение поля \"Дата помещения\" \"{0}\".", cellValue);
                LogException(exceptions, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            if (!exceptions.Any(e => e.RowNumber == rowNumber))
                LogException(exceptions, rowNumber, ExceptionType.Info, string.Empty, logger);

            var contracts = BusinessLogic.GetEntitiesWithFilter<IContracts>(x => x.RegistrationNumber == regNumber &&
                x.RegistrationDate.Value.ToString("d") == regDate.Value.ToString("d") &&
                x.Counterparty.Id == counterparty.Id &&
                x.DocumentRegister.Id == documentRegisters.Id, exceptions, logger, true);

            var contract = (IContracts)IOfficialDocuments.GetDocumentByRegistrationDate(contracts, regDate.Value, logger, exceptions);

            var dtoContract = new DtoContract();
            dtoContract.RowNumber = rowNumber;

            // Обязательные поля.
            dtoContract.Name = fileNameWithoutExtension;
            dtoContract.Created = DateTimeOffset.UtcNow;
            dtoContract.Counterparty = counterparty;
            dtoContract.DocumentKind = documentKind;
            dtoContract.DocumentGroup = contractCategory;
            dtoContract.Subject = subject;
            dtoContract.BusinessUnit = businessUnit;
            dtoContract.Department = department;

            if (validFrom.HasValue)
                dtoContract.ValidFrom = validFrom.Value;

            if (validTill.HasValue)
                dtoContract.ValidTill = validTill.Value;
            
            dtoContract.TotalAmount = totalAmount;
            dtoContract.Currency = currency;
            dtoContract.ResponsibleEmployee = responsibleEmployee;
            dtoContract.OurSignatory = ourSignatory;
            dtoContract.Note = note;

            dtoContract.DocumentRegisters = documentRegisters;

            if (regDate.HasValue)
                dtoContract.RegistrationDate = regDate.Value.UtcDateTime;

            dtoContract.RegistrationNumber = regNumber;
            if (!string.IsNullOrEmpty(dtoContract.RegistrationNumber) && dtoContract.DocumentRegisters != null)
                dtoContract.RegistrationState = BusinessLogic.GetRegistrationsState(regState);

            dtoContract.CaseFile = caseFile;

            if (placedToCaseFileDate.HasValue)
                dtoContract.PlacedToCaseFileDate = placedToCaseFileDate.Value;

            dtoContract.LifeCycleState = lifeCycleState;
            dtoContract.Attachment = filePath;
            dtoContract.Contract = contract;

            return dtoContract;
        }

        public new static void SaveToRX2(List<IDtoEntity> dtoEntities, List<Structures.ExceptionsStruct> exceptions, Logger logger)
        {
            foreach (var dtoEntity in dtoEntities)
            {
                var dtoContract = dtoEntity as DtoContract;

                try
                {
                    var isNewContract = false;
                    var contracts = BusinessLogic.GetEntitiesWithFilter<IContracts>(x => x.RegistrationNumber != null &&
                        x.RegistrationNumber == dtoContract.RegistrationNumber &&
                        x.RegistrationDate.Value.ToString("d") == dtoContract.RegistrationDate.Value.ToString("d") &&
                        x.Counterparty.Id == dtoContract.Counterparty.Id &&
                        x.DocumentRegister.Id == dtoContract.DocumentRegisters.Id, exceptions, logger, true);

                    var contract = (IContracts)IOfficialDocuments.GetDocumentByRegistrationDate(contracts, dtoContract.RegistrationDate.Value, logger, exceptions);
                    if (contract == null)
                    {
                        contract = new IContracts();
                        isNewContract = true;
                    }

                    // Обязательные поля.
                    contract.Name = dtoContract.Name;
                    contract.Created = DateTimeOffset.UtcNow;
                    contract.Counterparty = dtoContract.Counterparty;
                    contract.DocumentKind = dtoContract.DocumentKind;
                    contract.DocumentGroup = dtoContract.DocumentGroup;
                    contract.Subject = dtoContract.Subject;
                    contract.BusinessUnit = dtoContract.BusinessUnit;
                    contract.Department = dtoContract.Department;
                    contract.ValidFrom = dtoContract.ValidFrom;
                    contract.ValidTill = dtoContract.ValidTill;
                    contract.TotalAmount = dtoContract.TotalAmount;
                    contract.Currency = dtoContract.Currency;
                    contract.ResponsibleEmployee = dtoContract.ResponsibleEmployee;
                    contract.OurSignatory = dtoContract.OurSignatory;
                    contract.Note = dtoContract.Note;
                    contract.DocumentRegister = dtoContract.DocumentRegisters;
                    contract.RegistrationDate = dtoContract.RegistrationDate;
                    contract.RegistrationNumber = dtoContract.RegistrationNumber;
                    contract.RegistrationState = dtoContract.RegistrationState;
                    contract.CaseFile = dtoContract.CaseFile;
                    contract.PlacedToCaseFileDate = dtoContract.PlacedToCaseFileDate;

                    IContracts createdContract;
                    if (isNewContract)
                    {
                        createdContract = BusinessLogic.CreateEntity(contract, exceptions, logger);
                        // Дополнительно обновляем свойство Состояние, так как после установки регистрационного номера Состояние сбрасывается в значение "В разработке"
                        createdContract?.UpdateLifeCycleState(dtoContract.LifeCycleState);
                    }
                    else
                    {
                        // Карточку не обновляем, там ошибка, если у документа есть версия.
                        createdContract = contract;//BusinessLogic.UpdateEntity(contract, exceptionList, logger);
                    }

                    if (createdContract == null)
                        return;

                    var update_body = ExtraParameters.ContainsKey("update_body") && ExtraParameters["update_body"] == "true";
                    if (!string.IsNullOrWhiteSpace(dtoContract.Attachment))
                        exceptions.AddRange(BusinessLogic.ImportBody(createdContract, dtoContract.Attachment, logger, update_body));
                }
                catch (Exception ex)
                {
                    exceptions.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = ex.Message });
                    logger.Error(ex, ex.Message);
                }
            }
        }

        //public new static async Task SaveToRX(List<IDtoEntity> dtoEntities, List<Structures.ExceptionsStruct> exceptions, Logger logger)
        public async new static Task SaveToRX(List<IDtoEntity> dtoEntities, List<Structures.ExceptionsStruct> exceptions, Logger logger)
        {
            //var odataClientSettings = new ODataClientSettings(new Uri(Constants.ConfigServices.IntegrationServiceUrlParamName));
            var odataClient = BusinessLogic.InstanceOData();
            var odataBatchRequest = new ODataBatch(odataClient);
            object postRequestsResult;


            //odataBatchRequest += async odataClient => postRequestsResult = await odataClient.For("ISimpleDocuments")
            //    .Set(new { Id = -1, Name = "Пример простого документа" })
            //    .InsertEntryAsync();

            //var associatedApplication = await odataClient.For("IAssociatedApplications")
            //    .Filter("Extension eq 'doc'")
            //    .FindEntriesAsync();

            //var associatedApplicationId = associatedApplication.First()["Id"];

            //odataBatchRequest += async odataClient => await odataClient.For("ISimpleDocuments").Key(-1)
            //.NavigateTo("Versions")
            //.Set(new { Id = -2, Number = 1, AssociatedApplication = new { Id = associatedApplicationId } })
            //.InsertEntryAsync();

            //odataBatchRequest += async odataClient => await odataClient.For("ISimpleDocuments").Key(-1)
            //  .NavigateTo("Versions").Key(-2)
            //  .NavigateTo("Body")
            //  .Set(new { Value = "MTExMTEx" })
            //  .InsertEntryAsync();

            //await odataBatchRequest.ExecuteAsync();

            int id = -1;

            foreach (var dtoEntity in dtoEntities)
            {
                var dtoContract = dtoEntity as DtoContract;

                try
                {
                    var isNewContract = false;
                    var contract = dtoContract.Contract;
                    if (contract == null)
                    {
                        contract = new IContracts(); 
                        isNewContract = true;
                    }

                    // Обязательные поля.
                    //contract.Name = dtoContract.Name;
                    //contract.Created = DateTimeOffset.UtcNow;
                    //contract.Counterparty = dtoContract.Counterparty;
                    //contract.DocumentKind = dtoContract.DocumentKind;
                    //contract.DocumentGroup = dtoContract.DocumentGroup;
                    //contract.Subject = dtoContract.Subject;
                    //contract.BusinessUnit = dtoContract.BusinessUnit;
                    //contract.Department = dtoContract.Department;
                    //contract.ValidFrom = dtoContract.ValidFrom;
                    //contract.ValidTill = dtoContract.ValidTill;
                    //contract.TotalAmount = dtoContract.TotalAmount;
                    //contract.Currency = dtoContract.Currency;
                    //contract.ResponsibleEmployee = dtoContract.ResponsibleEmployee;
                    //contract.OurSignatory = dtoContract.OurSignatory;
                    //contract.Note = dtoContract.Note;
                    //contract.DocumentRegister = dtoContract.DocumentRegisters;
                    //contract.RegistrationDate = dtoContract.RegistrationDate;
                    //contract.RegistrationNumber = dtoContract.RegistrationNumber;
                    //contract.RegistrationState = dtoContract.RegistrationState;
                    //contract.CaseFile = dtoContract.CaseFile;
                    //contract.PlacedToCaseFileDate = dtoContract.PlacedToCaseFileDate;

                    //IContracts createdContract;
                    if (isNewContract)
                    {
                        odataBatchRequest += async client => postRequestsResult = await client.For("IContracts")
                            .Set(new 
                            {
                                Id = id,
                                dtoContract.Name,
                                Created = DateTimeOffset.UtcNow,
                                dtoContract.Counterparty,
                                dtoContract.DocumentKind,
                                dtoContract.DocumentGroup,
                                dtoContract.Subject,
                                dtoContract.BusinessUnit,
                                dtoContract.Department,
                                dtoContract.ValidFrom,
                                dtoContract.ValidTill,
                                dtoContract.TotalAmount,
                                dtoContract.Currency,
                                dtoContract.ResponsibleEmployee,
                                dtoContract.OurSignatory,
                                dtoContract.Note,
                                DocumentRegister = dtoContract.DocumentRegisters,
                                dtoContract.RegistrationDate,
                                dtoContract.RegistrationNumber,
                                dtoContract.RegistrationState,
                                dtoContract.CaseFile,
                                dtoContract.PlacedToCaseFileDate
                            })
                            .InsertEntryAsync();

                        //createdContract = BusinessLogic.CreateEntity(contract, exceptions, logger);
                        // Дополнительно обновляем свойство Состояние, так как после установки регистрационного номера Состояние сбрасывается в значение "В разработке"
                        //createdContract?.UpdateLifeCycleState(dtoContract.LifeCycleState);
                    }
                    else
                    {
                        // Карточку не обновляем, там ошибка, если у документа есть версия.
                        //createdContract = contract;//BusinessLogic.UpdateEntity(contract, exceptionList, logger);
                    }

                    //if (createdContract == null)
                    //    return;


                    var update_body = ExtraParameters.ContainsKey("update_body") && ExtraParameters["update_body"] == "true";
                    if (!string.IsNullOrWhiteSpace(dtoContract.Attachment))
                    {
                        //exceptions.AddRange(BusinessLogic.ImportBody(createdContract, dtoContract.Attachment, logger, update_body));

                        var associatedApplication = await odataClient.For("IAssociatedApplications")
                            .Filter($"Extension eq '{Path.GetExtension(dtoContract.Attachment).Replace(".", "")}'")
                            .FindEntriesAsync();

                        var associatedApplicationId = associatedApplication.First()["Id"];

                        odataBatchRequest += async client => await client.For("IContracts").Key(id)
                        .NavigateTo("Versions")
                        .Set(new { Id = --id, Number = 1, AssociatedApplication = new { Id = associatedApplicationId } })
                        .InsertEntryAsync();

                        
                    }
                        
                }
                catch (Exception ex)
                {
                    exceptions.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = ex.Message });
                    logger.Error(ex, ex.Message);
                }

                id--;
            }

            await odataBatchRequest.ExecuteAsync();

            


        }


    }
}
