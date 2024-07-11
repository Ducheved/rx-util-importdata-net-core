using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Diagnostics;
using ImportData.Dto;
using Simple.OData.Client;
using ImportData.Entities;
using ImportData.IntegrationServicesClient;

namespace ImportData
{   
    public class EntityProcessor
    {
        public static void ProcessOLD(Type type, string xlsxPath, string sheetName, Dictionary<string, string> extraParameters, string searchDoubles, bool isBatch, Logger logger)
        {
            if (type.Equals(typeof(Entity)))
            {
                logger.Error(string.Format("Не найден соответствующий обработчик операции: {0}", "action"));
                return;
            }

            Type genericType = typeof(EntityWrapper<>);
            Type[] typeArgs = { Type.GetType(type.ToString()) };
            Type wrapperType = genericType.MakeGenericType(typeArgs);
            object processor = Activator.CreateInstance(wrapperType);
            var getEntity = wrapperType.GetMethod("GetEntity");
            bool supplementEntity = false;
            var supplementEntityList = new List<string>();

            uint row = 2;
            uint rowImported = 1;
            var excelProcessor = new ExcelProcessor(xlsxPath, sheetName, logger);
            var importData = excelProcessor.GetDataFromExcel();
            var parametersListCount = importData.Count() - 1;
            var importItemCount = importData.First().Count();
            var exceptionList = new List<Structures.ExceptionsStruct>();
            var arrayItems = new ArrayList();
            var listImportItems = new List<string[]>();
            int paramCount = 0;
            var listResult = new List<List<Structures.ExceptionsStruct>>();

            logger.Info("===================Чтение строк из файла===================");
      
            var watch = Stopwatch.StartNew();
            // Пропускаем 1 строку, т.к. в ней заголовки таблицы.
            foreach (var importItem in importData.Skip(1))
            {
                int countItem = importItem.Count();
                foreach (var data in importItem.Take(countItem - 3))
                    arrayItems.Add(data);

                listImportItems.Add((string[])arrayItems.ToArray(typeof(string)));
                var percent = (double)(row - 1) / (double)parametersListCount * 100.00;
                logger.Info($"\rОбработано {row - 1} строк из {parametersListCount} ({percent:F2}%)");
                arrayItems.Clear();
                row++;
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            logger.Info($"Времени затрачено на чтение строк из файла: {elapsedMs} мс");
      
            logger.Info("======================Импорт сущностей=====================");
            row = 2;

            foreach (var importItem in listImportItems)
            {
                supplementEntity = false;
                var entity = (Entity)getEntity.Invoke(processor, new object[] { importItem.ToArray(), extraParameters });

                if (!supplementEntityList.Contains(importItem[2]))
                    supplementEntityList.Add(importItem[2]);

                if (supplementEntityList.Contains(importItem[0]))
                    supplementEntity = true;

                if (entity != null)
                {
                    if (importItemCount >= entity.GetPropertiesCount())
                    {
                    logger.Info($"Обработка сущности {row - 1}");
                    watch.Restart();

                    exceptionList = entity.SaveToRX(logger, supplementEntity, searchDoubles).ToList();
            
                    watch.Stop();
            
                    elapsedMs = watch.ElapsedMilliseconds;
                    if (exceptionList.Any(x => x.ErrorType == Constants.ErrorTypes.Error))
                    {
                        logger.Info($"Сущность {row - 1} не импортирована");
                    }
                    else
                    {
                        logger.Info($"Сущность {row - 1} импортирована");
                        logger.Info($"Времени затрачено на импорт сущности: {elapsedMs} мс");
                        rowImported++;
                    }
                    row++;
                    }
                    else
                    {
                    var message = string.Format("Количества входных параметров недостаточно. " +
                        "Количество ожидаемых параметров {0}. Количество переданных параметров {1}.", entity.GetPropertiesCount(), importItemCount);
                    exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
                    logger.Error(message);
                    }
                    listResult.Add(exceptionList);
                }

                if (paramCount == 0)
                    paramCount = entity.GetPropertiesCount();
            }

            var percent1 = (double)(rowImported - 1) / (double)parametersListCount * 100.00;
            logger.Info($"\rИмпортировано {rowImported - 1} сущностей из {parametersListCount} ({percent1:F2}%)");

            logger.Info("=============Запись результатов импорта в файл=============");
            watch.Restart();
            row = 2;

            var listArrayParams = new List<ArrayList>();
            string[] text = new string[] { "Итог", "Дата", "Подробности" };
            for (int i = 1; i <= 3; i++)
            {
                var title = excelProcessor.GetExcelColumnName(paramCount + i);
                var arrayParams = new ArrayList { text[i - 1], title, 1 };
                listArrayParams.Add(arrayParams);
            }

            foreach (var result in listResult)
            {
                if (result.Where(x => x.ErrorType == Constants.ErrorTypes.Error).Any())
                {
                    // TODO: Добавить локализацию строки.
                    var message = string.Join("; ", result.Where(x => x.ErrorType == Constants.ErrorTypes.Error).Select(x => x.Message).ToArray());
                    text = null;
                    text = new string[] { "Не загружен", DateTime.Now.ToString("d"), message };
                    for (int i = 1; i <= 3; i++)
                    {
                    var title = excelProcessor.GetExcelColumnName(paramCount + i);
                    var arrayParams = new ArrayList { text[i - 1], title, row };
                    listArrayParams.Add(arrayParams);
                    }
                }
                else if (result.Where(x => x.ErrorType == Constants.ErrorTypes.Warn).Any())
                {
                    // TODO: Добавить локализацию строки.
                    var message = string.Join(Environment.NewLine, result.Where(x => x.ErrorType == Constants.ErrorTypes.Warn).Select(x => x.Message).ToArray());
                    text = null;
                    text = new string[] { "Загружен частично", DateTime.Now.ToString("d"), message };
                    for (int i = 1; i <= 3; i++)
                    {
                    var title = excelProcessor.GetExcelColumnName(paramCount + i);
                    var arrayParams = new ArrayList { text[i - 1], title, row };
                    listArrayParams.Add(arrayParams);
                    }
                }
                else
                {
                    // TODO: Добавить локализацию строки.
                    text = null;
                    text = new string[] { "Загружен", DateTime.Now.ToString("d"), string.Empty };
                    for (int i = 1; i <= 3; i++)
                    {
                    var title = excelProcessor.GetExcelColumnName(paramCount + i);
                    var arrayParams = new ArrayList { text[i - 1], title, row };
                    listArrayParams.Add(arrayParams);
                    }
                }
                row++;
            }
            excelProcessor.InsertText(listArrayParams, parametersListCount);
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;
            logger.Info($"Времени затрачено на запись результатов в файл: {elapsedMs} мс");
        }

        public static void Process(Type type, string xlsxPath, string sheetName, Dictionary<string, string> extraParameters, string searchDoubles, bool isBatch, int maxRequestsPerBatch, Logger logger)
        {
            if (type.Equals(typeof(Entity)))
            {
                logger.Error(string.Format("Не найден соответствующий обработчик операции: {0}", "action"));
                return;
            }

            var excelProcessor = new ExcelProcessor(xlsxPath, sheetName, logger);
            var importData = excelProcessor.GetDataFromExcel();

            // Количество строк без шапки
            var rowsCount = importData.Count() - 1;

            // Количество столбцов с параметрами минус 3 столбца для лог-инфы
            var propertiesCount = importData.First().Count() - 3;

            var watch = Stopwatch.StartNew();

            var rowsList = GetRows(importData, logger);
            watch.Stop();
            logger.Info($"Времени затрачено на чтение строк из файла: {watch.ElapsedMilliseconds} мс");

            var exceptions = new List<Structures.ExceptionsStruct>();
            var dtoEntities = GetValidEntities(rowsList, rowsCount, propertiesCount, extraParameters, type, logger, watch, exceptions, searchDoubles);

            logger.Info("\r\n======================Импорт сущностей=====================");
            if (isBatch)
                ImportBatch(dtoEntities, maxRequestsPerBatch, exceptions, logger);
            else
                Import(dtoEntities, exceptions, logger);

            watch.Restart();
            WriteLog(excelProcessor, exceptions, propertiesCount, rowsCount, logger);
            watch.Stop();
            logger.Info($"Времени затрачено на запись результатов в файл: {watch.ElapsedMilliseconds} мс");
        }

        #region GetRows
        public static List<string[]> GetRows(IEnumerable<List<string>> importData, Logger logger)
        {
            logger.Info("===================Чтение строк из файла===================");

            uint row = 2;
            var rowsCount = importData.Count() - 1;
            var arrayItems = new ArrayList();
            var rowsList = new List<string[]>();

            // Пропускаем 1 строку, т.к. в ней заголовки таблицы.
            foreach (var importItem in importData.Skip(1))
            {
                var countItem = importItem.Count();
                foreach (var data in importItem.Take(countItem - 3))
                    arrayItems.Add(data);

                rowsList.Add((string[])arrayItems.ToArray(typeof(string)));
                var percent = (double)(row - 1) / rowsCount * 100.00;
                logger.Info($"\rОбработано {row - 1} строк из {rowsCount} ({percent:F2}%)");
                arrayItems.Clear();
                row++;
            }

            return rowsList;
        }
        #endregion

        #region Validate
        public static List<Entity> GetValidEntities(List<string[]> rowsList, int rowsCount, int propertiesCount, Dictionary<string, string> extraParameters, Type type, Logger logger, Stopwatch watch, List<Structures.ExceptionsStruct> exceptions, string searchDoubles)
        {
            logger.Info("======================Валидация сущностей=====================");

            uint currentRow = 2;
            uint validRows = 0;
            Type genericType = typeof(EntityWrapper<>);
            Type[] typeArgs = { Type.GetType(type.ToString()) };
            Type wrapperType = genericType.MakeGenericType(typeArgs);
            var getEntity = wrapperType.GetMethod("GetEntity");
            object processor = Activator.CreateInstance(wrapperType);
            var message = string.Empty;

            var entities = new List<Entity>();

            foreach (var importItem in rowsList)
            {
                var entity = (Entity)getEntity.Invoke(processor, new object[] { importItem.ToArray(), extraParameters });

                if (entity != null)
                {
                    if (propertiesCount >= entity.GetPropertiesCount())
                    {
                        logger.Info($"Строка {currentRow}. Валидация сущности.");
                        watch.Restart();

                        var dtoEntity = entity.Validate(exceptions, currentRow, logger);

                        if (dtoEntity != null)
                        {
                            entity.DtoEntity = dtoEntity;
                            entities.Add(entity);
                        }

                        watch.Stop();

                        if (exceptions.Any(e => e.RowNumber == currentRow && e.ExceptionType == ExceptionType.Error))
                        {
                            logger.Error($"Строка {currentRow}. Сущность не прошла валидацию.");
                        }
                        else
                        {
                            if (exceptions.Any(e => e.RowNumber == currentRow && e.ExceptionType == ExceptionType.Warn))
                                logger.Warn($"Строка {currentRow}. Сущность частично прошла валидацию.");
                            else
                                logger.Info($"Строка {currentRow}. Сущность прошла валидацию.");

                            logger.Info($"Времени затрачено на валидацию сущности: {watch.ElapsedMilliseconds} мс");
                            validRows++;
                        }
                    }
                    else
                    {
                        message = string.Format("Строка {0}. Количества входных параметров для сущности недостаточно. " +
                            "Количество ожидаемых параметров {1}. Количество переданных параметров {2}.", entity.GetPropertiesCount(), propertiesCount, currentRow);
                    }
                }
                else
                    message = string.Format("Строка {0}. Сущность не определена", currentRow);

                if (!string.IsNullOrEmpty(message))
                {
                    exceptions.Add(new Structures.ExceptionsStruct { RowNumber = currentRow, ExceptionType = ExceptionType.Error, Message = message });
                    logger.Error(message);
                }

                currentRow++;
            }

            var percent1 = (double)validRows / rowsCount * 100.00;
            logger.Info($"\rВалидацию прошло {validRows} сущностей из {rowsCount} ({percent1:F2}%)");

            return entities;
        }
        #endregion

        #region Import
        public static void Import(List<Entity> entities, List<Structures.ExceptionsStruct> exceptions, Logger logger)
        {
            foreach (var entity in entities)
            {
                var rowNumber = entity.DtoEntity.RowNumber;

                try
                {
                    logger.Info($"Строка {rowNumber}. Импорт сущности.");

                    entity.SaveToRX(entity.DtoEntity, exceptions, logger);
                }
                catch (Exception ex)
                {
                    logger.Info($"Строка {rowNumber}. Сущность не импортирована.");
                    BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, ex.Message, logger);
                }
            }
        }

        public static void ImportBatch(List<Entity> entities, int maxRequestsPerBatch, List<Structures.ExceptionsStruct> exceptions, Logger logger)
        {
            int entryCount = 0;
            var entitiesCount = entities.Count();

            foreach (var entity in entities)
            {
                entryCount++;
                var rowNumber = entity.DtoEntity.RowNumber;

                try
                {
                    logger.Info($"Строка {rowNumber}. Импорт сущности.");

                    entity.SaveToRXBatch(entity.DtoEntity, exceptions, logger);

                    if ((entryCount % maxRequestsPerBatch == 0) || entryCount == entitiesCount)
                        BusinessLogic.SaveBatch(logger);
                }
                catch (Exception ex)
                {
                    logger.Info($"Строка {rowNumber}. Сущность не импортирована.");
                    BusinessLogic.LogException(exceptions, rowNumber, ExceptionType.Error, ex.Message, logger);
                }
            }
        }
        #endregion

        #region WriteLog
        public static void WriteLog(ExcelProcessor excelProcessor, List<Structures.ExceptionsStruct> exceptions, int propertiesCount, int rowsCount, Logger logger)
        {
            logger.Info("=============Запись результатов импорта в файл=============");

            string info;
            var currentDate = DateTime.Now.ToString("d");
            var listArrayParams = new List<ArrayList>();

            string[] text = new string[] { "Итог", "Дата", "Подробности" };
            for (int i = 1; i <= 3; i++)
            {
                var title = excelProcessor.GetExcelColumnName(propertiesCount + i);
                var arrayParams = new ArrayList { text[i - 1], title, 1 };
                listArrayParams.Add(arrayParams);
            }

            var exceptionsDict = exceptions.GroupBy(e => new { e.RowNumber, e.ExceptionType }).ToDictionary(g => g.Key, g => string.Join("; ", g.Select(m => m.Message)));

            foreach (var item in exceptionsDict)
            {
                info = "Загружен";

                if (item.Key.ExceptionType == ExceptionType.Warn)
                    info = "Загружен частично";

                if (item.Key.ExceptionType == ExceptionType.Error)
                    info = "Не загружен";

                text = null;
                text = new string[] { info, currentDate, item.Value };
                for (int i = 1; i <= 3; i++)
                {
                    var title = excelProcessor.GetExcelColumnName(propertiesCount + i);
                    var arrayParams = new ArrayList { text[i - 1], title, item.Key.RowNumber };
                    listArrayParams.Add(arrayParams);
                }
            }

            excelProcessor.InsertText(listArrayParams, rowsCount);
        }
        #endregion



    }
}
