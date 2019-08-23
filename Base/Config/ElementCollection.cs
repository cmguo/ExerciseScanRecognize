using System.Configuration;

namespace Base.Config
{

    public abstract class Element : ConfigurationElement
    {
        public abstract object Key { get; }
    }

    public class ElementCollection<T> : ConfigurationElementCollection where T : Element, new()
    {
        #region Fields
        private static ConfigurationPropertyCollection properties;
        #endregion

        #region Constructors

        static ElementCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }

        public ElementCollection()
        {
        }

        #endregion

        #region Properties
        protected override ConfigurationPropertyCollection Properties => properties;

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.AddRemoveClearMap;
        #endregion

        #region Indexers
        public T this[int index]
        {
            get { return BaseGet(index) as T; }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new T this[string name]
        {
            get { return base.BaseGet(name) as T; }
        }
        #endregion

        #region Overrides
        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as T).Key;
        }
        #endregion
    }
}
