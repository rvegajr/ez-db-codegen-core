using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace EzDbCodeGen.Core.Classes
{
    public class SettingsHelper
    {
        public SettingsHelper()
        {

        }


        private string appSettingsFileName = "";
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
                var ext = System.IO.Path.GetExtension(FileName).ToLower();
                var content = System.IO.File.ReadAllText(FileName);
                if (ext.EndsWith("json"))
                {
                    xmldoc = JsonConvert.DeserializeXNode(content, "root").ToXmlDocument();
                }
                else if ((ext.EndsWith("xml")) || (ext.EndsWith("config")))
                {
                    xmldoc = new XmlDocument();
                    xmldoc.LoadXml(content);
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
            return false;
        }
    }
    public static class DocumentExtensions
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
