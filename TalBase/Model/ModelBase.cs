using Base.Mvvm;
using System.Collections.Generic;

namespace TalBase.Model
{
    public class ModelBase : NotifyBase
    {

        private static readonly List<ModelBase> models = new List<ModelBase>();

        public static void ShutdownAll()
        {
            foreach (ModelBase m in models)
            {
                m.Shutdown();
            }
        }

        protected ModelBase()
        {
            models.Add(this);
        }

        public virtual void Shutdown()
        {
        }
    }
}
