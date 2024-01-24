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
            if (exceptionType == ExceptionType.Error)
            {
                exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ErrorType = Constants.ErrorTypes.Error, Message = message });
                logger.Error(message);
                throw new Exception(message);
            }
            else
            {
                exceptions.Add(new Structures.ExceptionsStruct { RowNumber = rowNumber, ErrorType = Constants.ErrorTypes.Warn, Message = message });
                logger.Warn(message);
            }
        }

        public override void Validate(List<Structures.ExceptionsStruct> exceptionList, uint rowNumber, Logger logger, int shift = 0)
        {
            var contract = new DtoContract();
            var errorMessage = string.Empty;

            //var exceptionList = new List<Structures.ExceptionsStruct>();
            string cellValue;// = Parameters[shift + 0].Trim();

            var regNumber = Parameters[shift + 0];
            var regDate = DateTimeOffset.MinValue;
            var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;

            // Дата регистрации.
            cellValue = Parameters[shift + 1].Trim();
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            try
            {
                regDate = ParseDate(cellValue, style, culture);
            }
            catch (Exception)
            {
                errorMessage = string.Format("Не удалось обработать дату регистрации \"{0}\".", cellValue);
                LogException(exceptionList, rowNumber, ExceptionType.Error, errorMessage, logger);
            }

            // Наша организация.
            cellValue = Parameters[shift + 2].Trim();
            var counterparty = BusinessLogic.GetEntityWithFilter<ICounterparties>(c => c.Name == cellValue, exceptionList, logger);
            if (counterparty == null)
            {
                errorMessage = string.Format("Не найден контрагент \"{0}\".", cellValue);
                LogException(exceptionList, rowNumber, ExceptionType.Error, errorMessage, logger);
            }

            // Вид документа.
            cellValue = Parameters[shift + 3].Trim();
            var documentKind = BusinessLogic.GetEntityWithFilter<IDocumentKinds>(d => d.Name == cellValue, exceptionList, logger);
            if (documentKind == null)
            {
                errorMessage = string.Format("Не найден вид документа \"{0}\".", cellValue);
                LogException(exceptionList, rowNumber, ExceptionType.Error, errorMessage, logger);
            }

            // Категория.
            cellValue = Parameters[shift + 4].Trim();
            var contractCategory = BusinessLogic.GetEntityWithFilter<IContractCategories>(c => c.Name == cellValue, exceptionList, logger);
            if (!string.IsNullOrEmpty(cellValue.ToString()))
            {
                if (contractCategory == null)
                {
                    errorMessage = string.Format("Не найдена категория договора \"{0}\".", cellValue);
                    LogException(exceptionList, rowNumber, ExceptionType.Error, errorMessage, logger);
                }
            }

            // Содержание.
            var subject = Parameters[shift + 5].Trim();

            // НОР.
            cellValue = Parameters[shift + 6].Trim();
            var businessUnit = BusinessLogic.GetEntityWithFilter<IBusinessUnits>(b => b.Name == cellValue, exceptionList, logger);
            if (businessUnit == null)
            {
                errorMessage = string.Format("Не найдена НОР \"{0}\".", cellValue);
                LogException(exceptionList, rowNumber, ExceptionType.Error, errorMessage, logger);
            }

            // Подразделение.
            cellValue = Parameters[shift + 7].Trim();
            IDepartments department = null;
            if (businessUnit != null)
                department = BusinessLogic.GetEntityWithFilter<IDepartments>(d => d.Name == cellValue &&
                (d.BusinessUnit == null || d.BusinessUnit.Id == businessUnit.Id), exceptionList, logger, true);
            else
                department = BusinessLogic.GetEntityWithFilter<IDepartments>(d => d.Name == cellValue, exceptionList, logger);

            if (department == null)
            {
                errorMessage = string.Format("Не найдено подразделение \"{0}\".", cellValue);
                LogException(exceptionList, rowNumber, ExceptionType.Error, errorMessage, logger);
            }

            // Файл.
            var filePath = Parameters[shift + 8].Trim();
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            // Действует с.
            cellValue = Parameters[shift + 9].Trim();
            DateTimeOffset validFrom = DateTimeOffset.MinValue;
            try
            {
                validFrom = ParseDate(cellValue, style, culture);
            }
            catch (Exception)
            {
                errorMessage = string.Format("Не удалось обработать значение в поле \"Действует с\" \"{0}\".", cellValue);
                LogException(exceptionList, rowNumber, ExceptionType.Error, errorMessage, logger);
            }

            // Действует по.
            cellValue = Parameters[shift + 10].Trim();
            DateTimeOffset validTill = DateTimeOffset.MinValue;
            try
            {
                validTill = ParseDate(cellValue, style, culture);
            }
            catch (Exception)
            {             
                errorMessage = string.Format("Не удалось обработать значение в поле \"Действует по\" \"{0}\".", cellValue);
                LogException(exceptionList, rowNumber, ExceptionType.Error, errorMessage, logger);
            }

            // Сумма.
            cellValue = Parameters[shift + 11].Trim();
            var totalAmount = 0.0;

            if (!string.IsNullOrWhiteSpace(cellValue) && !double.TryParse(cellValue, style, culture, out totalAmount))
            {
                errorMessage = string.Format("Не удалось обработать значение в поле \"Сумма\" \"{0}\".", cellValue);
                LogException(exceptionList, rowNumber, ExceptionType.Error, errorMessage, logger);
            }

            // Валюта.
            cellValue = Parameters[shift + 12].Trim();
            var currency = BusinessLogic.GetEntityWithFilter<ICurrencies>(c => c.Name == cellValue, exceptionList, logger);

            if (!string.IsNullOrEmpty(cellValue) && currency == null)
            {
                errorMessage = string.Format("Не найдено соответствующее наименование валюты \"{0}\".", cellValue);
                LogException(exceptionList, rowNumber, ExceptionType.Error, errorMessage, logger);
            }

            // ЖЦ.
            cellValue = Parameters[shift + 13];
            var lifeCycleState = BusinessLogic.GetPropertyLifeCycleState(cellValue);

            if (!string.IsNullOrEmpty(cellValue) && lifeCycleState == null)
            {
                errorMessage = string.Format("Не найдено соответствующее значение состояния \"{0}\".", cellValue);
                LogException(exceptionList, rowNumber, ExceptionType.Error, errorMessage, logger);
            }

            // Ответственный.
            cellValue = Parameters[shift + 14].Trim();
            var responsibleEmployee = BusinessLogic.GetEntityWithFilter<IEmployees>(e => e.Name == cellValue, exceptionList, logger);

            if (!string.IsNullOrEmpty(cellValue) && responsibleEmployee == null)
            {
                errorMessage = string.Format("Не найден Ответственный \"{3}\". Договор: \"{0} {1} {2}\". ", regNumber, regDate.ToString(), counterparty, cellValue);
                LogException(exceptionList, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // Подписывающий.
            cellValue = Parameters[shift + 15].Trim();
            var ourSignatory = BusinessLogic.GetEntityWithFilter<IEmployees>(e => e.Name == cellValue, exceptionList, logger);

            if (!string.IsNullOrEmpty(cellValue) && ourSignatory == null)
            {
                errorMessage = string.Format("Не найден Подписывающий \"{3}\". Договор: \"{0} {1} {2}\". ", regNumber, regDate.ToString(), counterparty, cellValue);
                LogException(exceptionList, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // Содержание.
            var note = Parameters[shift + 16];

            // Журнал оегистрации.
            var documentRegisterIdStr = Parameters[shift + 17].Trim();
            if (!int.TryParse(documentRegisterIdStr, out var documentRegisterId))
                if (ExtraParameters.ContainsKey("doc_register_id"))
                    int.TryParse(ExtraParameters["doc_register_id"], out documentRegisterId);

            var documentRegisters = documentRegisterId != 0 ? BusinessLogic.GetEntityWithFilter<IDocumentRegisters>(r => r.Id == documentRegisterId, exceptionList, logger) : null;

            if (documentRegisters == null)
            {
                errorMessage = string.Format("Не найден журнал регистрации по ИД \"{0}\"", documentRegisterIdStr);
                LogException(exceptionList, rowNumber, ExceptionType.Error, errorMessage, logger);
            }

            // Состояние регистрации.
            var regState = Parameters[shift + 18].Trim();

            // Дело.
            var caseFileStr = Parameters[shift + 19].Trim();
            var caseFile = BusinessLogic.GetEntityWithFilter<ICaseFiles>(x => x.Name == caseFileStr, exceptionList, logger);
            if (!string.IsNullOrEmpty(caseFileStr) && caseFile == null)
            {
                errorMessage = string.Format("Не найдено Дело по наименованию \"{0}\"", caseFileStr);
                LogException(exceptionList, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // Дата помещения в дело.
            var placedToCaseFileDateStr = Parameters[shift + 20].Trim();
            DateTimeOffset placedToCaseFileDate = DateTimeOffset.MinValue;
            try
            {
                if (caseFile != null)
                    placedToCaseFileDate = ParseDate(placedToCaseFileDateStr, style, culture);
            }
            catch (Exception)
            {
                errorMessage = string.Format("Не удалось обработать значение поля \"Дата помещения\" \"{0}\".", placedToCaseFileDateStr);
                LogException(exceptionList, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            return exceptionList;
        }

        public static void zxc(DtoContract dtoContract)
        {
            try
            {
                var isNewContract = false;
                var contracts = BusinessLogic.GetEntitiesWithFilter<IContracts>(x => x.RegistrationNumber != null &&
                    x.RegistrationNumber == dtoContract.RegistrationNumber &&
                    x.RegistrationDate.Value.ToString("d") == dtoContract.RegistrationDate.ToString("d") &&
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






    }
}
