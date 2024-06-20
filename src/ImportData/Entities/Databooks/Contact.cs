﻿using System;
using System.Collections.Generic;
using NLog;
using ImportData.IntegrationServicesClient.Models;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ImportData.Entities.Databooks
{
  public class Contact : Entity
  {
    public int PropertiesCount = 10;

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
    public override IEnumerable<Structures.ExceptionsStruct> SaveToRX(Logger logger, bool supplementEntity, string ignoreDuplicates, int shift = 0)
    {
      var exceptionList = new List<Structures.ExceptionsStruct>();

      var lastName = this.Parameters[shift + 0].Trim();

      if (string.IsNullOrEmpty(lastName))
      {
        var message = string.Format("Не заполнено поле \"Фамилия\".");
        exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
        logger.Error(message);

        return exceptionList;
      }

      var firstName = this.Parameters[shift + 1].Trim();

      if (string.IsNullOrEmpty(firstName))
      {
        var message = string.Format("Не заполнено поле \"Имя\".");
        exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = message });
        logger.Error(message);

        return exceptionList;
      }

      var middleName = this.Parameters[shift + 2].Trim();

      var phones = this.Parameters[shift + 5].Trim();
      var email = this.Parameters[shift + 7].Trim();
      var name = string.Format("{0} {1} {2}", lastName, firstName, middleName);
      var person = BusinessLogic.GetEntityWithFilter<IPersons>(x => x.FirstName == firstName && x.MiddleName == middleName && x.LastName == lastName && x.Email == email, exceptionList, logger);

      if (person == null)
      {
        person = BusinessLogic.CreateEntity<IPersons>(new IPersons() { FirstName = firstName, MiddleName = middleName, LastName = lastName, Name = name, Status = "Active", Email = email, Phones = phones }, exceptionList, logger);
      }
      else if (person.Phones != phones)
      {
        person.Phones = phones;
        BusinessLogic.UpdateEntity<IPersons>(person, exceptionList, logger);
      }

      var variableForParameters = this.Parameters[shift + 3].Trim();
      var company = BusinessLogic.GetEntityWithFilter<ICompanies>(x => x.Name == variableForParameters, exceptionList, logger);

      if (company == null && !string.IsNullOrEmpty(this.Parameters[shift + 3].Trim()))
      {
        company = BusinessLogic.CreateEntity<ICompanies>(new ICompanies() { Name = variableForParameters, Status = "Active" }, exceptionList, logger);
      }

      var jobTitle = this.Parameters[shift + 4].Trim();
      var fax = this.Parameters[shift + 6].Trim();
      var homepage = this.Parameters[shift + 8].Trim();
      var note = this.Parameters[shift + 9].Trim();

      try
      {
        IContacts contact = null;
        var isNewContact = false;

        if (ignoreDuplicates.ToLower() != Constants.ignoreDuplicates.ToLower())
        {
          contact = BusinessLogic.GetEntityWithFilter<IContacts>(x => x.Email == email && x.Name == name, exceptionList, logger);
        }
        if (contact == null)
        {
          isNewContact = true;
          contact = new IContacts();
        }

        contact.Person = person;
        contact.Company = company;
        contact.Name = name;
        contact.JobTitle = jobTitle;
        contact.Phone = person.Phones;
        contact.Fax = fax;
        contact.Email = person.Email;
        contact.Homepage = homepage;
        contact.Note = note;
        contact.Status = "Active";

        if (isNewContact)
          BusinessLogic.CreateEntity(contact, exceptionList, logger);
        else
          BusinessLogic.UpdateEntity(contact, exceptionList, logger);

      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = ex.Message });

        return exceptionList;
      }

      return exceptionList;
    }
  }
}
