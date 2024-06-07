using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Certes;
using Certes.Acme;
using SignEdgeService.Interfaces;


namespace SignEdgeService
{
    public class ACMEService : IACMEService
    {
        private readonly XMLManager xmlManager;
        private readonly string DefaultSavePath;
        private string? _path = null;

        private ChallengeTypes type;

        private string BackupPath
        {
            get
            {
                if (_path != null)
                {
                    return _path;
                }
                else
                {
                    return DefaultSavePath;
                }
            }
            set
            {
                _path = value;
            }
        }

        public string SetSavePath(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            BackupPath = path;
            return BackupPath;
        }


        public ACMEService()
		{
            try
            {
                DefaultSavePath = Directory.GetCurrentDirectory();
                xmlManager = new XMLManager();
                type = ChallengeTypes.http;
            }
            catch
            {
                throw;
            }
        }

        public ACMEService(string path)
        {
            try
            {
                DefaultSavePath = Directory.GetCurrentDirectory();
                xmlManager = new XMLManager();
                BackupPath = path;
                type = ChallengeTypes.http;
            }
            catch
            {
                throw;
            }
        }

        public bool SetChallengeType(ChallengeTypes type)
        {
            this.type = type;
            return true;
        }

        public bool LoadAppConfigFile(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            try
            {
                xmlManager.LoadXML(path);
                return true;
            }
            catch
            {
                throw;
            }
        }
       

        private string? _pemKey;
        public string PemKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_pemKey))
                {
                    return _pemKey;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set
            {
                _pemKey = value;
            }
        }

        public bool LoadPemKey(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            try
            {
                var pem = LoadSimpleText(path);
                if (pem != null)
                {
                    PemKey = pem;
                    return true;
                }
                return false;
            }
            catch
            {
                throw;
            }
        }


        public async Task<string?> CreateACMEAccount(string email)
        {
            ArgumentException.ThrowIfNullOrEmpty(email);
            try
            {
                var acme = new AcmeContext(WellKnownServers.LetsEncryptV2);
                await acme.NewAccount(email, true);
                PemKey = acme.AccountKey.ToPem();
                using (var pem = ECDsa.Create())
                {
                    pem.ImportFromPem(PemKey);
                    SaveKeyPem(BackupPath, "AccountKey", pem);
                }
                return PemKey;
            }
            catch
            {
                throw;
            }
        }

        private AcmeContext? _acme;
        public AcmeContext ACME
        {
            get
            {
                if (_acme != null)
                {
                    return _acme;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set
            {
                _acme = value;
            }
        }



        public AcmeContext? LoadACMEContext()
        {
            try
            {
                var accountKey = KeyFactory.FromPem(PemKey);
                ACME = new AcmeContext(WellKnownServers.LetsEncryptV2, accountKey);
                return ACME;
            }
            catch
            {
                throw;
            }
        }

        private IOrderContext? _order;
        public IOrderContext Order
        {
            get
            {
                if (_order != null)
                {
                    return _order;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set
            {
                _order = value;
            }
        }


        private string? _location;
        public string Location
        {
            get
            {
                if (_location != null)
                {
                    return _location;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set
            {
                _location = value;
            }
        }

        public bool LoadLocation(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            try
            {
                var doc = xmlManager.LoadXML(path);
                if (doc == null)
                {
                    return false;
                }
                var result = xmlManager.GetValue("location");
                if (string.IsNullOrEmpty(result))
                {
                    return false;
                }
                Location = result;
                return true;
            }
            catch
            {
                throw;
            }
        }

        public bool LoadIOrderContext()
        {
            try
            {
                if (ACME == null || string.IsNullOrEmpty(Location))
                {
                    return false;
                }
                Order = ACME.Order(new Uri(Location));
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IOrderContext?> CreateNewOrder(string domain)
        {
            ArgumentException.ThrowIfNullOrEmpty(domain);
            if (ACME == null)
            {
                return null;
            }
            try
            {
                Order = await ACME.NewOrder(new[] { domain });
                return Order;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> SetupDNSValidation()
        {
            if (Order == null)
            {
                return false;
            }
            try
            {
                var authz = (await Order.Authorizations()).First();
                var dnsChallenge = await authz.Dns();
                var dnstxt = ACME.AccountKey.DnsTxt(dnsChallenge.Token);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Your DNS Token: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(dnstxt);
                xmlManager.Add("dns", dnstxt);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nYour Location absolute uri: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(Order.Location);
                Console.Write("\n");
                xmlManager.Add("location", Order.Location.AbsoluteUri);
                xmlManager.SaveConfig(BackupPath, "acme.config");
                Location = Order.Location.AbsoluteUri;
                return true;
            }
            catch
            {
                throw;
            }
        }

        

        public string CopyAuthKey(string path)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path);
            try
            {
                var workdir = BackupPath;
                if (!Directory.Exists(Path.Combine(workdir, "wwwroot")))
                {
                    workdir = DirectoryExtension.GetRootDirectory(workdir);
                }

                workdir = Path.Combine(workdir, "wwwroot/Files/");
                if (!Directory.Exists(workdir))
                {
                    Directory.CreateDirectory(workdir);
                }

                var filepath = Path.Combine(workdir, "AuthKey.txt");
                if (CheckIfExistFile(workdir, $"AuthKey.txt"))
                {
                    File.Delete(filepath);
                }
                File.Copy(path, filepath);
                return filepath;
            }
            catch
            {
                throw;
            }
        }
        

        public async Task<bool> SetupHttpValidation()
        {
            if (Order == null)
            {
                return false;
            }
            try
            {
                var authz = (await Order.Authorizations()).First();
                var challengeContext = await authz.Http();

                xmlManager.Add("token", challengeContext.Token);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Your http Token: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{challengeContext.Token}\n");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nYour Location absolute uri: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(Order.Location);
                Console.Write("\n");
                xmlManager.Add("location", Order.Location.AbsoluteUri);
                xmlManager.SaveConfig(BackupPath, "acme.config");
                Location = Order.Location.AbsoluteUri;
                var keyAuthz = challengeContext.KeyAuthz;
                var path = SaveSimpleText(BackupPath, "AuthKey.txt", keyAuthz);
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }
                Console.WriteLine($"File has been saved to: {path}");
                CopyAuthKey(path);
                return true;
            }
            catch
            {
                throw;
            }
        }


        public async Task<Certes.Acme.Resource.Challenge?> ValidateDomainOwnership()
        {
            if (Order == null)
            {
                return null;
            }
            try
            {
                var authz = (await Order.Authorizations()).First();
                IChallengeContext challengeContext;
                if (this.type == ChallengeTypes.http)
                {
                    Console.WriteLine("Using http challenge");
                    challengeContext = await authz.Http();
                }
                else
                {
                    Console.WriteLine("Using dns challenge");
                    challengeContext = await authz.Dns();
                }
                var result = await challengeContext.Validate();
                return result;
            }
            catch
            {
                throw;
            }
        }


        public void PrintFiles()
        {
            try
            {
                if (!Directory.Exists(BackupPath))
                {
                    Console.WriteLine("Directory does not exist!");
                    return;
                }
                string[] files = Directory.GetFiles(BackupPath);
                Console.WriteLine("Files:");
                foreach (string filePath in files)
                {
                    Console.WriteLine(filePath);
                }
            }
            catch
            {
                throw;
            }
        }


        public async Task<bool> DownloadCertificate()
        {
            if (Order == null)
            {
                return false;
            }
            try
            {
                
                var csr = new CsrInfo
                {
                    Organization = "Certes",
                    OrganizationUnit = "Dev",
                    CommonName = "peer.littlemozzarella.com",
                };

                var privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);
                var cert = await Order.Generate(csr, privateKey);
                
                var pem = cert.ToPem();
                using (var certificate = X509Certificate2.CreateFromPem(pem))
                {
                    using (var key = ECDsa.Create())
                    {
                        Console.WriteLine(privateKey.ToPem());
                        key.ImportFromPem(privateKey.ToPem());
                        SaveCACertificatePem(BackupPath, certificate.CopyWithPrivateKey(key));
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public void PrintFile(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            try
            {
                var lines = File.ReadAllLines(path);
                foreach (var i in lines)
                {
                    Console.WriteLine(i);
                }
            }
            catch
            {
                throw;
            }
        }

        public bool UpdateConfigFile()
        {
            try
            {
                string tokenString = "token";
                var token = xmlManager.GetValue(tokenString);
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }
                UpdateConfigFile(token);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public bool SetToken(string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            try
            {
                xmlManager.Add("token", token);
                xmlManager.SaveConfig(BackupPath, "acme.config");
                UpdateConfigFile(token);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public bool CheckIfConfigured(string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            try
            {
                Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                return config.AppSettings.Settings.AllKeys.Contains(value);
            }
            catch
            {
                throw;
            }

        }

        public bool UpdateConfigFile(string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            try
            {
                string tokenString = "token";
                Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (!config.AppSettings.Settings.AllKeys.Contains(tokenString))
                {
                    config.AppSettings.Settings.Add(tokenString, token);
                }
                else
                {
                    config.AppSettings.Settings[tokenString].Value = token;
                }
                config.Save(ConfigurationSaveMode.Modified);
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
                return true;
            }
            catch
            {
                throw;
            }
        }

        public bool CheckIfExistFile(string path, string filename)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            ArgumentException.ThrowIfNullOrEmpty(filename);
            if (System.IO.Directory.GetFiles(path, filename).Length > 0)
            {
                return true;
            }
            return false;
        }

        public string SaveCACertificatePem(string path, X509Certificate2 certificate, X509Certificate2Collection? collection = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            ArgumentNullException.ThrowIfNull(certificate);
            try
            {
                var filename = "ca";
                if (CheckIfExistFile(path, "ca.pem"))
                {
                    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    filename += $".{timestamp}";
                }
                var pk = certificate.GetECDsaPrivateKey();
                var task = Task.Run(async () =>
                {
                    if (pk != null)
                    {
                        await File.WriteAllTextAsync(Path.Combine(path, $"{filename}.key"), pk.ExportPkcs8PrivateKeyPem());
                    }
                });
                var filepath = Path.Combine(path, $"{filename}.pem");
                File.WriteAllText(filepath, certificate.ExportCertificatePem() + "\n");
                if (collection != null)
                {
                    foreach (var cert in collection)
                    {
                        File.AppendAllText(filepath, cert.ExportCertificatePem() + "\n");
                    }
                }
                task.Wait();
                return path;
            }
            catch
            {
                throw;
            }
        }

        public string SaveKeyPem(string path, string filename, ECDsa key)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            ArgumentNullException.ThrowIfNull(key);
            ArgumentException.ThrowIfNullOrEmpty(filename);
            try
            {
                if (Path.HasExtension(filename))
                {
                    if (CheckIfExistFile(path, filename))
                    {
                        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        filename = $"{Path.GetFileNameWithoutExtension(filename)}.{timestamp}{Path.GetExtension(filename)}";
                    }
                }
                else
                {
                    if (CheckIfExistFile(path, $"{filename}.pem"))
                    {
                        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        filename = $"{filename}.{timestamp}.pem";
                    }
                    else
                    {
                        filename = $"{filename}.pem";
                    }
                }
                path = Path.Combine(path, filename);
                File.WriteAllText(path, key.ExportPkcs8PrivateKeyPem() + "\n");
                return path;
            }
            catch
            {
                throw;
            }
        }

        public string? SaveSimpleText(string path, string filename, string text)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            ArgumentException.ThrowIfNullOrEmpty(text);
            ArgumentException.ThrowIfNullOrEmpty(filename);
            try
            {
                if (Path.HasExtension(filename))
                {
                    if (CheckIfExistFile(path, filename))
                    {
                        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        filename = $"{Path.GetFileNameWithoutExtension(filename)}.{timestamp}{Path.GetExtension(filename)}";
                    }
                }
                else
                {
                    if (CheckIfExistFile(path, $"{filename}.txt"))
                    {
                        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        filename = $"{filename}.{timestamp}.txt";
                    }
                    else
                    {
                        filename = $"{filename}.txt";
                    }
                }
                path = Path.Combine(path, filename);
                File.WriteAllText(path, text + "\n");
                return path;
            }
            catch
            {
                throw;
            }
        }

        public string? LoadSimpleText(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            try
            {
                return File.ReadAllText(path);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Revoke(string CertPath, string KeyPath)
        {
            ArgumentException.ThrowIfNullOrEmpty(CertPath);
            ArgumentException.ThrowIfNullOrEmpty(KeyPath);
            if (ACME == null)
            {
                return false;
            }
            try
            {
                var CertPem = await File.ReadAllTextAsync(CertPath);
                using (X509Certificate2 certificate = X509Certificate2.CreateFromPem(CertPem))
                {
                    var Pem = await File.ReadAllTextAsync(KeyPath);
                    var key = KeyFactory.FromPem(Pem);
                    await ACME.RevokeCertificate(certificate.RawData, Certes.Acme.Resource.RevocationReason.KeyCompromise, key);
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        public bool ShareCertificate(string? workdir = null)
        {
            try
            {
                if (string.IsNullOrEmpty(workdir))
                {
                    workdir = Path.Combine(DirectoryExtension.GetRootDirectory(BackupPath), "/etc/certificate");
                }
                if (!Directory.Exists(workdir))
                {
                    return false;
                }
                if (!File.Exists(Path.Combine(BackupPath, "ca.key")) || !File.Exists(Path.Combine(BackupPath, "ca.pem")))
                {
                    return false;
                }
                File.Copy(Path.Combine(BackupPath, "ca.key"), Path.Combine(workdir, "ca.key"));
                File.Copy(Path.Combine(BackupPath, "ca.pem"), Path.Combine(workdir, "ca.pem"));
                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}

