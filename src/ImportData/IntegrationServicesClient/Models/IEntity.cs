﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ImportData.IntegrationServicesClient.Models
{
    public class IEntity
    {
        public int Id { get; set; }
		[PropertyOptions("Наименование", RequiredType.ForSearch, PropertyType.Simple)]
		public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
