
using System.Configuration;

namespace Errordite.Utils.DevUtility.Configuration
{
    public class DevUtilityConfiguration : ConfigurationSection
    {
        private static readonly DevUtilityConfiguration _configuration =
            ConfigurationManager.GetSection("devutility") as DevUtilityConfiguration;

        public static DevUtilityConfiguration Current
        {
            get
            {
                return _configuration;
            }
        }

        [ConfigurationProperty("services", IsDefaultCollection = true, IsKey = false, IsRequired = true)]
        public ServiceElementCollection Services
        {
            get
            {
                return base["services"] as ServiceElementCollection;
            }
        }
    }

    #region Services

    public class ServiceElementCollection : ConfigurationElementCollection
    {
        public ServiceElement this[int index]
        {
            get
            {
                return BaseGet(index) as ServiceElement;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "service";
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServiceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServiceElement)element).Name;
        }

        protected override bool IsElementName(string elementName)
        {
            return !string.IsNullOrEmpty(elementName) && elementName == "service";
        }
    }

    public class ServiceElement : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("displayName", DefaultValue = "", IsRequired = true)]
        public string DisplayName
        {
            get
            {
                return (string)this["displayName"];
            }
            set
            {
                this["displayName"] = value;
            }
        }

        [ConfigurationProperty("description", DefaultValue = "", IsRequired = true)]
        public string Description
        {
            get
            {
                return (string)this["description"];
            }
            set
            {
                this["description"] = value;
            }
        }

        [ConfigurationProperty("relativePath", DefaultValue = "", IsRequired = true)]
        public string RelativePath
        {
            get
            {
                return (string)this["relativePath"];
            }
            set
            {
                this["relativePath"] = value;
            }
        }

        [ConfigurationProperty("servicePath", DefaultValue = "", IsRequired = false)]
        public string ServicePath
        {
            get
            {
                return (string)this["servicePath"];
            }
            set
            {
                this["servicePath"] = value;
            }
        }

        [ConfigurationProperty("isNServiceBusService", DefaultValue = true, IsRequired = true)]
        public bool IsNServiceBusService
        {
            get
            {
                return (bool)this["isNServiceBusService"];
            }
            set
            {
                this["isNServiceBusService"] = value;
            }
        }
    }

    #endregion
}
