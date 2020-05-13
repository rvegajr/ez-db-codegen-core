using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Classes
{
    internal static class SettingsExtention {
        /// <summary>
        /// Gets the setting from file. For format that string must be in should be @{FILENAME}>{XPATH} 
        /// Where file name is the settings file in either XML or JSON.  for example @C:/Inetpub/wwwroot/web.config>/xpath/@attribute
        /// </summary>
        /// <returns>if the string isng in the pattern noted (with @ and >), then it will return itself, otherwise it will return the value if found</returns>
        public static string SettingResolution(this string fileName)
        {
            return SettingsHelper.GetSettingFromFile(fileName);
        }
    }
    internal class SettingsHelper
    {
        public SettingsHelper()
        {

        }
        /// <summary>
        /// Gets the setting from file.
        /// </summary>
        /// <param name="fileparm">For format of this parm needs to be @{FILENAME}>{XPATH} Where file name is the settings file in 
        /// either XML or JSON.  for example @C:/Inetpub/wwwroot/web.config>/ </param>
        /// <returns>if the string can't result, then it will return itself, otherwise </returns>
        public static string GetSettingFromFile(string fileparm)
        {
            var retValue = fileparm;
            if ((fileparm.StartsWith("@")) && (fileparm.Contains(">")))
            {
                fileparm = fileparm.Substring(1);
                var arrParts = fileparm.Split('>');
                var filename = arrParts[0].Trim();
                var xpath = arrParts[1].Trim();
                SettingsHelper settings = new SettingsHelper();
                settings.AppSettingsFileName = filename;

                /*Using Connection string Shortcut Capital "CS" will translate to /configuration/connectionStrings/add[@name='DatabaseContext']/@connectionString
                 * The syntax is CS[SETTINGNAMEHERE], so the example above would be CS['SETTINGNAMEHERE'] 
                 * XML:
                 * Using Connection string Shortcut Capital "AS" will translate to /configuration/appSettings/add[@key='DatabaseContext']/@value
                 * JSON:
                 * Using Connection string Shortcut Capital "AS" will translate to /root/DefaultSettings/Settings/XXXX where XXXX = ConnectionString
                 */
                if (xpath.StartsWith("CS"))
                {
                    xpath = xpath.Replace("CS[", "/configuration/connectionStrings/add[@name='");
                    xpath = xpath.Replace("]", "']/@connectionString");
                } else if (xpath.StartsWith("AS"))
                {
                    if (settings.isJson)
                    {
                        xpath = xpath.Replace("AS[", "/root/DefaultSettings/Settings/");
                        xpath = xpath.Replace("]", "");

                    }
                    else if (settings.isXml)
                    {
                        xpath = xpath.Replace("AS[", "/configuration/appSettings/add[@key='");
                        xpath = xpath.Replace("]", "']/@value");

                    }
                }
                retValue = settings.FindValue(xpath);
            }
            return retValue;
        }
        private string appSettingsFileName = "";
        public bool isJson = false;
        public bool isXml = false;
        /// <summary>
        /// Gets or sets the name of the source file.  This will also cause the reload of the config file
        /// </summary>
        /// <value>
        /// The name of the source file.
        /// </value>
        public string AppSettingsFileName
        {
            get
            {
                return appSettingsFileName;
            }
            set
            {
                appSettingsFileName = value;
                ParseAppSettingsFileName(appSettingsFileName);
            }
        }

        /// <summary>
        /// Finds the value using XPath and will return what is in InnerText.  Please note that if you are reading a JSON Setting file, the encompassing node will be 'root'
        /// </summary>
        /// <param name="XPath">The xpath.</param>
        /// <returns></returns>
        public string FindValue(string XPath)
        {
            if (xmldoc == null) return "";
            var xnodes = xmldoc.SelectNodes(XPath);
            var returnValue = "";
            if (xnodes.Count > 0)
            {
                var firstNode = xnodes[0];
                returnValue = firstNode.InnerText;
            }
            return returnValue;
        }

        /// <summary>
        /// Finds the value using XPath and will return what is in InnerText.  Please note that if you are reading a JSON Setting file, the encompassing node will be 'root'
        /// </summary>
        /// <param name="XPath">The xpath.</param>
        /// <returns></returns>
        public string FindValue(string XPath, string AttributeName)
        {
            if (xmldoc == null) return "";
            var xnodes = xmldoc.SelectNodes(XPath);
            var returnValue = "";
            if (xnodes.Count > 0)
            {
                var firstNode = xnodes[0];
                returnValue = ((XmlElement)firstNode).Attributes[AttributeName].Value;
            }
            return returnValue;
        }

        private XmlDocument xmldoc = null;
        private bool ParseAppSettingsFileName(string FileName)
        {
            try
            {
                isJson = false;
                isXml = false;
                var ext = System.IO.Path.GetExtension(FileName).ToLower();
                var content = System.IO.File.ReadAllText(FileName);
                if (ext.EndsWith("json"))
                {
                    isJson = true;
                    xmldoc = JsonConvert.DeserializeXNode(content, "root").ToXmlDocument();
                }
                else if ((ext.EndsWith("xml")) || (ext.EndsWith("config")))
                {
                    isXml = true;
                    xmldoc = new XmlDocument();
                    xmldoc.LoadXml(content);
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
    internal static class DocumentExtensions
    {
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }
    }
}
