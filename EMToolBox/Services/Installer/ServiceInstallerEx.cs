using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.ServiceProcess;

namespace EMToolBox.Services.Installer
{
    public class ServiceInstallerEx : ComponentInstaller
    {
        private string description = "";
        private string displayName = "";
        private static bool environmentChecked;
        private EventLogInstaller eventLogInstaller = new EventLogInstaller();
        private static bool isWin9x;
        private const string LocalServiceName = @"NT AUTHORITY\LocalService";
        private const string NetworkServiceName = @"NT AUTHORITY\NetworkService";
        private string serviceName = "";
        private string[] servicesDependedOn = new string[0];
        private ServiceStartMode startType = ServiceStartMode.Manual;
        private string m_commandLineOptions = "";

        public ServiceInstallerEx()
        {
            this.eventLogInstaller.Log = "Application";
            this.eventLogInstaller.Source = "";
            this.eventLogInstaller.UninstallAction = UninstallAction.Remove;
            base.Installers.Add(this.eventLogInstaller);
        }

        internal static void CheckEnvironment()
        {
            if (environmentChecked)
            {
                if (isWin9x)
                {
                    throw new PlatformNotSupportedException(Res.GetString("CantControlOnWin9x"));
                }
            }
            else
            {
                isWin9x = Environment.OSVersion.Platform != PlatformID.Win32NT;
                environmentChecked = true;
                if (isWin9x)
                {
                    throw new PlatformNotSupportedException(Res.GetString("CantInstallOnWin9x"));
                }
            }
        }

        public override void CopyFromComponent(IComponent component)
        {
            if (!(component is ServiceBase))
            {
                throw new ArgumentException(Res.GetString("NotAService"));
            }
            ServiceBase base2 = (ServiceBase)component;
            this.ServiceName = base2.ServiceName;
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Context.LogMessage(Res.GetString("InstallingService", new object[] { this.ServiceName }));
            try
            {
                CheckEnvironment();
                string servicesStartName = null;
                string password = null;
                ServiceProcessInstaller parent = null;
                if (base.Parent is ServiceProcessInstaller)
                {
                    parent = (ServiceProcessInstaller)base.Parent;
                }
                else
                {
                    for (int j = 0; j < base.Parent.Installers.Count; j++)
                    {
                        if (base.Parent.Installers[j] is ServiceProcessInstaller)
                        {
                            parent = (ServiceProcessInstaller)base.Parent.Installers[j];
                            break;
                        }
                    }
                }
                if (parent == null)
                {
                    throw new InvalidOperationException(Res.GetString("NoInstaller"));
                }
                switch (parent.Account)
                {
                    case ServiceAccount.LocalService:
                        servicesStartName = @"NT AUTHORITY\LocalService";
                        break;

                    case ServiceAccount.NetworkService:
                        servicesStartName = @"NT AUTHORITY\NetworkService";
                        break;

                    case ServiceAccount.User:
                        servicesStartName = parent.Username;
                        password = parent.Password;
                        break;
                }
                string binaryPath = base.Context.Parameters["assemblypath"];
                if ((binaryPath == null) || (binaryPath.Length == 0))
                {
                    throw new InvalidOperationException(Res.GetString("FileName"));
                }
                binaryPath = "\"" + binaryPath + "\"";
                if (!string.IsNullOrEmpty(m_commandLineOptions)) binaryPath += " " + m_commandLineOptions;
                if (!ValidateServiceName(this.ServiceName))
                {
                    object[] args = new object[] { this.ServiceName, 80.ToString(CultureInfo.CurrentCulture) };
                    throw new InvalidOperationException(Res.GetString("ServiceName", args));
                }
                if (this.DisplayName.Length > 0xff)
                {
                    throw new ArgumentException(Res.GetString("DisplayNameTooLong", new object[] { this.DisplayName }));
                }
                string dependencies = null;
                if (this.ServicesDependedOn.Length > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int k = 0; k < this.ServicesDependedOn.Length; k++)
                    {
                        string name = this.ServicesDependedOn[k];
                        try
                        {
                            ServiceController controller = new ServiceController(name, ".");
                            name = controller.ServiceName;
                        }
                        catch
                        {
                        }
                        builder.Append(name);
                        builder.Append('\0');
                    }
                    builder.Append('\0');
                    dependencies = builder.ToString();
                }
                IntPtr databaseHandle = SafeNativeMethods.OpenSCManager(null, null, 0xf003f);
                IntPtr zero = IntPtr.Zero;
                if (databaseHandle == IntPtr.Zero)
                {
                    throw new InvalidOperationException(Res.GetString("OpenSC", new object[] { "." }), new Win32Exception());
                }
                int serviceType = 0x10;
                int num4 = 0;
                for (int i = 0; i < base.Parent.Installers.Count; i++)
                {
                    if (base.Parent.Installers[i] is ServiceInstallerEx)
                    {
                        num4++;
                        if (num4 > 1)
                        {
                            break;
                        }
                    }
                }
                if (num4 > 1)
                {
                    serviceType = 0x20;
                }
                try
                {
                    zero = NativeMethods.CreateService(databaseHandle, this.ServiceName, this.DisplayName, 0xf01ff, serviceType, (int)this.StartType, 1, binaryPath, null, IntPtr.Zero, dependencies, servicesStartName, password);
                    if (zero == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }
                    if (this.Description.Length != 0)
                    {
                        NativeMethods.SERVICE_DESCRIPTION serviceDesc = new NativeMethods.SERVICE_DESCRIPTION();
                        serviceDesc.description = Marshal.StringToHGlobalUni(this.Description);
                        bool flag = NativeMethods.ChangeServiceConfig2(zero, 1, ref serviceDesc);
                        Marshal.FreeHGlobal(serviceDesc.description);
                        if (!flag)
                        {
                            throw new Win32Exception();
                        }
                    }
                    stateSaver["installed"] = true;
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        SafeNativeMethods.CloseServiceHandle(zero);
                    }
                    SafeNativeMethods.CloseServiceHandle(databaseHandle);
                }
                base.Context.LogMessage(Res.GetString("InstallOK", new object[] { this.ServiceName }));
            }
            finally
            {
                base.Install(stateSaver);
            }
        }

        public override bool IsEquivalentInstaller(ComponentInstaller otherInstaller)
        {
            ServiceInstallerEx installer = otherInstaller as ServiceInstallerEx;
            if (installer == null)
            {
                return false;
            }
            return (installer.ServiceName == this.ServiceName);
        }

        private void RemoveService()
        {
            base.Context.LogMessage(Res.GetString("ServiceRemoving", new object[] { this.ServiceName }));
            IntPtr databaseHandle = SafeNativeMethods.OpenSCManager(null, null, 0xf003f);
            if (databaseHandle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            IntPtr zero = IntPtr.Zero;
            try
            {
                zero = NativeMethods.OpenService(databaseHandle, this.ServiceName, 0x10000);
                if (zero == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
                NativeMethods.DeleteService(zero);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseServiceHandle(zero);
                }
                SafeNativeMethods.CloseServiceHandle(databaseHandle);
            }
            base.Context.LogMessage(Res.GetString("ServiceRemoved", new object[] { this.ServiceName }));
            try
            {
                using (ServiceController controller = new ServiceController(this.ServiceName))
                {
                    if (controller.Status != ServiceControllerStatus.Stopped)
                    {
                        base.Context.LogMessage(Res.GetString("TryToStop", new object[] { this.ServiceName }));
                        controller.Stop();
                        int num = 10;
                        controller.Refresh();
                        while ((controller.Status != ServiceControllerStatus.Stopped) && (num > 0))
                        {
                            Thread.Sleep(0x3e8);
                            controller.Refresh();
                            num--;
                        }
                    }
                }
            }
            catch
            {
            }
            Thread.Sleep(0x1388);
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            object obj2 = savedState["installed"];
            if ((obj2 != null) && ((bool)obj2))
            {
                this.RemoveService();
            }
        }

        private bool ShouldSerializeServicesDependedOn()
        {
            return ((this.servicesDependedOn != null) && (this.servicesDependedOn.Length > 0));
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            this.RemoveService();
        }

        private static bool ValidateServiceName(string name)
        {
            if (((name == null) || (name.Length == 0)) || (name.Length > 80))
            {
                return false;
            }
            char[] chArray = name.ToCharArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                if (((chArray[i] < ' ') || (chArray[i] == '/')) || (chArray[i] == '\\'))
                {
                    return false;
                }
            }
            return true;
        }

        [ComVisible(false), ServiceProcessDescriptionEx("ServiceInstallerDescription"), DefaultValue("")]
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                this.description = value;
            }
        }

        [DefaultValue(""), ServiceProcessDescriptionEx("ServiceInstallerDisplayName")]
        public string DisplayName
        {
            get
            {
                return this.displayName;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                this.displayName = value;
            }
        }

        [DefaultValue(""), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ServiceProcessDescriptionEx("ServiceInstallerServiceName")]
        public string ServiceName
        {
            get
            {
                return this.serviceName;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (!ValidateServiceName(value))
                {
                    object[] args = new object[] { value, 80.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentException(Res.GetString("ServiceName", args));
                }
                this.serviceName = value;
                this.eventLogInstaller.Source = value;
            }
        }

        [ServiceProcessDescriptionEx("ServiceInstallerServicesDependedOn")]
        public string[] ServicesDependedOn
        {
            get
            {
                return this.servicesDependedOn;
            }
            set
            {
                if (value == null)
                {
                    value = new string[0];
                }
                this.servicesDependedOn = value;
            }
        }

        [DefaultValue(3), ServiceProcessDescriptionEx("ServiceInstallerStartType")]
        public ServiceStartMode StartType
        {
            get
            {
                return this.startType;
            }
            set
            {
                if (!Enum.IsDefined(typeof(ServiceStartMode), value))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(ServiceStartMode));
                }
                this.startType = value;
            }
        }

        public string CommandLineOptions { get { return m_commandLineOptions; } set { m_commandLineOptions = value; } }
    }

    public enum ServiceStartMode
    {
        Automatic = 2,
        Disabled = 4,
        Manual = 3
    }

}

