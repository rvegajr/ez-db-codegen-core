using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.Classes;
using EzDbSchema.Core.Interfaces;

namespace EzDbCodeGen.Core.Extensions
{
    /// <summary>
    /// Extension methods for the IDatabase interface.
    /// </summary>
    public static class DatabaseSchemaExtensions
    {
        /// <summary>
        /// Compares two database schemas and returns the differences.
        /// </summary>
        /// <param name="source">The source database schema.</param>
        /// <param name="target">The target database schema to compare with.</param>
        /// <returns>A list of schema differences.</returns>
        public static List<SchemaDiff> CompareTo(this IDatabase source, IDatabase target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var differences = new List<SchemaDiff>();

            // Compare entities in source that are not in target (added)
            foreach (var sourceEntity in source.Values)
            {
                if (!target.ContainsKey(sourceEntity.TableName))
                {
                    differences.Add(new SchemaDiff
                    {
                        EntityName = sourceEntity.TableName,
                        FileAction = Enums.TemplateFileAction.Add,
                        Reason = $"Entity '{sourceEntity.TableName}' exists in source but not in target"
                    });
                }
            }

            // Compare entities in target that are not in source (deleted)
            foreach (var targetEntity in target.Values)
            {
                if (!source.ContainsKey(targetEntity.TableName))
                {
                    differences.Add(new SchemaDiff
                    {
                        EntityName = targetEntity.TableName,
                        FileAction = Enums.TemplateFileAction.Delete,
                        Reason = $"Entity '{targetEntity.TableName}' exists in target but not in source"
                    });
                }
            }

            // Compare entities that exist in both (modified)
            foreach (var sourceEntity in source.Values)
            {
                if (target.ContainsKey(sourceEntity.TableName))
                {
                    var targetEntity = target[sourceEntity.TableName];
                    bool hasChanges = false;

                    // Compare properties
                    foreach (var sourceProperty in sourceEntity.Properties.Values)
                    {
                        if (!targetEntity.Properties.ContainsKey(sourceProperty.PropertyName) ||
                            !ArePropertiesEqual(sourceProperty, targetEntity.Properties[sourceProperty.PropertyName]))
                        {
                            hasChanges = true;
                            break;
                        }
                    }

                    // Check for deleted properties
                    foreach (var targetProperty in targetEntity.Properties.Values)
                    {
                        if (!sourceEntity.Properties.ContainsKey(targetProperty.PropertyName))
                        {
                            hasChanges = true;
                            break;
                        }
                    }

                    if (hasChanges)
                    {
                        differences.Add(new SchemaDiff
                        {
                            EntityName = sourceEntity.TableName,
                            FileAction = Enums.TemplateFileAction.Update,
                            Reason = $"Entity '{sourceEntity.TableName}' has been modified"
                        });
                    }
                }
            }

            return differences;
        }

        /// <summary>
        /// Compares two properties for equality.
        /// </summary>
        /// <param name="source">The source property.</param>
        /// <param name="target">The target property to compare with.</param>
        /// <returns>True if the properties are equal, false otherwise.</returns>
        private static bool ArePropertiesEqual(IProperty source, IProperty target)
        {
            if (source == null || target == null)
            {
                return source == target;
            }

            return source.PropertyName == target.PropertyName &&
                   source.DataType == target.DataType &&
                   source.MaxLength == target.MaxLength &&
                   source.IsNullable == target.IsNullable &&
                   source.IsPrimaryKey == target.IsPrimaryKey;
        }
    }
}
