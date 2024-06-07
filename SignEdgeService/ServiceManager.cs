using System.IO;
using Certes;
using SignEdgeService.Interfaces;

namespace SignEdgeService
{
	public class ServiceManager
	{
        private IACMEService? _acme = null;
        private IHostService? _host = null;


        public IACMEService ACME
        {
            get
            {
                if (_acme == null)
                {
                    throw new ArgumentNullException();
                }
                return _acme;
            }
            set
            {
                _acme = value;
            }
        }

        public IHostService Host
        {
            get
            {
                if (_host == null)
                {
                    throw new ArgumentNullException();
                }
                return _host;
            }
            set
            {
                _host = value;
            }
        }

        public async Task RunHost()
        {
            try
            {
                await Host.RunHost(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task RestartHost()
        {
            try
            {
                await Host.RestartHost(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool Cancel()
        {
            try
            {
                return Host.Cancel();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<string?> CreateACMEAccount(string email)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(email);
            try
            {
                
                return await ACME.CreateACMEAccount(email);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }



        public async Task<bool> CreateNewOrder(string domain)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(domain);
            try
            {
                var result = await ACME.CreateNewOrder(domain);
                return result != null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<bool> SetupDNSValidation()
        {
            try
            {
                return await ACME.SetupDNSValidation();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<bool> SetupHttpValidation()
        {
            try
            {
                return await ACME.SetupHttpValidation();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<Certes.Acme.Resource.Challenge?> ValidateDomainOwnership()
        {
            try
            {
                return await ACME.ValidateDomainOwnership();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<bool> DownloadCertificate()
        {
            try
            {
                return await ACME.DownloadCertificate();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }


        public string SetSavePath(string path)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path);
            try
            {
                return ACME.SetSavePath(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }


        public bool LoadPemKey(string filename)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(filename);
            try
            {
                return ACME.LoadPemKey(filename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public AcmeContext? LoadACMEContext()
        {
            try
            {
                return ACME.LoadACMEContext();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public bool LoadLocation(string path)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path);
            try
            {
                return ACME.LoadLocation(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool LoadIOrderContext()
        {
            try
            {
                return ACME.LoadIOrderContext();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void PrintFiles()
        {
            try
            {
                ACME.PrintFiles();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool UpdateConfigFile()
        {
            try
            {
                return ACME.UpdateConfigFile();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool UpdateConfigFile(string token)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(token);
            try
            {
                return ACME.UpdateConfigFile(token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool SetToken(string token)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(token);
            try
            {
                return ACME.SetToken(token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool CheckIfConfigured(string value)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(value);
            try
            {
                return ACME.CheckIfConfigured(value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public string CopyAuthKey(string path)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path);
            try
            {
                return ACME.CopyAuthKey(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }

        public void PrintFile(string path)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path);
            try
            {
                ACME.PrintFile(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool SetChallengeType(ChallengeTypes type)
        {
            try
            {
                return ACME.SetChallengeType(type);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<bool> RevokeCertificate(string CertFilePath, string KeyFilePath)
        {
            try
            {
                var result = await ACME.Revoke(CertFilePath, KeyFilePath);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool ShareCertificate(string? workdir = null)
        {
            try
            {
                var result = ACME.ShareCertificate(workdir);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}

