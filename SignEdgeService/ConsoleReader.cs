using System;
using System.CommandLine;
using System.Threading;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting.Server;

namespace SignEdgeService
{

    public class ConsoleReader
	{
		public event EventHandler<string[]?>? ProcessCompleted;

        public void StartReadConsole()
		{
			try
			{
                while (true)
                {
                    var arguments = Console.ReadLine();
                    if (!string.IsNullOrEmpty(arguments))
                    {
                        var args = arguments.Split(" ");
                        OnProcessCompleted(args);
                    }
                    else
                    {
                        break;
                    }
                }
            }
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
                OnProcessCompleted(null);
            }

        }

		protected virtual void OnProcessCompleted(string[]? args)
        {
            ProcessCompleted?.Invoke(this, args);
        }

    }
}

