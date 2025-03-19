using HandlebarsDotNet;
using System.Diagnostics;
using EzDbSchema.Core.Interfaces;

namespace EzDbCodeGen.Core.Handlebars
{
    internal static class HandlebarsUnitTestHelpers
    {
        internal static void RegisterHelpers(IHandlebars handlebars)
        {
            handlebars.RegisterHelper("UnitTestsRenderExtendedEndpoints", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('UnitTestsRenderExtendedEndpoints')";
                try
                {
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                    if (context.Value is IEntity entity)
                    {
                        var tableName = entity.TableName ?? string.Empty;
                        var tableAlias = tableName;

                        var relationshipGroups = entity.RelationshipGroups;
                        if (relationshipGroups != null)
                        {
                            foreach (var relationshipGroupKV in relationshipGroups)
                            {
                                var relationshipGroup = relationshipGroupKV.Value;
                                if (relationshipGroup != null)
                                {
                                    var relationship = relationshipGroup.FirstOrDefault();
                                    if (relationship is EzDbSchema.Core.Interfaces.IRelationship coreRelationship)
                                    {
                                        string expandColumnName = coreRelationship.FromPropertyName != null ? string.Join("", coreRelationship.FromPropertyName) : (coreRelationship.FromTableName ?? string.Empty);
                                        writer.WriteSafeString($"\n{prefix}using (var response = await HttpClient.GetAsync(\"http://testserver/api/{tableAlias}?%24expand={expandColumnName}&%24top=10\")) ");
                                        writer.WriteSafeString($"\n{prefix}{{ ");
                                        writer.WriteSafeString($"\n{prefix}    var result = await response.Content.ReadAsStringAsync(); ");
                                        writer.WriteSafeString($"\n{prefix}    Assert.IsTrue(response.StatusCode == HttpStatusCode.OK, \"Return Get 10 or less {tableName} with Expand of {expandColumnName}. \" + result); ");
                                        writer.WriteSafeString($"\n{prefix}}} ");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });
        }
    }
}
