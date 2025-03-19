using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EzDbSchema.Core.Interfaces;
using System.Text.Json;

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Enhanced selector for navigating and filtering entities and relationships
    /// in Handlebars templates based on path expressions and conditions.
    /// </summary>
    public class SelectEz
    {
        private readonly IfCondEz _conditionEvaluator;

        public SelectEz()
        {
            _conditionEvaluator = new IfCondEz();
        }

        /// <summary>
        /// Selects items from the provided source based on the specified path expression.
        /// </summary>
        /// <param name="expression">The path expression with optional filter conditions.</param>
        /// <param name="source">The source object to select from.</param>
        /// <returns>A collection of items that match the selection criteria.</returns>
        public IEnumerable<object> Select(string expression, object source)
        {
            return Select(expression, source, null);
        }

        /// <summary>
        /// Selects items from the provided source based on the specified path expression,
        /// with access to a context for parent/root navigation.
        /// </summary>
        /// <param name="expression">The path expression with optional filter conditions.</param>
        /// <param name="source">The source object to select from.</param>
        /// <param name="context">Optional context for parent/root navigation.</param>
        /// <returns>A collection of items that match the selection criteria.</returns>
        public IEnumerable<object> Select(string expression, object source, IDictionary<string, object> context)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Source cannot be null.");
            }

            // Handle absolute path starting with /
            if (expression.StartsWith("/"))
            {
                if (context == null)
                {
                    throw new ArgumentException("Context is required for absolute path expressions.", nameof(context));
                }

                return SelectFromAbsolutePath(expression.Substring(1), context);
            }

            // Handle parent navigation with $..
            if (expression.StartsWith("$.."))
            {
                if (context == null)
                {
                    throw new ArgumentException("Context is required for parent navigation expressions.", nameof(context));
                }

                return SelectFromParent(expression.Substring(3), context);
            }

            // Handle current node navigation with $.
            if (expression.StartsWith("$."))
            {
                return SelectFromCurrentNode(expression.Substring(2), source);
            }

            // Default to current node if no prefix is specified
            return SelectFromCurrentNode(expression, source);
        }

        private IEnumerable<object> SelectFromAbsolutePath(string path, IDictionary<string, object> context)
        {
            // Extract the root object name and the filter part
            var parts = path.Split(new[] { '[' }, 2);
            string rootName = parts[0];

            if (!context.TryGetValue(rootName, out var rootObject))
            {
                throw new ArgumentException($"Root object '{rootName}' not found in context.", nameof(context));
            }

            if (parts.Length == 1)
            {
                // No filter, return the entire collection
                return ConvertToEnumerable(rootObject);
            }

            // Extract the filter condition
            string filterCondition = parts[1].TrimEnd(']');
            return ApplyFilter(ConvertToEnumerable(rootObject), filterCondition);
        }

        private IEnumerable<object> SelectFromParent(string path, IDictionary<string, object> context)
        {
            // For simplicity, we'll just look in the Entities collection in the context
            if (!context.TryGetValue("Entities", out var entities))
            {
                throw new ArgumentException("Parent navigation requires 'Entities' in context.", nameof(context));
            }

            // Extract the filter condition
            var match = Regex.Match(path, @"\[(.*?)\]");
            if (!match.Success)
            {
                return ConvertToEnumerable(entities);
            }

            string filterCondition = match.Groups[1].Value;
            return ApplyFilter(ConvertToEnumerable(entities), filterCondition);
        }

        private IEnumerable<object> SelectFromCurrentNode(string path, object source)
        {
            // Handle direct filtering on a collection
            if (path.StartsWith("[") && path.EndsWith("]"))
            {
                string directFilter = path.Substring(1, path.Length - 2);
                return ApplyFilter(ConvertToEnumerable(source), directFilter);
            }
            
            // Handle property access with optional filter
            var propertyMatch = Regex.Match(path, @"^([^[]+)(?:\[(.*?)\])?$");
            if (!propertyMatch.Success)
            {
                throw new ArgumentException($"Invalid path expression: {path}", nameof(path));
            }

            string propertyName = propertyMatch.Groups[1].Value;
            
            // If we're selecting from a collection directly
            if (string.IsNullOrEmpty(propertyName) && source is IEnumerable enumerable && !(source is string))
            {
                var collectionFilter = propertyMatch.Groups[2].Value;
                return ApplyFilter(ConvertToEnumerable(enumerable), collectionFilter);
            }

            // Special handling for Relationships property
            if (propertyName.Equals("Relationships", StringComparison.OrdinalIgnoreCase) && source is IEntity entity)
            {
                var relationships = entity.Relationships;
                if (relationships == null)
                {
                    return Enumerable.Empty<object>();
                }

                // Convert the IRelationshipReferenceList to a list of IRelationship objects
                var relationshipsList = new List<object>();
                foreach (var relationship in relationships)
                {
                    relationshipsList.Add(relationship);
                }

                // If there's a filter, apply it
                if (propertyMatch.Groups[2].Success)
                {
                    string relationshipFilter = propertyMatch.Groups[2].Value;
                    return ApplyFilter(relationshipsList, relationshipFilter);
                }

                return relationshipsList;
            }

            // Access the property on the source object
            var property = GetProperty(source, propertyName);
            if (property == null)
            {
                return Enumerable.Empty<object>();
            }

            // If there's no filter, return the property as a collection
            if (!propertyMatch.Groups[2].Success)
            {
                return ConvertToEnumerable(property);
            }

            // Apply filter to the property value
            var propertyFilter = propertyMatch.Groups[2].Value;
            return ApplyFilter(ConvertToEnumerable(property), propertyFilter);
        }

        private IEnumerable<object> ApplyFilter(IEnumerable<object> items, string filterCondition)
        {
            if (string.IsNullOrEmpty(filterCondition))
            {
                return items;
            }

            // Check for multiple conditions
            if (filterCondition.Contains("&&"))
            {
                var conditions = filterCondition.Split(new[] { "&&" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .ToList();
                return items.Where(item => conditions.All(condition => EvaluateCondition(item, condition)));
            }

            // Special handling for relationship filtering with ConstraintName
            if (filterCondition.StartsWith("ConstraintName="))
            {
                var relationshipMatch = Regex.Match(filterCondition, @"ConstraintName=(?:'([^']*)'|""([^""]*)"")");
                if (relationshipMatch.Success)
                {
                    string expectedName = relationshipMatch.Groups[1].Success 
                        ? relationshipMatch.Groups[1].Value 
                        : relationshipMatch.Groups[2].Value;
                    
                    bool hasWildcard = expectedName.Contains("*");
                    
                    if (hasWildcard)
                    {
                        string pattern = "^" + Regex.Escape(expectedName).Replace("\\*", ".*") + "$";
                        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                        
                        return items.Where(item => 
                        {
                            if (item is IRelationship rel)
                            {
                                return regex.IsMatch(rel.ConstraintName);
                            }
                            return false;
                        });
                    }
                    else
                    {
                        return items.Where(item => 
                        {
                            if (item is IRelationship rel)
                            {
                                return string.Equals(rel.ConstraintName, expectedName, StringComparison.OrdinalIgnoreCase);
                            }
                            return false;
                        });
                    }
                }
            }

            // Handle standard condition
            return items.Where(item => EvaluateCondition(item, filterCondition));
        }

        private bool EvaluateCondition(object item, string condition)
        {
            // Special handling for IRelationship objects and ConstraintName condition
            if (item is IRelationship relationship && 
                condition.StartsWith("ConstraintName=", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(condition, @"ConstraintName=(?:'([^']*)'|""([^""]*)"")");
                if (match.Success)
                {
                    string relationshipValue = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                    bool hasWildcard = relationshipValue.Contains("*");
                    
                    if (hasWildcard)
                    {
                        string pattern = "^" + Regex.Escape(relationshipValue).Replace("\\*", ".*") + "$";
                        return Regex.IsMatch(relationship.ConstraintName, pattern, RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        return string.Equals(relationship.ConstraintName, relationshipValue, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            
            // Parse the condition in format PropertyName='Value'
            var condMatch = Regex.Match(condition, @"([^=]+)=(?:'([^']*)'|""([^""]*)"")");
            if (!condMatch.Success)
            {
                // For boolean properties, allow shorthand like Properties['Id'].IsPrimaryKey=true
                condMatch = Regex.Match(condition, @"([^=]+)=(true|false)");
                if (!condMatch.Success)
                {
                    return false;
                }
                
                string boolPropertyPath = condMatch.Groups[1].Value.Trim();
                bool expectedBoolValue = bool.Parse(condMatch.Groups[2].Value);

                // Get the actual property value
                object boolPropertyValue = GetPropertyValue(item, boolPropertyPath);
                if (boolPropertyValue == null)
                {
                    return false;
                }

                // Handle boolean comparison
                if (boolPropertyValue is bool actualBoolValue)
                {
                    return actualBoolValue == expectedBoolValue;
                }

                // Try to parse string value as boolean
                if (boolPropertyValue is string actualStringValue && bool.TryParse(actualStringValue, out var parsedValue))
                {
                    return parsedValue == expectedBoolValue;
                }

                return false;
            }

            string propertyPath = condMatch.Groups[1].Value.Trim();
            string expectedValue = condMatch.Groups[2].Success ? condMatch.Groups[2].Value : condMatch.Groups[3].Value;
            bool isWildcard = expectedValue.Contains("*");

            // Get the actual property value
            object actualValue = GetPropertyValue(item, propertyPath);
            if (actualValue == null)
            {
                return false;
            }

            string actualValueString = actualValue.ToString() ?? string.Empty;

            // Handle wildcard matching
            if (isWildcard)
            {
                string pattern = "^" + Regex.Escape(expectedValue).Replace("\\*", ".*") + "$";
                return Regex.IsMatch(actualValueString, pattern, RegexOptions.IgnoreCase);
            }

            // Handle exact matching
            return string.Equals(actualValueString, expectedValue, StringComparison.OrdinalIgnoreCase);
        }

        private object GetPropertyValue(object obj, string propertyPath)
        {
            if (obj == null || string.IsNullOrEmpty(propertyPath))
            {
                return null;
            }

            // Handle nested properties using dot notation
            string[] parts = propertyPath.Split('.');
            object current = obj;

            foreach (var part in parts)
            {
                if (current == null)
                {
                    return null;
                }

                // Handle indexers like Properties['Id']
                var indexerMatch = Regex.Match(part, @"([^\[]+)\['([^']+)'\]");
                if (indexerMatch.Success)
                {
                    string collectionName = indexerMatch.Groups[1].Value;
                    string key = indexerMatch.Groups[2].Value;

                    // Get the collection
                    var collection = GetProperty(current, collectionName);
                    if (collection == null)
                    {
                        return null;
                    }

                    // Handle dictionary-like collections
                    if (collection is IDictionary dictionary && dictionary.Contains(key))
                    {
                        current = dictionary[key];
                        continue;
                    }

                    // Handle IProperty collections specifically
                    if (collection is IDictionary<string, IProperty> propertyDictionary)
                    {
                        if (propertyDictionary.TryGetValue(key, out var property))
                        {
                            current = property;
                            continue;
                        }
                    }

                    // Try to find an item with a Name, Key, or ConstraintName property matching the key
                    if (collection is IEnumerable enumerable)
                    {
                        bool found = false;
                        foreach (var item in enumerable)
                        {
                            if (item == null) continue;

                            // Try to match by ConstraintName for IRelationship
                            if (item is IRelationship relationship && 
                                string.Equals(relationship.ConstraintName, key, StringComparison.OrdinalIgnoreCase))
                            {
                                current = item;
                                found = true;
                                break;
                            }

                            // Try to match by common property names
                            var nameProperty = item.GetType().GetProperty("Name");
                            var keyProperty = item.GetType().GetProperty("Key");
                            var constraintNameProperty = item.GetType().GetProperty("ConstraintName");

                            if (nameProperty != null && 
                                string.Equals(nameProperty.GetValue(item)?.ToString(), key, StringComparison.OrdinalIgnoreCase))
                            {
                                current = item;
                                found = true;
                                break;
                            }
                            else if (keyProperty != null && 
                                     string.Equals(keyProperty.GetValue(item)?.ToString(), key, StringComparison.OrdinalIgnoreCase))
                            {
                                current = item;
                                found = true;
                                break;
                            }
                            else if (constraintNameProperty != null && 
                                     string.Equals(constraintNameProperty.GetValue(item)?.ToString(), key, StringComparison.OrdinalIgnoreCase))
                            {
                                current = item;
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    // Simple property access
                    current = GetProperty(current, part);
                    if (current == null)
                    {
                        return null;
                    }
                }
            }

            return current;
        }

        private object GetProperty(object obj, string propertyName)
        {
            if (obj == null)
            {
                return null;
            }
            
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));
            }

            // Handle special property Count for collections
            if (propertyName == "Count" && obj is ICollection collection)
            {
                return collection.Count;
            }

            // Handle special property Length for strings
            if (propertyName == "Length" && obj is string str)
            {
                return str.Length;
            }

            // Handle special property Count for IRelationshipReferenceList
            if (propertyName == "Count" && obj is IRelationshipReferenceList relationshipList)
            {
                return relationshipList.Count;
            }

            // Try to get property via reflection
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null)
            {
                return property.GetValue(obj);
            }

            // Handle dictionary-like access
            if (obj is IDictionary dictionary && dictionary.Contains(propertyName))
            {
                return dictionary[propertyName];
            }

            // For IEntity objects, verify if this is a known property
            if (obj is IEntity && !IsKnownEntityProperty(propertyName))
            {
                throw new ArgumentException($"Property '{propertyName}' not found on entity.", nameof(propertyName));
            }

            return null;
        }

        private bool IsKnownEntityProperty(string propertyName)
        {
            // List of known IEntity properties to avoid throwing exceptions for expected properties
            var knownProperties = new[] { 
                "EntityType", "TableName", "Relationships", "Properties", 
                "PrimaryKey", "Description", "SchemaName"
            };
            
            return knownProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<object> ConvertToEnumerable(object obj)
        {
            if (obj == null)
            {
                return Enumerable.Empty<object>();
            }

            if (obj is IEnumerable<object> objectEnumerable)
            {
                return objectEnumerable;
            }

            if (obj is IEnumerable enumerable && !(obj is string))
            {
                return enumerable.Cast<object>();
            }

            // Handle IRelationshipReferenceList specially
            if (obj is IRelationshipReferenceList relationshipList)
            {
                var relationships = new List<object>();
                foreach (var relationship in relationshipList)
                {
                    relationships.Add(relationship);
                }
                return relationships;
            }

            // Single item as a collection of one
            return new[] { obj };
        }
    }
}
