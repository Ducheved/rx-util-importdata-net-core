﻿using System.Collections.Generic;

namespace ImportData.IntegrationServicesClient.Models
{
    [EntityName("Регионы")]
    public class IRegions : IEntity
    {
        public string Status { get; set; }
        public string Code { get; set; }

        new public static IRegions FindEntity(Dictionary<string, string> propertiesForSearch, Entity entity, bool isEntityForUpdate, List<Structures.ExceptionsStruct> exceptionList, NLog.Logger logger)
        {
            var name = propertiesForSearch[Constants.KeyAttributes.Name];
            return BusinessLogic.GetEntityWithFilter<IRegions>(x => x.Name == name, exceptionList, logger);
        }
    }
}
