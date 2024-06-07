using System;
using SignEdgeService.Interfaces;

namespace SignEdgeService
{
    internal class ServiceLocator
    {
        private static ServiceLocator? locator = null;

        public static ServiceLocator Instance
        {
            get
            {
                if (locator == null)
                {
                    locator = new ServiceLocator();
                }
                return locator;
            }
        }

        private ServiceLocator()
        {
        }

        private IACMEService? acmeLocator = null;
        private IHostService? host = null;

        public IACMEService GetIACMELocator()
        {
            if (acmeLocator == null)
            {
                acmeLocator = new ACMEService();
            }
            return acmeLocator;
        }

        public IHostService GetIIHostService()
        {
            
            if (host == null)
            {
                host = new HostService();
            }
            return host;
        }
    }
}

