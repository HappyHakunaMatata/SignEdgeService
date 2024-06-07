using System;
using Certes;
using Certes.Acme;


namespace SignEdgeService.Interfaces
{
	public interface IACMEService
    {
        public string SetSavePath(string path);

        public bool LoadPemKey(string path);

        public AcmeContext? LoadACMEContext();

        public Task<string?> CreateACMEAccount(string email);

        public bool LoadLocation(string path);

        public bool LoadIOrderContext();

        public Task<bool> SetupDNSValidation();

        public Task<IOrderContext?> CreateNewOrder(string domain);

        public Task<bool> SetupHttpValidation();
        
        public Task<Certes.Acme.Resource.Challenge?> ValidateDomainOwnership();

        public void PrintFiles();
        
        public Task<bool> DownloadCertificate();

        public void PrintFile(string path);

        public bool UpdateConfigFile();

        public bool UpdateConfigFile(string token);

        public bool SetToken(string token);

        public string CopyAuthKey(string path);

        public bool CheckIfConfigured(string value);

        public bool LoadAppConfigFile(string path);

        public bool SetChallengeType(ChallengeTypes type);

        public Task<bool> Revoke(string CertPem, string PemKey);

        public bool ShareCertificate(string? workdir = null);
    }
}

