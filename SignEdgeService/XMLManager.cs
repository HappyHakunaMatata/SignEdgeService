using System;
using System.IO;
using System.Xml;


namespace SignEdgeService
{
	public class XMLManager
	{
        private XmlDocument xml;

        public XMLManager(string path)
        {
            try
            {
                xml = new XmlDocument();
                if (File.Exists(path))
                {
                    xml.Load(path);
                }
                else
                {
                    xml = CreateNewConfig();
                }
            }
            catch
            {
                throw;
            }
        }


        public XMLManager()
		{
            try
            {
                xml = new XmlDocument();
                var filepath = Path.Combine(DirectoryExtension.GetRootDirectory(Directory.GetCurrentDirectory()), "App.config");
                if (File.Exists(filepath))
                {
                    xml.Load(filepath);
                }
                else
                {
                    xml = CreateNewConfig();
                }
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XmlDocument CreateNewConfig()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(xmlDeclaration);
                XmlElement configurationElement = doc.CreateElement("configuration");
                doc.AppendChild(configurationElement);
                XmlElement appSettingsElement = doc.CreateElement("appSettings");
                configurationElement.AppendChild(appSettingsElement);
                xml = doc;
                return xml;
            }
            catch
            {
                throw;
            }
        }

        
        public string? GetValue(string key)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(key);
            if (xml == null)
            {
                return null;
            }
            try
            {
                var elements = xml.GetElementsByTagName("add");
                var result = elements.Cast<XmlNode>()
                .Reverse()
                .SelectMany(node =>
                {
                    var attributes = node.Attributes;
                    if (attributes != null)
                    {
                        if (attributes.Cast<XmlAttribute>()
                            .Any(attribute =>
                                attribute.Name == "key" && attribute.Value == key))
                        {
                            return attributes.Cast<XmlAttribute>().Select(attribute =>
                                attribute).Where(attribute =>
                                attribute.Name == "value");
                        }
                    }
                    return Enumerable.Empty<XmlAttribute>();
                }).First().Value;

                if (string.IsNullOrEmpty(result))
                {
                    return "";
                }
                return result;
            }
            catch
            {
                throw;
            }
        }


        public bool CheckIsExist(string key)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(key);
            if (xml == null)
            {
                return false;
            }
            try
            {
                var addElements = xml.GetElementsByTagName("add");
                var result = addElements.Cast<XmlNode>().Any(node =>
                {
                    var attributes = node.Attributes;
                    if (attributes != null)
                    {
                        return attributes.Cast<XmlAttribute>().Any(attribute =>
                            attribute.Name == "key" && attribute.Value == key);
                    }
                    return false;
                });
                return result;
            }
            catch
            {
                throw;
            }
        }

        public void SaveConfig(string path, string filename)
		{
            ArgumentNullException.ThrowIfNullOrEmpty(path);
            ArgumentNullException.ThrowIfNullOrEmpty(filename);
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                xml.Save($"{Path.Combine(path, filename)}");
            }
            catch
            {
                throw;
            }
        }



        public XmlDocument Add(string key, string value)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNullOrEmpty(value);
            try
            {
                XmlElement element = xml.CreateElement("add");
                element.SetAttribute("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
                element.SetAttribute("key", key);
                element.SetAttribute("value", value);
                XmlNode? root = xml.DocumentElement;
                if(root == null)
                {
                    return xml;
                }
                XmlNode? appSettingsNode = root.SelectSingleNode("appSettings");
                appSettingsNode?.AppendChild(element);
                return xml;
            }
            catch
            {
                throw;
            }
        }


        public XmlDocument? LoadXML(string path)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path);
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }
                xml.Load(path);
                return xml;
            }
            catch
            {
                throw;
            }
        }
    }
}

