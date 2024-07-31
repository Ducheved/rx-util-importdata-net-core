using ImportData.IntegrationServicesClient.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImportData.Entities.EDocs
{
  public class DocumentEntity : Entity
  {
    public override IEnumerable<Structures.ExceptionsStruct> SaveToRX(NLog.Logger logger, bool supplementEntity, string ignoreDuplicates)
    {
      var exceptionList = new List<Structures.ExceptionsStruct>();

      //Перед обработкой сущности проверим, что в шаблоне есть обязательное поле "файл" и указанный по пути файл существует,
      //при не обрабатываем сущность. При добавлении новых сущностей, предполагающих обязательную загрузку файла, в константу RequiredDocumentBody
      //необходимо добавить новый тип сущности.
      if (CheckNeedRequiredDocumentBody(EntityType, out var exceptions))
      {
        if (exceptions.Count > 0)
        {
          exceptionList.AddRange(exceptions);
          return exceptionList;
        }
      }

      exceptionList.AddRange(base.SaveToRX(logger, supplementEntity, ignoreDuplicates));

      //Загрузка тела сущности в систему 
      if (NamingParameters.ContainsKey(Constants.CellNameFile))
      {
        IEntityBase entity = null;
        var propertiesForCreate = GetPropertiesForSearch(EntityType, exceptionList, logger);
        entity = (IEntityBase)MethodCall(EntityType, Constants.EntityActions.FindEntity, propertiesForCreate, this, true, exceptionList, logger);
        var filePath = NamingParameters[Constants.CellNameFile];
        if (!string.IsNullOrWhiteSpace(filePath) && entity != null)
          exceptionList.AddRange(BusinessLogic.ImportBody((IElectronicDocuments)entity, filePath, logger));
      }

      return exceptionList;
    }

    protected override string GetName()
    {
      var documentKind = ResultValues[Constants.KeyAttributes.DocumentKind];
      var subject = ResultValues[Constants.KeyAttributes.Subject];

      return string.Format("{0} \"{1}\"", documentKind, subject);
    }

    protected override bool FillProperies(List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
    {
      ResultValues[Constants.KeyAttributes.Name] = GetName();
      ResultValues[Constants.KeyAttributes.Created] = ResultValues[Constants.KeyAttributes.RegistrationDate];
      ResultValues[Constants.KeyAttributes.RegistrationState] = BusinessLogic.GetRegistrationsState((string)ResultValues[Constants.KeyAttributes.RegistrationState]);
      ResultValues[Constants.KeyAttributes.LifeCycleState] = BusinessLogic.GetPropertyLifeCycleState((string)ResultValues[Constants.KeyAttributes.LifeCycleState]);

      return false;
    }

    /// <summary>
    /// Проверка требования наличия пути к телу документа и самого документа по пути
    /// </summary>
    /// <param name="entityType">Сущность RX для заполнения.</param>
    /// <returns>Результат проверки.</returns>
    protected bool CheckNeedRequiredDocumentBody(Type entityType, out List<Structures.ExceptionsStruct> exceptionList)
    {
      exceptionList = new List<Structures.ExceptionsStruct>();
      if (Constants.RequiredDocumentBody.Contains(entityType))
      {
        if (NamingParameters.ContainsKey(Constants.CellNameFile))
        {
          var pathToBody = NamingParameters[Constants.CellNameFile];
          if (string.IsNullOrWhiteSpace(pathToBody))
          {
            exceptionList.Add(new Structures.ExceptionsStruct
            {
              ErrorType = Constants.ErrorTypes.Error,
              Message = string.Format(Constants.Resources.EmptyColumn, Constants.CellNameFile)
            });
          }
          if (!System.IO.File.Exists(pathToBody))
            exceptionList.Add(new Structures.ExceptionsStruct
            {
              ErrorType = Constants.ErrorTypes.Error,
              Message = string.Format(Constants.Resources.FileNotExist, pathToBody)
            });
        }
        else
          exceptionList.Add(new Structures.ExceptionsStruct
          {
            ErrorType = Constants.ErrorTypes.Error,
            Message = string.Format(Constants.Resources.NeedRequiredDocumentBody, Constants.CellNameFile)
          });
        return true;
      }
      else
        return false;
    }
  }
}