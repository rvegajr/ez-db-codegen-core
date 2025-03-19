using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EzDbCodeGen.Core.Classes
{
    public static class SettingsHelper
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
        /// Loads settings from a file. The file can be either JSON or XML format.
        /// </summary>
        /// <typeparam name="T">The type of settings object to deserialize</typeparam>
        /// <param name="filePath">Path to the settings file</param>
        /// <returns>The deserialized settings object</returns>
        public static T LoadSettings<T>(string filePath) where T : class, new()
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Settings file not found: {filePath}");

            var extension = Path.GetExtension(filePath).ToLower();
            var content = File.ReadAllText(filePath);

            return extension switch
            {
                ".json" => JsonConvert.DeserializeObject<T>(content, DefaultJsonSettings) ?? new T(),
                ".xml" => LoadXmlSettings<T>(content),
                _ => throw new NotSupportedException($"Unsupported file extension: {extension}")
            };
        }

        /// <summary>
        /// Saves settings to a file in either JSON or XML format.
        /// </summary>
        /// <typeparam name="T">The type of settings object to serialize</typeparam>
        /// <param name="settings">The settings object to save</param>
        /// <param name="filePath">Path where to save the settings file</param>
        /// <param name="asXml">If true, save as XML; if false, save as JSON</param>
        public static void SaveSettings<T>(T settings, string filePath, bool asXml = false) where T : class
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var content = asXml
                ? ConvertToXml(settings)
                : JsonConvert.SerializeObject(settings, DefaultJsonSettings);

            File.WriteAllText(filePath, content);
        }

        private static T LoadXmlSettings<T>(string xmlContent) where T : class, new()
        {
            var xDoc = XDocument.Parse(xmlContent);
            var jsonContent = ConvertXmlToJson(xDoc);
            return JsonConvert.DeserializeObject<T>(jsonContent, DefaultJsonSettings) ?? new T();
        }

        private static string ConvertToXml<T>(T obj) where T : class
        {
            var json = JsonConvert.SerializeObject(obj, DefaultJsonSettings);
            var jsonObj = JObject.Parse(json);
            var xDoc = new XDocument();
            var root = new XElement(typeof(T).Name);
            ConvertJsonToXml(jsonObj, root);
            xDoc.Add(root);
            return xDoc.ToString();
        }

        private static void ConvertJsonToXml(JToken token, XElement parent)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var property in ((JObject)token).Properties())
                    {
                        var element = new XElement(property.Name);
                        ConvertJsonToXml(property.Value, element);
                        parent.Add(element);
                    }
                    break;

                case JTokenType.Array:
                    foreach (var item in ((JArray)token))
                    {
                        var element = new XElement("Item");
                        ConvertJsonToXml(item, element);
                        parent.Add(element);
                    }
                    break;

                default:
                    parent.Value = token.ToString();
                    break;
            }
        }

        private static string ConvertXmlToJson(XDocument xDoc)
        {
            var jsonObj = new JObject();
            foreach (var element in xDoc.Root?.Elements() ?? Enumerable.Empty<XElement>())
            {
                ConvertXmlElementToJson(element, jsonObj);
            }
            return jsonObj.ToString();
        }

        private static void ConvertXmlElementToJson(XElement element, JObject parent)
        {
            if (!element.HasElements)
            {
                parent[element.Name.LocalName] = element.Value;
                return;
            }

            if (element.Elements().All(e => e.Name.LocalName == "Item"))
            {
                var array = new JArray();
                foreach (var item in element.Elements())
                {
                    var obj = new JObject();
                    ConvertXmlElementToJson(item, obj);
                    array.Add(obj);
                }
                parent[element.Name.LocalName] = array;
            }
            else
            {
                var obj = new JObject();
                foreach (var child in element.Elements())
                {
                    ConvertXmlElementToJson(child, obj);
                }
                parent[element.Name.LocalName] = obj;
            }
        }
    }
}
