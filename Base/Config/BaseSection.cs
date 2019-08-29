using Base.Misc;
using System;
using System.Configuration;
using System.Xml;

namespace Base.Config
{
    public class BaseSection : ConfigurationSection
    {

        private static readonly Logger Log = Logger.GetLogger<BaseSection>();
        //
        // 摘要:
        //     获取或设置的属性或此配置元素的特性。
        //
        // 参数:
        //   prop:
        //     要访问的属性。
        //
        // 返回结果:
        //     指定的属性、特性或子元素。
        //
        // 异常:
        //   T:System.Configuration.ConfigurationException:
        //     prop 是 null 或不存在的元素中。
        //
        //   T:System.Configuration.ConfigurationErrorsException:
        //     prop 为只读或锁定。
        protected new virtual object this[ConfigurationProperty prop]
        {
            get => SimpleElement.Unwrap(base[prop]);
            set { Set(prop, value); }
        }

        private static readonly object mutex = new object();
        private static Configuration editableConfig;

        private static Configuration GetEditableConfig()
        {
            if (editableConfig == null)
            {
                lock (mutex)
                {
                    if (editableConfig == null)
                        editableConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                }
            }
            return editableConfig;
        }

        private BaseSection editableSection;

        protected void Set(ConfigurationProperty prop, object value)
        {
            if (typeof(ISimpleElement).IsAssignableFrom(prop.Type))
            {
                ISimpleElement se = Activator.CreateInstance(prop.Type) as ISimpleElement;
                se.Value = value;
                value = se;
            }
            base[prop] = value;
            if (editableSection == null)
            {
                editableSection = GetEditableConfig().GetSection(SectionInformation.Name) as BaseSection;
            }
            editableSection.SetPropertyValue(prop, value, true);
            editableConfig.Save(ConfigurationSaveMode.Minimal);
            ConfigurationManager.RefreshSection(SectionInformation.Name);
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            Log.w("Unrecognized attribute " + name);
            return true;
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            Log.w("Unrecognized element " + elementName);
            return true;
        }
    }
}
