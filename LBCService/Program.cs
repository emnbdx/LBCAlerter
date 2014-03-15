using EMToolBox.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace LBCService
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        static void Main()
        {
            DebuggableService service = new DebuggableService(typeof(TimedService));
            ServiceBase.Run(service);
        }
    }
}
