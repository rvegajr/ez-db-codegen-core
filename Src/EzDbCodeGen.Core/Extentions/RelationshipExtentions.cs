using System;
using System.Collections.Generic;
using System.Linq;
using EzDbSchema.Core;
using EzDbSchema.Core.Enums;
using EzDbSchema.Core.Objects;
using CoreInterfaces = EzDbSchema.Core.Interfaces;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Extensions
{
    /// <summary>
    /// Extension methods for working with database relationships.
    /// </summary>
    public static class RelationshipExtentions
    {
        // Extension method to get RelationshipMultiplicityType from IRelationship
        /// <summary>
        /// Determines the multiplicity type of a relationship.
        /// </summary>
        /// <param name="relationship">The relationship to check.</param>
        /// <returns>The multiplicity type of the relationship.</returns>
        public static RelationshipMultiplicityType GetMultiplicityType(this IRelationship relationship)
        {
            if (relationship == null)
            {
                return RelationshipMultiplicityType.Unknown;
            }

            return IsOneToOne(relationship) ? RelationshipMultiplicityType.OneToOne : RelationshipMultiplicityType.OneToMany;
        }

        /// <summary>
        /// Determines if a relationship is one-to-one.
        /// </summary>
        /// <param name="relationship">The relationship to check.</param>
        /// <returns>True if the relationship is one-to-one, false otherwise.</returns>
        public static bool IsOneToOne(this IRelationship relationship)
        {
            if (relationship == null)
            {
                return false;
            }

            // Check the MultiplicityType property if available
            if (relationship.MultiplicityType == RelationshipMultiplicityType.OneToOne)
            {
                return true;
            }

            // Check the RelationshipType string property
            if (!string.IsNullOrEmpty(relationship.RelationshipType))
            {
                return relationship.RelationshipType.Equals("OneToOne", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Groups relationships by foreign key name.
        /// </summary>
        /// <param name="relationships">The collection of relationships to group.</param>
        /// <returns>A dictionary of relationships grouped by foreign key name.</returns>
        public static Dictionary<string, List<IRelationship>> GroupByFKName(this IEnumerable<IRelationship> relationships)
        {
            if (relationships == null)
            {
                throw new ArgumentNullException(nameof(relationships));
            }

            return relationships
                .GroupBy(r => r.ConstraintName)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList(),
                    StringComparer.OrdinalIgnoreCase
                );
        }

        /// <summary>
        /// Creates a summary of the relationship.
        /// </summary>
        /// <param name="relationship">The relationship to summarize.</param>
        /// <returns>A RelationshipSummary object containing key information about the relationship.</returns>
        public static RelationshipSummary AsSummary(this IRelationship relationship)
        {
            if (relationship == null)
            {
                throw new ArgumentNullException(nameof(relationship));
            }

            return new RelationshipSummary
            {
                ConstraintName = relationship.ConstraintName,
                FromTableName = relationship.FromTableName,
                FromColumnNameSingle = relationship.FromColumnName,
                ToTableName = relationship.ToTableName,
                ToColumnNameSingle = relationship.ToColumnName,
                IsOneToOne = relationship.IsOneToOne(),
                IsSelfReferencing = relationship.FromTableName == relationship.ToTableName,
                FromTableNameSingular = relationship.FromTableName.ToSingular(),
                ToTableNameSingular = relationship.ToTableName.ToSingular(),
                FromTableNamePlural = relationship.FromTableName.ToPlural(),
                ToTableNamePlural = relationship.ToTableName.ToPlural()
            };
        }

        /// <summary>
        /// Gets all relationships for an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>A list of relationships.</returns>
        public static IEnumerable<IRelationship> GetRelationships(this IEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.Relationships == null)
            {
                return Enumerable.Empty<IRelationship>();
            }

            return entity.Relationships;
        }

        public static string GenerateObjectName(this IEntity entity, string FKName, ObjectNameGeneratedFrom generatedFrom)
        {
            var PROC_NAME = $"RelationshipExtentions.GenerateObjectName(entity='{entity?.GetType().GetProperty("TableName")?.GetValue(entity)}', FKName='{FKName}')";
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                if (string.IsNullOrEmpty(FKName))
                {
                    throw new ArgumentNullException(nameof(FKName));
                }

                var relationships = entity.GetRelationships();
                if (relationships == null || !relationships.Any())
                {
                    return string.Empty;
                }

                var relationship = relationships.FirstOrDefault(r => r.ConstraintName == FKName);
                if (relationship == null)
                {
                    return string.Empty;
                }

                switch (generatedFrom)
                {
                    case ObjectNameGeneratedFrom.JoinFromColumnName:
                        return relationship.FromColumnName;
                    case ObjectNameGeneratedFrom.JoinToColumnName:
                        return relationship.ToColumnName;
                    case ObjectNameGeneratedFrom.JoinFromPropertyName:
                        return relationship.FromColumnName.ToSingular();
                    case ObjectNameGeneratedFrom.JoinToPropertyName:
                        return relationship.ToColumnName.ToSingular();
                    default:
                        return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{PROC_NAME} failed: {ex.Message}", ex);
            }
        }

        public static List<string> GenerateObjectNames(this IEntity entity, string FKName, ObjectNameGeneratedFrom generatedFrom)
        {
            var PROC_NAME = $"RelationshipExtentions.GenerateObjectNames(entity='{entity?.GetType().GetProperty("TableName")?.GetValue(entity)}', FKName='{FKName}')";
            try
            {
                var list = new List<string>();
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                if (string.IsNullOrEmpty(FKName))
                {
                    throw new ArgumentNullException(nameof(FKName));
                }

                var relationships = entity.GetRelationships();
                if (relationships == null || !relationships.Any())
                {
                    return list;
                }

                var relationship = relationships.FirstOrDefault(r => r.ConstraintName == FKName);
                if (relationship == null)
                {
                    return list;
                }

                switch (generatedFrom)
                {
                    case ObjectNameGeneratedFrom.JoinFromColumnName:
                        list.Add(relationship.FromColumnName);
                        break;
                    case ObjectNameGeneratedFrom.JoinToColumnName:
                        list.Add(relationship.ToColumnName);
                        break;
                    case ObjectNameGeneratedFrom.JoinFromPropertyName:
                        list.Add(relationship.FromColumnName.ToSingular());
                        break;
                    case ObjectNameGeneratedFrom.JoinToPropertyName:
                        list.Add(relationship.ToColumnName.ToSingular());
                        break;
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception($"{PROC_NAME} failed: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Represents a summary of a database relationship.
    /// </summary>
    public class RelationshipSummary
    {
        /// <summary>
        /// Gets or sets the name of the foreign key.
        /// </summary>
        public string ConstraintName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the from table.
        /// </summary>
        public string FromTableName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the from column.
        /// </summary>
        public string FromColumnNameSingle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the to table.
        /// </summary>
        public string ToTableName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the to column.
        /// </summary>
        public string ToColumnNameSingle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the relationship is one-to-one.
        /// </summary>
        public bool IsOneToOne { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the relationship is self-referencing.
        /// </summary>
        public bool IsSelfReferencing { get; set; }

        /// <summary>
        /// Gets or sets the singular form of the from table name.
        /// </summary>
        public string FromTableNameSingular { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the singular form of the to table name.
        /// </summary>
        public string ToTableNameSingular { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the plural form of the from table name.
        /// </summary>
        public string FromTableNamePlural { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the plural form of the to table name.
        /// </summary>
        public string ToTableNamePlural { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        public IEntity? Entity { get; set; }

        /// <summary>
        /// Gets or sets the list of from column names.
        /// </summary>
        public List<string> FromColumnName { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of to column names.
        /// </summary>
        public List<string> ToColumnName { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of from property names.
        /// </summary>
        public List<string> FromPropertyName { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of to property names.
        /// </summary>
        public List<string> ToPropertyName { get; set; } = new();
    }

    public enum ObjectNameGeneratedFrom
    {
        JoinFromColumnName,
        JoinToColumnName,
        JoinFromPropertyName,
        JoinToPropertyName
    }
}
