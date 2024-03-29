﻿using EzDbCodeGen.Core.Enums;
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
    internal static class ProjectHelpersExtentions
    {
        public static Dictionary<string, TemplateFileAction> AsWildCardPaths(this Dictionary<string, TemplateFileAction> filesList ) {
            var retFilesList = new Dictionary<string, TemplateFileAction>();
            foreach(var fileName in filesList.Keys)
            {
                var path = Path.GetDirectoryName(fileName);
                var WCPath = $"{path}{Path.DirectorySeparatorChar}*";
                if (!retFilesList.ContainsKey(WCPath))
                {
                    retFilesList.Add(WCPath, TemplateFileAction.Add);
                }
            }
            return retFilesList;
        }

    }
    internal class ProjectHelpers
    {
        /// <summary>
        /// Modifies a project file so that it will contain the includes to bring in files to a project.  This is only applicable to older versions of a project.  
        /// VS2017 that follow the new project format,  this processing will be ignored.   This function will also go through all Compile[@include] items and 
        /// will prune all those entries that do not have actual existing file names.
        /// </summary>
        /// <param name="ProjectFile">The project file.</param>
        /// <param name="PathToSearchFor">A single filename (that can contain wild cards)</param>
        /// <param name="clearObjAndBin">Removes the obj and bin files from the Project Path</param>
        /// <returns>true if the project file was modified,  </returns>
        public bool ModifyClassPath(string ProjectFile, string PathToSearchFor, bool clearObjAndBin) {
            var list = new Dictionary<string, TemplateFileAction>
            {
                { PathToSearchFor, TemplateFileAction.Add }
            };
            return ModifyClassPath(ProjectFile, list, clearObjAndBin);
        }

        /// <summary>
        /// Modifies a project file so that it will contain the includes to bring in files to a project.  This is only applicable to older versions of a project.  
        /// VS2017 that follow the new project format,  this processing will be ignored.   This function will also go through all Compile[@include] items and 
        /// will prune all those entries that do not have actual existing file names.
        /// </summary>
        /// <param name="ProjectFile">The project file.</param>
        /// <param name="PathsToSearchFor">A dictionary that contains a list of paths (that can contain wild cards) and File actions</param>
        /// <param name="clearObjAndBin">Removes the obj and bin files from the Project Path</param>
        /// <returns>true if the project file was modified,  </returns>
        public bool ModifyClassPath(string ProjectFile, Dictionary<string, TemplateFileAction> PathsToSearchFor, bool clearObjAndBin)
        {
            var UpdateXML = false;
            try
            {
                var WildCardDirectories = new HashSet<string>();
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
                        } else if (nodes.Count > 0)
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
                    //Ignore wile card paths in the file check
                    if ((!FileToCheck.EndsWith("*")) && (!File.Exists(FileToCheck))) 
                    {
                        nod.ParentNode.RemoveChild(nod);
                        UpdateXML = true;
                    }
                }
                if (clearObjAndBin)
                {
                    // Figure out output path and clear all object and bin
                    var nodeOutputPath = xmldoc.SelectNodes(@"//x:OutputPath", mgr);
                    foreach (XmlElement nod in nodeOutputPath)
                    {
                        var OutputPath = (ProjectPath + nod.InnerText).PathEnds();
                        var ObjPath = (ProjectPath + "obj").PathEnds();
                        try
                        {
                            if (Directory.Exists(OutputPath)) Directory.Delete(OutputPath, true);
                            if (Directory.Exists(ObjPath)) Directory.Delete(ObjPath, true);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine($"ModifyClassPath: Clearing {OutputPath}, but {ex.Message}");
                        }
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
