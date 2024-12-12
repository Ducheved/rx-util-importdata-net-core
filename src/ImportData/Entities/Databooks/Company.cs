using DocumentFormat.OpenXml.Wordprocessing;
using ImportData.IntegrationServicesClient.Models;
using NLog;
using System;
using System.Collections.Generic;

namespace ImportData
{
    class Company : Entity
    {
        public override int PropertiesCount { get { return 21; } }
        protected override Type EntityType { get { return typeof(ICompanies); } }

        protected override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
        {
            try
            {
                if (ResultValues == null)
                {
                    logger.Error("ResultValues is null");
                    return false;
                }

                if (NamingParameters == null)
                {
                    logger.Error("NamingParameters is null");
                    return false;
                }

                logger.Info("Список всех доступных параметров:");
                foreach (var param in NamingParameters)
                {
                    logger.Info($"Параметр: {param.Key} = {param.Value}");
                }

                if (ResultValues.ContainsKey(Constants.KeyAttributes.HeadCompany))
                {
                    if (ResultValues[Constants.KeyAttributes.HeadCompany] != null &&
                        ResultValues.ContainsKey(Constants.KeyAttributes.Name) &&
                        ((IEntity)ResultValues[Constants.KeyAttributes.HeadCompany]).Name == (string)ResultValues[Constants.KeyAttributes.Name])
                    {
                        ResultValues[Constants.KeyAttributes.HeadCompany] = null;
                    }
                }

                if (ResultValues.ContainsKey(Constants.KeyAttributes.Nonresident))
                {
                    ResultValues[Constants.KeyAttributes.Nonresident] = BusinessLogic.GetBoolProperty((string)ResultValues[Constants.KeyAttributes.Nonresident]);
                }

                if (ResultValues.ContainsKey(Constants.KeyAttributes.Status))
                {
                    ResultValues[Constants.KeyAttributes.Status] = Constants.AttributeValue[Constants.KeyAttributes.Status];
                }

                logger.Info("Начало обработки вида контрагента");
                ResultValues["Status"] = "Active";

                if (NamingParameters.ContainsKey("Вид контрагента"))
                {
                    var kindValue = NamingParameters["Вид контрагента"];
                    logger.Info($"Найдено значение вида контрагента: {kindValue}");

                    if (!string.IsNullOrEmpty(kindValue))
                    {
                        logger.Info($"Поиск вида контрагента с именем: {kindValue.Trim()}");

                        var counterpartyKind = BusinessLogic.GetEntityWithFilter<ICounterpartyKind>(
                            k => k.Name == kindValue.Trim() && k.Status == "Active",
                            exceptionList,
                            logger);

                        if (counterpartyKind != null)
                        {
                            logger.Info($"Найден вид контрагента с ID: {counterpartyKind.Id}");
                            ResultValues["Kind"] = counterpartyKind;
                        }
                        else
                        {
                            var message = $"Не найден активный вид контрагента \"{kindValue.Trim()}\" для организации \"{ResultValues[Constants.KeyAttributes.Name]}\"";
                            logger.Warn(message);
                            BusinessLogic.GetWarnResult(exceptionList, logger, message);
                        }
                    }
                }

                else
                {
                    logger.Info("Колонка 'Вид контрагента' не найдена в шаблоне");
                }

                var nonresident = ResultValues.ContainsKey(Constants.KeyAttributes.Nonresident) ?
                    (bool)ResultValues[Constants.KeyAttributes.Nonresident] : false;
                var tin = ResultValues.ContainsKey(Constants.KeyAttributes.TIN) ?
                    (string)ResultValues[Constants.KeyAttributes.TIN] : string.Empty;
                var trrc = ResultValues.ContainsKey(Constants.KeyAttributes.TRRC) ?
                    (string)ResultValues[Constants.KeyAttributes.TRRC] : string.Empty;
                var psrn = ResultValues.ContainsKey(Constants.KeyAttributes.PSRN) ?
                    (string)ResultValues[Constants.KeyAttributes.PSRN] : string.Empty;
                var nceo = ResultValues.ContainsKey(Constants.KeyAttributes.NCEO) ?
                    (string)ResultValues[Constants.KeyAttributes.NCEO] : string.Empty;

                return CheckCompanyRequsite(nonresident, tin, trrc, psrn, nceo, exceptionList, logger);
            }
            catch (Exception ex)
            {
                logger.Error($"Ошибка в FillProperies: {ex.Message}");
                logger.Error($"Stack Trace: {ex.StackTrace}");
                exceptionList.Add(new Structures.ExceptionsStruct
                {
                    ErrorType = Constants.ErrorTypes.Error,
                    Message = ex.Message
                });
                return true;
            }
        }
    }
}