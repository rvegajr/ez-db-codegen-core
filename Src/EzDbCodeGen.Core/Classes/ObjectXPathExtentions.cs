using FastMember;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Xml;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Extentions
{
    internal static class ObjectXPathExtentions
    {
        /// <summary>
        /// Will return a formatted string that is based on Xpath query but adds a couple of extra commands to interrogate different properties of the XmlNode object.  This function is most useful if you want to return
        /// a string list from an array of objects.  This function works by first turning it into a JSON object and then turning it into an XML object. I chose this because XPATH has a richer and more well known syntax
        /// than JsonPath.
        /// </summary>
        /// <typeparam name="T">This allows this to be used with an object passed to it</typeparam>
        /// <param name="_item">The object you want to search and stringify</param>
        /// <param name="xpathPathQueryString">The XPath query that will be used to query the object.  You can end this with a custom Hash string of 1 of 4 values: #Name, #Value, #InnerXml, #InnerText. 
        /// The function will obtain the value from this property of the XmlNode Object and format based on the pattern.  The default property will be InnerText.
        /// If you just include one of the 4 hash parms, then xpathPathQueryString will automatically prepend a "/*/*" which will search only all direct child nodes
        /// If the first character of this string is ">", then it will substitute "/*/*| " which means direct child node
        /// You can chain an xpath to take affect after a selection by using the | operator... so you can do this > | /Name#InnerText will select all child nodes and then get all Name elements and get the InnerText from each child
        /// </param>
        /// <param name="pattern">Default is "%1,%2" where %1=first item, %2 are all subsequent items.. so %1,%2 is comma delimited</param>
        /// <returns></returns>
        public static string ObjectPropertyAsString<T>(this T _item, string xpathPathQueryString= "/*/*#InnerText", string pattern = "%1,%2") where T : new()
        {
            var PROC = string.Format("ObjectXPathExtentions.ObjectPropertyAsString( item=[object {0}], xpathPathQueryString='{1}', pattern={2}, AttributeField={3})", _item.GetType().Name, xpathPathQueryString, pattern, "??");
            var xml = "";
            try
            {
                //if the first character is the direct child short cut ">" and we do not already have the 
                if ((xpathPathQueryString.StartsWith(">")) && (!xpathPathQueryString.StartsWith(">|"))) xpathPathQueryString = ">|" + xpathPathQueryString.Substring(1);
                var xpathPathQueryStringChild = "";
                if (xpathPathQueryString.Contains("|"))
                {
                    var arrXpathPathQueryString = xpathPathQueryString.Split('|');
                    xpathPathQueryString = arrXpathPathQueryString[0].Trim();
                    xpathPathQueryStringChild = arrXpathPathQueryString[1].Trim();
                }
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(_item, Newtonsoft.Json.Formatting.Indented
                    , new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.All,
                        TypeNameHandling = TypeNameHandling.All
                    });
                XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode(json, _item.GetType().Name);
                XmlNode root = doc.DocumentElement;
                xml = doc.OuterXml;

                // Add the namespace.  
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("json", "http://james.newtonking.com/projects/json");

                var AttributeField = "InnerText";
                var _xpathPathQueryString = xpathPathQueryString;
                if (_xpathPathQueryString.StartsWith("#")) _xpathPathQueryString = "/*/*" + _xpathPathQueryString;
                if (_xpathPathQueryString.Equals("")) _xpathPathQueryString = "/*/*";
                if (_xpathPathQueryString.StartsWith(">")) _xpathPathQueryString = "/*/*" + _xpathPathQueryString.Substring(1);
                if (_xpathPathQueryString.Contains("#"))
                {
                    var hashPos = _xpathPathQueryString.IndexOf("#");
                    AttributeField = _xpathPathQueryString.Substring(hashPos + 1);
                    _xpathPathQueryString = _xpathPathQueryString.Substring(0, hashPos);
                }
                var listitems = doc.SelectNodes(_xpathPathQueryString);

                PROC = string.Format("ObjectXPathExtentions.ObjectPropertyAsString( item=[object {0}], _xpathPathQueryString='{1}', pattern={2}, AttributeField={3}, XPathCount={4})", _item.GetType().Name, _xpathPathQueryString, pattern, AttributeField, listitems.Count);

                string firstPrefix = "";
                string otherPrefix = "";
                string otherSuffix = "";
                var pos = 0;
                if (pattern.Contains("%1"))
                {
                    firstPrefix = pattern.Substring(pos, pattern.IndexOf("%1") - pos);
                    pos = pattern.IndexOf("%1") + 2;
                }
                if (pattern.Contains("%2"))
                {
                    otherPrefix = pattern.Substring(pos, pattern.IndexOf("%2") - pos);
                    pos = pattern.IndexOf("%2") + 2;
                }
                if (pos > 0)
                {
                    otherSuffix = pattern.Substring(pos);
                }
                var itemCount = 0;
                var sb = new StringBuilder();
                foreach (var item in listitems)
                {

                    var nod = ((XmlNode)item);
                    if (xpathPathQueryStringChild.Length>0)
                    {
                        var _xpathPathQueryStringChild = xpathPathQueryStringChild;
                        if (_xpathPathQueryStringChild.StartsWith("#")) _xpathPathQueryStringChild = "/*/*" + _xpathPathQueryStringChild;
                        if (_xpathPathQueryStringChild.Equals("")) _xpathPathQueryStringChild = "/*/*";
                        if (_xpathPathQueryStringChild.StartsWith(">")) _xpathPathQueryStringChild = "/*/*/" + _xpathPathQueryStringChild.Substring(1);
                        if (_xpathPathQueryStringChild.Contains("#"))
                        {
                            var hashPos = _xpathPathQueryStringChild.IndexOf("#");
                            AttributeField = _xpathPathQueryStringChild.Substring(hashPos + 1);
                            _xpathPathQueryStringChild = _xpathPathQueryStringChild.Substring(0, hashPos);
                        }
                        if (nod.Attributes["json:ref"] != null)
                        {
                            //if it is, lets get the id and then grab the node and set the node, thus evaluating the reference
                            var refid = nod.Attributes["json:ref"].Value;
                            nod = doc.SelectSingleNode(string.Format("//*[@json:id ='{0}']", refid), nsmgr);
                            nod = nod.SelectSingleNode(_xpathPathQueryStringChild);
                        }
                        else
                        {
                            nod = nod.SelectSingleNode(_xpathPathQueryStringChild);

                        }

                        //Check and see if this is a reference to another node
                    }
                    if (nod != null)
                    {
                        var text = "";
                        switch (AttributeField.ToUpper())
                        {
                            case "NAME":
                                text = nod.Name;
                                break;
                            case "VALUE":
                                text = nod.Value;
                                break;
                            case "INNERTEXT":
                                text = nod.InnerText;
                                break;
                            case "INNERXML":
                                text = nod.InnerXml;
                                break;
                        }
                        if (itemCount == 0)
                        {
                            sb.Append(firstPrefix);
                            sb.Append(text);
                        }
                        else
                        {
                            sb.Append(otherPrefix);
                            sb.Append(text);
                            sb.Append(otherSuffix);
                        }
                        itemCount++;
                    }
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(PROC + ": ERROR! " + ex.Message, ex);
            }

        }
    }
}
