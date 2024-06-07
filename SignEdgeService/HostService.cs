using System;
using SignEdgeService.Controllers;
using SignEdgeService.Interfaces;

namespace SignEdgeService
{
	public class HostService: IHostService
    {
        private CancellationTokenSource cancellationTokenSource;

        public HostService()
		{
            cancellationTokenSource = new CancellationTokenSource();
        }


		public async Task RunHost(string[]? args)
		{
            string? token = System.Configuration.ConfigurationManager.AppSettings["token"];
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("No token configured!");
                return;
            };
            var path = Directory.GetCurrentDirectory();
            if (!Directory.Exists(Path.Combine(path, "wwwroot")))
            {
                path = DirectoryExtension.GetRootDirectory(path);
            }
            if (!File.Exists(Path.Combine(path, "AuthKey.txt")))
            {
                Console.WriteLine("No AuthKey.txt configured!");
                return;
            };
            Console.WriteLine($"Started with token: {token}");
            await Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseWebRoot(Path.Combine(path, "wwwroot"));
            webBuilder.UseSetting("token", token);
            webBuilder.UseStartup<Startup>();
        })
        .Build()
        .RunAsync(cancellationTokenSource.Token);
        }

        public bool Cancel()
        {
            try
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return true;
                }
                cancellationTokenSource.Cancel();
                return true;
            }
            catch
            {
                throw;
            }
        }

        private bool _disposed = false;
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                cancellationTokenSource.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task RestartHost(string[]? args)
        {
            Cancel();
            this.cancellationTokenSource = new CancellationTokenSource();
            await RunHost(args);
        }


    }
}

