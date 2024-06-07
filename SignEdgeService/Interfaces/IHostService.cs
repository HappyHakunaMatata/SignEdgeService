using System;
namespace SignEdgeService.Interfaces
{
	public interface IHostService
	{
        public Task RunHost(string[]? args);

        public bool Cancel();

        public Task RestartHost(string[]? args);
    }
}

