using System;
using System.Reflection;

namespace SignEdgeService
{
	public static class DirectoryExtension
	{
        public static string GetRootDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException();
            }
            try
            {
                var parent = Directory.GetParent(path);
                string? assembly = Assembly.GetExecutingAssembly().GetName().Name;
                if (parent == null || assembly == null)
                {
                    return path;
                }
                if (parent.Name == assembly)
                {
                    return parent.FullName;
                }
                else
                {
                    return GetRootDirectory(parent.FullName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}

