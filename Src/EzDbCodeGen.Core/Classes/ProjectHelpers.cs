using EzDbCodeGen.Core.Enums;
using EzDbCodeGen.Core.Extentions.Strings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Classes
{
    internal class ProjectHelpers
    {
        /// <summary>
        /// Modifies a project file so that it will contain the includes to bring in files to a project.  This is only applicable to older versions of a project.  
        /// VS2017 that follow the new project format,  this processing will be ignored.   This function will also go through all Compile[@include] items and 
        /// will prune all those entries that do not have actual existing file names.
        /// </summary>
        /// <param name="ProjectFile">The project file.</param>
        /// <param name="PathToSearchFor">A single filename (that can contain wild cards)</param>
        /// <returns>true if the project file was modified,  </returns>
        public bool ModifyClassPath(string ProjectFile, string PathToSearchFor) {
            var list = new Dictionary<string, TemplateFileAction>
            {
                { PathToSearchFor, TemplateFileAction.Add }
            };
            return ModifyClassPath(ProjectFile, list);
        }

        /// <summary>
        /// Modifies a project file so that it will contain the includes to bring in files to a project.  This is only applicable to older versions of a project.  
        /// VS2017 that follow the new project format,  this processing will be ignored.   This function will also go through all Compile[@include] items and 
        /// will prune all those entries that do not have actual existing file names.
        /// </summary>
        /// <param name="ProjectFile">The project file.</param>
        /// <param name="PathsToSearchFor">A dictionary that contains a list of paths (that can contain wild cards) and File actions</param>
        /// <returns>true if the project file was modified,  </returns>
        public bool ModifyClassPath(string ProjectFile, Dictionary<string, TemplateFileAction> PathsToSearchFor)
        {
            var UpdateXML = false;
            try
            {
                var isNewProjectFormat = false;
                var txt = File.ReadAllText(ProjectFile);
                isNewProjectFormat = (txt.ToLower().Contains("\"microsoft.net.sdk"));
                if (isNewProjectFormat) return false;
                FileStream fs = new FileStream(ProjectFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var xmldoc = new XmlDocument();
                xmldoc.Load(fs);
                XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
                mgr.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");
                var ItemGroupBuffer = new StringBuilder();
                foreach (var PathToSearchForKeyWithAction in PathsToSearchFor)
                {
                    var PathToSearchFor = PathToSearchForKeyWithAction.Key;
                    if (PathToSearchFor.Contains("*"))
                    {
                        var PathForWildcard = PathToSearchFor.Split('*')[0];
                        //First check and see if wildcard exists
                        var nodesWc = xmldoc.SelectNodes(@"//x:Compile[@Include='" + PathToSearchFor + @"']", mgr);
                        if (nodesWc.Count == 0)
                        {
                            var nodes = xmldoc.SelectNodes(@"//x:Compile[starts-with(@Include, '" + PathForWildcard + @"')]", mgr);
                            for (int i = nodes.Count - 1; i >= 0; i--)
                            {
                                nodes[i].ParentNode.RemoveChild(nodes[i]);
                            }
                            ItemGroupBuffer.Append(@"<Compile Include=""" + PathToSearchFor + @""" />");
                            UpdateXML = true;
                        }
                    }
                    else  // find the node in the project and remove 
                    {
                        //var nodes = xmldoc.SelectNodes(@"//x:Compile[lower-case(@Include)='" + PathToSearchFor.ToLower() + @"']", mgr);
                        //Perform a case insensitive search for the file pattern and if we find it,  then we can set the case correctly and remove any duplicates
                        var nodes = xmldoc.SelectNodes(@"//x:Compile[translate(@Include,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='" + PathToSearchFor.ToLower() + @"']", mgr);
                        
                        if ((PathToSearchForKeyWithAction.Value == TemplateFileAction.Delete) && (nodes.Count > 0)) { 
                            for (int i = nodes.Count - 1; i >= 0; i--) nodes[i].ParentNode.RemoveChild(nodes[i]);
                            UpdateXML = true;
                        } else if ((PathToSearchForKeyWithAction.Value == TemplateFileAction.Add) && (nodes.Count == 0))
                        {
                            ItemGroupBuffer.Append(@"<Compile Include=""" + PathToSearchFor + @""" />");
                            UpdateXML = true;
                        } else if (nodes.Count > 1)
                        {//This will remove duplicates and ensure the name that we have matches the case for files
                            for (int i = nodes.Count - 1; i >= 0; i--)
                            {
                                if (i== nodes.Count - 1)
                                {
                                    nodes[i].Attributes["Include"].InnerText = PathToSearchFor;
                                }
                                else
                                {
                                    nodes[i].ParentNode.RemoveChild(nodes[i]);
                                    UpdateXML = true;
                                }
                            }
                        }
                    }

                }
                if (ItemGroupBuffer.Length>0)
                {
                    XmlDocumentFragment docFrag = xmldoc.CreateDocumentFragment();

                    //Set the contents of the document fragment.
                    docFrag.InnerXml = @"<ItemGroup>" + ItemGroupBuffer.ToString() + "</ItemGroup>";

                    //Add the children of the document fragment to the original document.
                    xmldoc.DocumentElement.AppendChild(docFrag);
                    UpdateXML = true;

                }
                //Clean up ItemGroup nodes that do not have any child nodes
                var nodesEmpty = xmldoc.SelectNodes(@"//x:ItemGroup[not(node())]", mgr);
                if (nodesEmpty.Count > 0) UpdateXML = true;
                for (int i = nodesEmpty.Count - 1; i >= 0; i--)
                {
                    UpdateXML = true;
                    nodesEmpty[i].ParentNode.RemoveChild(nodesEmpty[i]);
                }
                //While we are here,  lets loop through the solution file and clean up Include References that no longer exist
                var nodeToCheck = xmldoc.SelectNodes(@"//x:Compile[@Include]", mgr);
                var ProjectPath = Path.GetDirectoryName(ProjectFile).PathEnds();
                foreach (XmlElement nod in nodeToCheck)
                {
                    var FileToCheck = ProjectPath + nod.Attributes["Include"].InnerText;
                    if (!File.Exists(FileToCheck))
                    {
                        nod.ParentNode.RemoveChild(nod);
                        UpdateXML = true;
                    }
                }

                if (UpdateXML)
                {
                    var sXML = xmldoc.OuterXml.Replace(@" xmlns=""""", "").Replace(@" xmlns=""""", "");
                    xmldoc.LoadXml(sXML);
                    xmldoc.Save(ProjectFile);
                }
                return UpdateXML;
            }
            catch 
            {
                throw;
            }
        }
    }
}
