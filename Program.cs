using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace sfwServiceTache
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        static void Main()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new Service1() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
