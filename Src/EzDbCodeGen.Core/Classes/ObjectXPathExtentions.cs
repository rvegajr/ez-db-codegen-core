using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EzDbCodeGen.Core.Classes
{
    public static class ObjectXPathExtensions
    {
        private static readonly JsonSerializerSettings DefaultJsonSettings = new()
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        /// <summary>
        /// Converts an object to XML and evaluates an XPath expression against it.
        /// </summary>
        /// <param name="obj">The object to evaluate.</param>
        /// <param name="xpath">The XPath expression.</param>
        /// <returns>The value found at the XPath location, or null if not found.</returns>
        public static string? EvaluateXPath(this object obj, string xpath)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (string.IsNullOrEmpty(xpath))
            {
                throw new ArgumentException("XPath expression cannot be null or empty", nameof(xpath));
            }

            var json = JsonConvert.SerializeObject(obj, DefaultJsonSettings);
            var jsonObj = JObject.Parse(json);
            var xml = ConvertJsonToXml(jsonObj);

            try
            {
                return xml.XPathSelectElement(xpath)?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Evaluates multiple XPath expressions against an object and returns a dictionary of results.
        /// </summary>
        /// <param name="obj">The object to evaluate.</param>
        /// <param name="xpaths">The XPath expressions to evaluate.</param>
        /// <returns>A dictionary containing the XPath expressions and their corresponding values.</returns>
        public static Dictionary<string, string?> EvaluateXPaths(this object obj, IEnumerable<string> xpaths)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (xpaths == null)
            {
                throw new ArgumentNullException(nameof(xpaths));
            }

            var results = new Dictionary<string, string?>();
            var json = JsonConvert.SerializeObject(obj, DefaultJsonSettings);
            var jsonObj = JObject.Parse(json);
            var xml = ConvertJsonToXml(jsonObj);

            foreach (var xpath in xpaths)
            {
                if (string.IsNullOrEmpty(xpath)) continue;

                try
                {
                    var value = xml.XPathSelectElement(xpath)?.Value;
                    results[xpath] = value;
                }
                catch
                {
                    results[xpath] = null;
                }
            }

            return results;
        }

        private static XDocument ConvertJsonToXml(JToken token)
        {
            var doc = new XDocument();
            var root = new XElement("root");
            ConvertJsonNodeToXml(token, root);
            doc.Add(root);
            return doc;
        }

        private static void ConvertJsonNodeToXml(JToken token, XElement parent)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var property in ((JObject)token).Properties())
                    {
                        var element = new XElement(property.Name);
                        ConvertJsonNodeToXml(property.Value, element);
                        parent.Add(element);
                    }
                    break;

                case JTokenType.Array:
                    foreach (var item in (JArray)token)
                    {
                        var element = new XElement("item");
                        ConvertJsonNodeToXml(item, element);
                        parent.Add(element);
                    }
                    break;

                case JTokenType.Null:
                    break;

                default:
                    parent.Value = token.ToString();
                    break;
            }
        }
    }
}
