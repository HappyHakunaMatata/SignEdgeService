using System.CommandLine;
using SignEdgeService;


class Program
{
    static private readonly RootCommand rootCommand = new RootCommand();
    static private readonly ConsoleReader reader = new ConsoleReader();
    
    
    public static void Main(string[] args)
    {
        ServiceManager manager = new ServiceManager();
        manager.Host = ServiceLocator.Instance.GetIIHostService();
        manager.ACME = ServiceLocator.Instance.GetIACMELocator();
        #region Setup Options
        var PathOption = new Option<string>(
            name: "-path",
            description: "Sets work directory path")
        { IsRequired = false, AllowMultipleArgumentsPerToken = false };

        var RegisterOption = new Option<string>(
            name: "-email",
            description: "Email for registration")
        { IsRequired = true, AllowMultipleArgumentsPerToken = false };

        var CreateNewOrderOption = new Option<string>(
            name: "-domain",
            description: "Domain name for dns. Example: *.your.domain.name")
        { IsRequired = true, AllowMultipleArgumentsPerToken = false };

        var TokenOption = new Option<string>(
            name: "-token",
            description: "Sets token value")
        { IsRequired = false, AllowMultipleArgumentsPerToken = false };

        var FilepathOption = new Option<string>(
          name: "-filepath",
          description: "Path to file.");

        var KeypathOption = new Option<string>(
          name: "-key",
          description: "Path to key file.");

        #region Load command options

        var PemKeyOption = new Option<string>(
            name: "-pem",
            description: "Loads pem filepath for login.")
        { IsRequired = false, AllowMultipleArgumentsPerToken = false };

        var ACMEOption = new Option<bool>(
           name: "-acme",
           description: "Loads ACME context. Pem must be configured before usage.");

        var LocationOption = new Option<string>(
           name: "-location",
           description: "Loads location path.");

        var IOrderOption = new Option<bool>(
           name: "-order",
           description: "Loads order context. ACME context and location must be configured before usage.");

        var httpOption = new Option<bool>(
          name: "-http",
          description: "Specifies http challenge option");

        var dnsOption = new Option<bool>(
          name: "-dns",
          description: "Specifies dns challenge option");

        #endregion
        #endregion
        #region Setup Commands
        var RunHostCommand = new Command(
            name: "Run",
            description: "Run host");

        var CancelHostCommand = new Command(
            name: "Cancel",
            description: "Cancel host");

        var RestartHostCommand = new Command(
            name: "Restart",
            description: "Restart host");

        var SetupHostCommand = new Command(
           name: "Setup",
           description: "Setup host");

        var CopyAuthFile = new Command(
           name: "Copy",
           description: "Copy auth file to host work directory");

        var PrintCommand = new Command(
            name: "Print",
            description: "Print all files in work directory"
            );

        var PrintfileCommand = new Command(
            name: "Printfile",
            description: "Print content of file"
            );

        var RegisterCommand = new Command(
            name: "Create",
            description: "Create ACME account with specific email")
        {
            RegisterOption
        };

        var LoginCommand = new Command(
            name: "Login",
            description: "Login into ACME account")
        {};

        var CreateNewOrderCommand = new Command(
            name: "Generate",
            description: "Creates order record")
        {
            CreateNewOrderOption
        };

        var SetupDNSValidationCommand = new Command(
           name: "DNSValidation",
           description: "Setup DNS validation for your order"
           );

        var SetupHTTPValidationCommand = new Command(
           name: "HTTPValidation",
           description: "Setup HTTP validation for your order"
           );

        var ValidateCommand = new Command(
            name: "Validate",
            description: "Validates domain ownership"
            );

        var DownloadCertificateCommand = new Command(
            name: "Download",
            description: "Downloads created certificate"
            );

        var LoadCommand = new Command(
            name: "Load",
            description: "Loads specific data for ACME creation."
            );

        var UpdateCommand = new Command(
            name: "Update",
            description: "Updates app.config file. You should use it when token is configured."
            )
        {TokenOption};

        var SetChallengeCommand = new Command(
            name: "Challenge",
            description: "Set challenge option for validation. Http challenge is default"
            )
        {httpOption, dnsOption};

        var SetRevokeCommand = new Command(
            name: "Revoke",
            description: "Revoke your certificate"
            )
        {FilepathOption, KeypathOption};

        var ShareCertificateCommand = new Command(
            name: "Share",
            description: "Copies your certificate to public volume. By default: /etc/certificate"
            )
        {FilepathOption};

        #endregion
        #region Add Commands
        LoadCommand.AddOption(PemKeyOption);
        LoadCommand.AddOption(ACMEOption);
        LoadCommand.AddOption(LocationOption);
        LoadCommand.AddOption(IOrderOption);


        CopyAuthFile.AddOption(FilepathOption);
        PrintfileCommand.AddOption(FilepathOption);


        SetupHostCommand.AddCommand(SetChallengeCommand);
        SetupHostCommand.AddCommand(RegisterCommand);
        SetupHostCommand.AddCommand(LoginCommand);
        SetupHostCommand.AddCommand(CreateNewOrderCommand);
        SetupHostCommand.AddCommand(SetupHTTPValidationCommand);
        SetupHostCommand.AddCommand(SetupDNSValidationCommand);
        SetupHostCommand.AddCommand(UpdateCommand);
        SetupHostCommand.AddCommand(CopyAuthFile);
        SetupHostCommand.AddCommand(ValidateCommand);
        SetupHostCommand.AddCommand(DownloadCertificateCommand);
        SetupHostCommand.AddCommand(ShareCertificateCommand);
        SetupHostCommand.AddCommand(SetRevokeCommand);

        
        SetupHostCommand.AddOption(PathOption);
        SetupHostCommand.AddOption(TokenOption);


        rootCommand.AddCommand(PrintCommand);
        rootCommand.AddCommand(PrintfileCommand);
        rootCommand.AddCommand(SetupHostCommand);
        rootCommand.AddCommand(LoadCommand);
        rootCommand.AddCommand(RunHostCommand);
        rootCommand.AddCommand(CancelHostCommand);
        rootCommand.AddCommand(RestartHostCommand);
        #endregion
        #region Setup Handlers
        RunHostCommand.SetHandler(async (args) =>
        {
            await manager.RunHost();
        });


        SetupHostCommand.SetHandler((path, token) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                var workdir = manager.SetSavePath(path);
                if (string.IsNullOrEmpty(workdir))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Path has not been set");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"Default path: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{workdir}\n");
                }
            }
            if (!string.IsNullOrEmpty(token))
            {
                var result = manager.SetToken(token);
                Console.WriteLine(result ? "Token has been set" : "Token has not been set");
            }
        }, PathOption, TokenOption);


        RegisterCommand.SetHandler(async (email) =>
        {
            var result = await manager.CreateACMEAccount(email);
            if (string.IsNullOrEmpty(result))
            {
                Console.WriteLine("Account has not been created");
                return;
            }
            Console.WriteLine(result);
        }, RegisterOption);


        CreateNewOrderCommand.SetHandler(async (domain) =>
        {
            var result = await manager.CreateNewOrder(domain);
            Console.WriteLine(result ? "Order has been created" : "Order has not been created");
        }, CreateNewOrderOption);


        LoadCommand.SetHandler((pem, acme, location, order) =>
        {
            if (!string.IsNullOrEmpty(pem))
            {
                var result = manager.LoadPemKey(pem);
                Console.WriteLine(result ? "PEM has been loaded" : "PEM has not been loaded");
            }
            if (acme)
            {
                var result = manager.LoadACMEContext();
                Console.WriteLine(result != null ? "ACME context has been loaded" : "ACME context has not been loaded");
            }
            if (!string.IsNullOrEmpty(location))
            {
                var result = manager.LoadLocation(location);
                Console.WriteLine(result ? $"Location has been loaded" : "ACME context has not been loaded");
            }
            if (order)
            {
                var result = manager.LoadIOrderContext();
                Console.WriteLine(result ? $"Order context has been loaded" : "Order context has not been loaded");
            }
            
        }, PemKeyOption, ACMEOption, LocationOption, IOrderOption);
        

        PrintCommand.SetHandler(manager.PrintFiles);


        DownloadCertificateCommand.SetHandler(async () =>
        {
            var result = await manager.DownloadCertificate();
            Console.WriteLine(result ? $"Certificate has been downloaded" : "Certificate has not been downloaded");
        });


        SetupDNSValidationCommand.SetHandler(async () =>
        {
            var result = await manager.SetupDNSValidation();
            Console.WriteLine(result ? $"DNS validation has been setup" : "DNS validation has not been setup");
        });


        SetupHTTPValidationCommand.SetHandler(async () =>
        {
            var result = await manager.SetupHttpValidation();
            Console.WriteLine(result ? $"HTTP validation has been setup" : "HTTP validation has not been setup");
        });


        ValidateCommand.SetHandler(async () =>
        {
            var result = await manager.ValidateDomainOwnership();
            if (result != null)
            {
                var error = result.Error;
                if (error != null)
                {
                    Console.WriteLine(result.Error.Detail);
                }
                Console.WriteLine(result.Status);
                Console.WriteLine($"Validated :{result.Validated}");
            }
        });


        UpdateCommand.SetHandler((token) =>
        {
            if (string.IsNullOrEmpty(token))
            {
                var updated = manager.UpdateConfigFile();
                Console.WriteLine(updated ? $"Config has been updated" : "Config has not been updated");
                return;
            }
            var result = manager.UpdateConfigFile(token);
            Console.WriteLine(result ? $"Token {token} has been set" : "Token has not been set");
        }, TokenOption);


        LoginCommand.SetHandler(() =>
        {
            var result = manager.LoadACMEContext();
            Console.WriteLine(result != null ? $"Logged successfully" : "Login has not been proceed");
        });


        CopyAuthFile.SetHandler((path) =>
        {
            var result = manager.CopyAuthKey(path);
            Console.WriteLine(!string.IsNullOrEmpty(result) ? $"File has been copied to: {path}" : $"File not been copied");
        }, FilepathOption);


        PrintfileCommand.SetHandler((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                manager.PrintFile(path);
            }
        }, FilepathOption);


        CancelHostCommand.SetHandler(() =>
        {
            var result = manager.Cancel();
            Console.WriteLine(result ? "Host has been closed" : "Host has not been closed");
        });


        RestartHostCommand.SetHandler(async () =>
        {
            await manager.RestartHost();
        });


        SetChallengeCommand.SetHandler((http, dns) =>
        {
            if (http)
            { 
                var result = manager.SetChallengeType(ChallengeTypes.http);
                Console.WriteLine(result ? "Challenge has been set to http" : "Challenge type has not been set");
                return;
            }
            else if (dns)
            {
                var result = manager.SetChallengeType(ChallengeTypes.dns);
                Console.WriteLine(result ? "Challenge has been set to dns" : "Challenge type has not been set");
                return;
            }
        }, httpOption, dnsOption);


        ShareCertificateCommand.SetHandler((pathOption) =>
        {
            var result = manager.ShareCertificate(pathOption);
            Console.WriteLine(result ? "The certificate has been shared" : "The certificate has not been shared");
        }, FilepathOption);


        SetRevokeCommand.SetHandler(async (pathOption, KeypathOption) =>
        {
            var result = await manager.RevokeCertificate(pathOption, KeypathOption);
            Console.WriteLine(result ? "The certificate has been revoked" : "The certificate has not been revoked");
        }, FilepathOption, KeypathOption);
        #endregion
        if (args.Length > 0)
        {
            rootCommand.Invoke(args);
        }

        reader.ProcessCompleted += ProcessCompletedEvent;
        reader.StartReadConsole();
    }

    static void ProcessCompletedEvent(object? sender, string[]? args)
    {
        if (args == null)
        {
            return;
        }
        if (args.First() == "Run" || args.First() == "Restart")
        {
            rootCommand.InvokeAsync(args);
        }
        else
        {
            rootCommand.Invoke(args);
        }
    }
}

