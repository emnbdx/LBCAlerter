using System;
using System.ComponentModel;

namespace EMToolBox.Services.Installer
{
    [AttributeUsage(AttributeTargets.All)]
    public class ServiceProcessDescriptionExAttribute : DescriptionAttribute
    {
        private bool replaced;

        public ServiceProcessDescriptionExAttribute(string description)
            : base(description)
        {
        }

        public override string Description
        {
            get
            {
                if (!this.replaced)
                {
                    this.replaced = true;
                    base.DescriptionValue = Res.GetString(base.Description);
                }
                return base.Description;
            }
        }
    }
}

