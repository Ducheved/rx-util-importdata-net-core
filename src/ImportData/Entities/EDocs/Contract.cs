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
        //public override IEnumerable<Structures.ExceptionsStruct> SaveToRX_OLD(Logger logger, bool supplementEntity, string ignoreDuplicates, int shift = 0)
        public override IEnumerable<Structures.ExceptionsStruct> SaveToRX_OLD(uint rowNumber, bool supplementEntity, string ignoreDuplicates, Logger logger, int shift = 0)
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
              createdContract = BusinessLogic.CreateEntity(contract, rowNumber, exceptionList, logger);
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

        public override IDtoEntity Validate(List<Structures.ExceptionsStruct> exceptions, uint rowNumber, Logger logger, int shift = 0)
        {
            string cellValue;
            var errorMessage = string.Empty;
            var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;

            // 1. № договора.
            var regNumber = Parameters[shift + 0];

            // 2. Дата договора (Дата регистрации).
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
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 3. Контрагент.
            cellValue = Parameters[shift + 2].Trim();
            var counterparty = BusinessLogic.GetEntityWithFilter<ICounterparties>(c => c.Name == cellValue, exceptions, logger);
            if (counterparty == null)
            {
                errorMessage = string.Format("Не найден контрагент \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 4. Вид документа.
            cellValue = Parameters[shift + 3].Trim();
            var documentKind = BusinessLogic.GetEntityWithFilter<IDocumentKinds>(d => d.Name == cellValue, exceptions, logger);
            if (documentKind == null)
            {
                errorMessage = string.Format("Не найден вид документа \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 5. Категория.
            cellValue = Parameters[shift + 4].Trim();
            var contractCategory = BusinessLogic.GetEntityWithFilter<IContractCategories>(c => c.Name == cellValue, exceptions, logger);
            if (!string.IsNullOrEmpty(cellValue) && contractCategory == null)
            {
                errorMessage = string.Format("Не найдена категория договора \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 6. Содержание.
            var subject = Parameters[shift + 5].Trim();

            // 7. Наша организация.
            cellValue = Parameters[shift + 6].Trim();
            var businessUnit = BusinessLogic.GetEntityWithFilter<IBusinessUnits>(b => b.Name == cellValue, exceptions, logger);
            if (businessUnit == null)
            {
                errorMessage = string.Format("Не найдена НОР \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 8. Подразделение.
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
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 9. Файл.
            var filePath = Parameters[shift + 8].Trim();
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

            // 10. Действует с.
            cellValue = Parameters[shift + 9].Trim();
            DateTimeOffset? validFrom = null;
            try
            {
                validFrom = ParseDate(cellValue, style, culture);
            }
            catch (Exception)
            {
                errorMessage = string.Format("Не удалось обработать значение в поле \"Действует с\" \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 11. Действует по.
            cellValue = Parameters[shift + 10].Trim();
            DateTimeOffset? validTill = null;
            try
            {
                validTill = ParseDate(cellValue, style, culture);
            }
            catch (Exception)
            {             
                errorMessage = string.Format("Не удалось обработать значение в поле \"Действует по\" \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 12. Сумма.
            cellValue = Parameters[shift + 11].Trim();
            var totalAmount = 0.0;

            if (!string.IsNullOrWhiteSpace(cellValue) && !double.TryParse(cellValue, style, culture, out totalAmount))
            {
                errorMessage = string.Format("Не удалось обработать значение в поле \"Сумма\" \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 13. Валюта.
            cellValue = Parameters[shift + 12].Trim();
            var currency = BusinessLogic.GetEntityWithFilter<ICurrencies>(c => c.Name == cellValue, exceptions, logger);

            if (currency == null)
            {
                errorMessage = string.Format("Не найдено соответствующее наименование валюты \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 14. Состояние (ЖЦ).
            cellValue = Parameters[shift + 13].Trim();
            var lifeCycleState = BusinessLogic.GetPropertyLifeCycleState(cellValue);

            if (lifeCycleState == null)
            {
                errorMessage = string.Format("Не найдено соответствующее значение состояния \"{0}\".", cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 15. Ответственный.
            cellValue = Parameters[shift + 14].Trim();
            var responsibleEmployee = BusinessLogic.GetEntityWithFilter<IEmployees>(e => e.Name == cellValue, exceptions, logger);

            if (responsibleEmployee == null)
            {
                errorMessage = string.Format("Не найден Ответственный \"{3}\". Договор: \"{0} {1} {2}\". ", regNumber, regDate.ToString(), counterparty, cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // 16. Подписал.
            cellValue = Parameters[shift + 15].Trim();
            var ourSignatory = BusinessLogic.GetEntityWithFilter<IEmployees>(e => e.Name == cellValue, exceptions, logger);

            if (ourSignatory == null)
            {
                errorMessage = string.Format("Не найден Подписывающий \"{3}\". Договор: \"{0} {1} {2}\". ", regNumber, regDate.ToString(), counterparty, cellValue);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // 17. Примечание.
            var note = Parameters[shift + 16];

            // 18. ИД журнала регистрации (Журнал регистрации).
            var documentRegisterIdStr = Parameters[shift + 17].Trim();
            if (!int.TryParse(documentRegisterIdStr, out var documentRegisterId))
                if (ExtraParameters.ContainsKey("doc_register_id"))
                    int.TryParse(ExtraParameters["doc_register_id"], out documentRegisterId);

            var documentRegister = documentRegisterId != 0 ? BusinessLogic.GetEntityWithFilter<IDocumentRegisters>(r => r.Id == documentRegisterId, exceptions, logger) : null;

            if (documentRegister == null)
            {
                errorMessage = string.Format("Не найден журнал регистрации по ИД \"{0}\"", documentRegisterIdStr);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, errorMessage, logger);
                return null;
            }

            // 19. Регистрация (Состояние регистрации).
            var regState = Parameters[shift + 18].Trim();

            // 20. Дело.
            var caseFileStr = Parameters[shift + 19].Trim();
            var caseFile = BusinessLogic.GetEntityWithFilter<ICaseFiles>(x => x.Name == caseFileStr, exceptions, logger);
            if (caseFile == null)
            {
                errorMessage = string.Format("Не найдено Дело по наименованию \"{0}\"", caseFileStr);
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            // 21. Дата помещения в дело.
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
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Warn, errorMessage, logger);
            }

            if (!exceptions.Any(e => e.RowNumber == rowNumber))
                BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Info, string.Empty, logger);

            // Договор.
            var contracts = BusinessLogic.GetEntitiesWithFilter<IContracts>(x => x.RegistrationNumber == regNumber &&
                x.RegistrationDate.Value.ToString("d") == regDate.Value.ToString("d") &&
                x.Counterparty.Id == counterparty.Id &&
                x.DocumentRegister.Id == documentRegister.Id, exceptions, logger, true);

            var contract = (IContracts)IOfficialDocuments.GetDocumentByRegistrationDate(contracts, regDate.Value, rowNumber, exceptions, logger);

            // Запись в DTO.
            var dtoContract = new DtoContract();
            dtoContract.RowNumber = rowNumber;
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

            dtoContract.DocumentRegister = documentRegister;

            if (regDate.HasValue)
                dtoContract.RegistrationDate = regDate.Value.UtcDateTime;

            dtoContract.RegistrationNumber = regNumber;
            if (!string.IsNullOrEmpty(dtoContract.RegistrationNumber) && dtoContract.DocumentRegister != null)
                dtoContract.RegistrationState = BusinessLogic.GetRegistrationsState(regState);

            dtoContract.CaseFile = caseFile;

            if (placedToCaseFileDate.HasValue)
                dtoContract.PlacedToCaseFileDate = placedToCaseFileDate.Value;

            dtoContract.LifeCycleState = lifeCycleState;
            dtoContract.Contract = contract;
            dtoContract.LastVersion = contract?.LastVersion();

            dtoContract.Attachment = new Attachment()
            {
                AttachmentPath = filePath,
                AssociatedApplication = associatedApplication,
                Extension = extention,
                Body = File.ReadAllBytes(filePath)
            };

            return dtoContract;
        }

        public override void SaveToRX(IDtoEntity dtoEntity, List<Structures.ExceptionsStruct> exceptions, Logger logger)
        {
            //ExtraParameters["update_body"] = "true";

            var dtoContract = dtoEntity as DtoContract;
            var contract = dtoContract.Contract;
            var isNewContract = false;

            if (contract == null)
            {
                contract = new IContracts();
                isNewContract = true;
            }

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
            contract.DocumentRegister = dtoContract.DocumentRegister;
            contract.RegistrationDate = dtoContract.RegistrationDate;
            contract.RegistrationNumber = dtoContract.RegistrationNumber;
            contract.RegistrationState = dtoContract.RegistrationState;
            contract.CaseFile = dtoContract.CaseFile;
            contract.PlacedToCaseFileDate = dtoContract.PlacedToCaseFileDate;

            IContracts createdContract;

            if (isNewContract)
            {
                createdContract = BusinessLogic.CreateEntity(contract, dtoEntity.RowNumber, exceptions, logger);
                // Дополнительно обновляем свойство Состояние, так как после установки регистрационного номера Состояние сбрасывается в значение "В разработке"
                createdContract?.UpdateLifeCycleState(dtoContract.LifeCycleState);
            }
            else
            {
                // Карточку не обновляем, там ошибка, если у документа есть версия.
                createdContract = contract;//BusinessLogic.UpdateEntity(contract, exceptionList, logger);
            }

            var update_body = ExtraParameters.ContainsKey("update_body") && ExtraParameters["update_body"] == "true";

            if (!string.IsNullOrWhiteSpace(dtoContract.Attachment.AttachmentPath))
                BusinessLogic.ImportBody(createdContract, dtoContract.Attachment, dtoEntity.RowNumber, exceptions, logger, update_body);
        }

        public override void SaveToRXBatch(IDtoEntity dtoEntity, List<Structures.ExceptionsStruct> exceptions, Logger logger)
        {
            ExtraParameters["update_body"] = "true";

            int entityId = -1;

            var dtoContract = dtoEntity as DtoContract;
            var contract = dtoContract.Contract;
            var isNewContract = false;

            if (contract == null)
            {
                contract = new IContracts() { Id = entityId };
                isNewContract = true;
            }

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
            contract.DocumentRegister = dtoContract.DocumentRegister;
            contract.RegistrationDate = dtoContract.RegistrationDate;
            contract.RegistrationNumber = dtoContract.RegistrationNumber;
            contract.RegistrationState = dtoContract.RegistrationState;
            contract.CaseFile = dtoContract.CaseFile;
            contract.PlacedToCaseFileDate = dtoContract.PlacedToCaseFileDate;

            //var properties = new Dictionary<string, object>()
            //{
            //    { nameof(contract.Name), dtoContract.Name},
            //    { nameof(contract.Created), DateTimeOffset.UtcNow},
            //    { nameof(contract.Counterparty), dtoContract.Counterparty},
            //    { nameof(contract.DocumentKind), dtoContract.DocumentKind},
            //    { nameof(contract.DocumentGroup), dtoContract.DocumentGroup},
            //    { nameof(contract.Subject), dtoContract.Subject},
            //    { nameof(contract.BusinessUnit), dtoContract.BusinessUnit},
            //    { nameof(contract.Department), dtoContract.Department},
            //    { nameof(contract.ValidFrom), dtoContract.ValidFrom},
            //    { nameof(contract.ValidTill), dtoContract.ValidTill},
            //    { nameof(contract.TotalAmount), dtoContract.TotalAmount},
            //    { nameof(contract.Currency), dtoContract.Currency},
            //    { nameof(contract.ResponsibleEmployee), dtoContract.ResponsibleEmployee},
            //    { nameof(contract.OurSignatory), dtoContract.OurSignatory},
            //    { nameof(contract.Note), dtoContract.Note},
            //    { nameof(contract.DocumentRegister), dtoContract.DocumentRegister},
            //    { nameof(contract.RegistrationDate), dtoContract.RegistrationDate},
            //    { nameof(contract.RegistrationNumber), dtoContract.RegistrationNumber},
            //    { nameof(contract.RegistrationState), dtoContract.RegistrationState},
            //    { nameof(contract.CaseFile), dtoContract.CaseFile},
            //    { nameof(contract.PlacedToCaseFileDate), dtoContract.PlacedToCaseFileDate}
            //};

            if (isNewContract)
            {
                BusinessLogic.CreateEntityBatch(contract, dtoEntity.RowNumber, exceptions, logger);
                //Client.CreateEntityBatch<IContracts>(properties);

                // Дополнительно обновляем свойство Состояние, так как после установки регистрационного номера Состояние сбрасывается в значение "В разработке"
                BusinessLogic.UpdatePropertiesBatch<IContracts>(entityId, dtoEntity.RowNumber, new Dictionary<string, object>() { { nameof(dtoContract.LifeCycleState), dtoContract.LifeCycleState } }, exceptions, logger);

                contract = null;
            }
            else
            {
                // Карточку не обновляем, там ошибка, если у документа есть версия.
                //BusinessLogic.UpdateEntityBatch(contract, exceptions, logger);
            }

            var update_body = ExtraParameters.ContainsKey("update_body") && ExtraParameters["update_body"] == "true";

            if (!string.IsNullOrWhiteSpace(dtoContract.Attachment.AttachmentPath))
                BusinessLogic.ImportBodyBatch(contract, entityId, dtoContract.Attachment, dtoContract.RowNumber, exceptions, logger, update_body);
        }




    }
}