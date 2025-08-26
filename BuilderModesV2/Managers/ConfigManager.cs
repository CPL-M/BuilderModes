using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BuilderModesV2.Managers
{
    public class ConfigManager
    {
        public static void UpdateConfig()
        {
            if (File.Exists(Main.Instance.configPath))
            {
                XmlDocument configXml = new XmlDocument();
                configXml.Load(Main.Instance.configPath);

                XmlNode root = configXml.DocumentElement;
                var defaultConfig = new Config();
                defaultConfig.LoadDefaults();

                var properties = typeof(Config).GetProperties();
                foreach (var prop in properties)
                {
                    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        // Handle dynamic lists separately
                        HandleDynamicList(configXml, root, prop.Name, (IEnumerable)prop.GetValue(defaultConfig));
                    }
                    else
                    {
                        string nodeName = prop.Name;
                        string defaultValue = prop.GetValue(defaultConfig)?.ToString();
                        UpdateMissingNode(configXml, root, nodeName, defaultValue);
                    }
                }

                configXml.Save(Main.Instance.configPath);
                Main.Instance.Configuration.Load();
            }
        }
        private static void HandleDynamicList(XmlDocument configXml, XmlNode root, string listName, IEnumerable defaultValues)
        {
            XmlNode listNode = configXml.SelectSingleNode($"//{listName}");
            if (listNode == null)
            {
                XmlElement newListNode = configXml.CreateElement(listName);
                foreach (var item in defaultValues)
                {
                    XmlElement newElement = configXml.CreateElement("Id");
                    newElement.InnerText = "====== NEEDS CHANGED ======";
                    newListNode.AppendChild(newElement);
                }
                root.AppendChild(newListNode);
            }
        }
        public static void UpdateMissingNode(XmlDocument configXml, XmlNode root, string nodeName, string defaultValue)
        {
            XmlNode node = configXml.SelectSingleNode($"//{nodeName}");
            if (node == null)
            {
                XmlElement newElement = configXml.CreateElement(nodeName);
                newElement.InnerText = "====== NEEDS CHANGED ======";

                XmlNode refNode = null;
                foreach (XmlNode childNode in root.ChildNodes)
                {
                    if (string.Compare(childNode.Name, nodeName, StringComparison.Ordinal) > 0)
                    {
                        refNode = childNode;
                        break;
                    }
                }

                if (refNode != null)
                {
                    root.InsertBefore(newElement, refNode);
                }
                else
                {
                    root.AppendChild(newElement);
                }
            }
        }
    }
}
