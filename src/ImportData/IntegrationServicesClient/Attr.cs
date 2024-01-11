using System;

namespace ImportData.IntegrationServicesClient
{
    public class EntityName : Attribute
    {
        string name;
        public EntityName(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return name;
        }
    }

    public class PropertyOptions : Attribute
    {
        public PropertyOptions(string excelName, RequiredType required, PropertyType type)
        {
            ExcelName = excelName;
		    Required = required;
		    Type = type;
	    }

        public string ExcelName { get; }
		public RequiredType Required { get; }
		public PropertyType Type { get; }

        public bool IsRequired()
        {
            return Required == RequiredType.Required;
        }

        public bool IsSimple()
        {
            return Type == PropertyType.Simple;
        }
	}
}
