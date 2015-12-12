using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace EMToolBox.Services
{
    public partial class ServiceManagerDialog : Form
    {
        private Type m_serviceType;
        private ServiceDebugDialog m_serviceDebugDialog = null;
        public ServiceManagerDialog(Type serviceType)
        {
            InitializeComponent();
            m_serviceType = serviceType;
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            string displayName = Application.ProductName;
            string internalName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            if (SingleFieldInputDialog.Show(
                this,
                "Installation du service",
                "Libellé du service",
                displayName,
                50,
                out displayName) == DialogResult.OK)
            {
                if (SingleFieldInputDialog.Show(
                    this,
                    "Installation du service",
                    "Nom Interne du service",
                    internalName,
                    50,
                    out internalName) == DialogResult.OK)
                {

                    ServiceManager.Install(displayName, internalName, null, null);
                    MessageBox.Show(
                        this,
                        "Service was installed with name [" + displayName + "]",
                        "Install",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
               }
            }
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            string internalName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            if (SingleFieldInputDialog.Show(
                this,
                "Désinstallation du service",
                "Nom Interne du service",
                internalName,
                50,
                out internalName) == DialogResult.OK)
            {
                ServiceManager.Uninstall(internalName);
            }
        }

        private void btnStartDebug_Click(object sender, EventArgs e)
        {
            if (m_serviceDebugDialog == null) m_serviceDebugDialog = new ServiceDebugDialog(m_serviceType);
            m_serviceDebugDialog.ShowDialog();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}