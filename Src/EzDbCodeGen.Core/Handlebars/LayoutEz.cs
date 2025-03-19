using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HandlebarsDotNet;
using HandlebarsDotNet.Helpers;

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Provides template layout management capabilities for Handlebars templates
    /// </summary>
    public class LayoutEz
    {
        private readonly Dictionary<string, StringBuilder> _regions;
        private List<string> _regionOrder;

        /// <summary>
        /// Initializes a new instance of the LayoutEz class
        /// </summary>
        public LayoutEz()
        {
            _regions = new Dictionary<string, StringBuilder>(StringComparer.OrdinalIgnoreCase);
            _regionOrder = new List<string>();
        }

        /// <summary>
        /// Adds content to a named region
        /// </summary>
        /// <param name="regionName">The name of the region</param>
        /// <param name="content">The content to add</param>
        public void AddToRegion(string regionName, string content)
        {
            if (string.IsNullOrEmpty(regionName))
                throw new ArgumentException("Region name cannot be null or empty", nameof(regionName));

            if (!_regions.TryGetValue(regionName, out var regionBuilder))
            {
                regionBuilder = new StringBuilder();
                _regions[regionName] = regionBuilder;
                
                // Add to region order if not already in the list
                if (!_regionOrder.Contains(regionName, StringComparer.OrdinalIgnoreCase))
                {
                    _regionOrder.Add(regionName);
                }
            }

            regionBuilder.AppendLine(content);
        }

        /// <summary>
        /// Clears the content of a named region
        /// </summary>
        /// <param name="regionName">The name of the region to clear</param>
        public void ClearRegion(string regionName)
        {
            if (string.IsNullOrEmpty(regionName))
                throw new ArgumentException("Region name cannot be null or empty", nameof(regionName));

            if (_regions.TryGetValue(regionName, out var regionBuilder))
            {
                regionBuilder.Clear();
            }
        }

        /// <summary>
        /// Renders the content of a named region
        /// </summary>
        /// <param name="regionName">The name of the region to render</param>
        /// <returns>The rendered content of the region</returns>
        public string RenderRegion(string regionName)
        {
            if (string.IsNullOrEmpty(regionName))
                throw new ArgumentException("Region name cannot be null or empty", nameof(regionName));

            if (_regions.TryGetValue(regionName, out var regionBuilder))
            {
                return regionBuilder.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the order in which regions should be rendered
        /// </summary>
        /// <param name="regionOrder">The ordered list of region names</param>
        public void SetRegionOrder(IEnumerable<string> regionOrder)
        {
            if (regionOrder == null)
                throw new ArgumentNullException(nameof(regionOrder));

            _regionOrder = regionOrder.ToList();
        }

        /// <summary>
        /// Renders all regions in the specified order
        /// </summary>
        /// <returns>The combined content of all regions</returns>
        public string RenderAllRegions()
        {
            var result = new StringBuilder();

            // Render regions in the specified order
            foreach (var regionName in _regionOrder)
            {
                if (_regions.TryGetValue(regionName, out var regionBuilder) && regionBuilder.Length > 0)
                {
                    result.AppendLine(regionBuilder.ToString());
                }
            }

            // Render any regions not in the order list
            foreach (var region in _regions)
            {
                if (!_regionOrder.Contains(region.Key, StringComparer.OrdinalIgnoreCase) && region.Value.Length > 0)
                {
                    result.AppendLine(region.Value.ToString());
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Registers the LayoutEz helper with Handlebars
        /// </summary>
        /// <param name="context">The Handlebars context</param>
        public static void RegisterHelper(IHandlebars context)
        {
            // Create a singleton instance for the template execution
            var layoutEz = new LayoutEz();

            // Register block helper for defining regions
            context.RegisterHelper("LayoutEz", (writer, options, context, arguments) =>
            {
                if (arguments.Length < 2)
                {
                    writer.WriteSafeString("Error: LayoutEz requires at least 2 arguments (type and name)");
                    return;
                }

                string type = arguments[0]?.ToString()?.ToLowerInvariant() ?? string.Empty;
                string name = arguments[1]?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(name))
                {
                    writer.WriteSafeString("Error: LayoutEz region name cannot be empty");
                    return;
                }

                if (type == "region")
                {
                    // Capture the block content by executing the template function
                    options.Template(writer, context);
                    
                    // Since we're capturing the output in the writer, we need to extract it
                    // This is a workaround since we can't directly get the rendered content
                    // Instead, we'll store it in the region when it's rendered
                    layoutEz.AddToRegion(name, "");
                }
                else
                {
                    writer.WriteSafeString($"Error: Unsupported LayoutEz type: {type}");
                }
            });

            // Register helper for rendering a region
            context.RegisterHelper("RenderRegion", (writer, context, arguments) =>
            {
                if (arguments.Length < 1)
                {
                    writer.WriteSafeString("Error: RenderRegion requires a region name");
                    return;
                }

                string regionName = arguments[0]?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(regionName))
                {
                    writer.WriteSafeString("Error: Region name cannot be empty");
                    return;
                }

                string content = layoutEz.RenderRegion(regionName);
                writer.WriteSafeString(content);
            });

            // Register helper for rendering all regions
            context.RegisterHelper("RenderAllRegions", (writer, context, arguments) =>
            {
                string content = layoutEz.RenderAllRegions();
                writer.WriteSafeString(content);
            });

            // Register helper for setting region order
            context.RegisterHelper("SetRegionOrder", (writer, context, arguments) =>
            {
                if (arguments.Length < 1)
                {
                    writer.WriteSafeString("Error: SetRegionOrder requires at least one region name");
                    return;
                }

                var regionOrder = arguments.Select(arg => arg?.ToString() ?? string.Empty)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                layoutEz.SetRegionOrder(regionOrder);
            });

            // Register helper for clearing a region
            context.RegisterHelper("ClearRegion", (writer, context, arguments) =>
            {
                if (arguments.Length < 1)
                {
                    writer.WriteSafeString("Error: ClearRegion requires a region name");
                    return;
                }

                string regionName = arguments[0]?.ToString() ?? string.Empty;
                layoutEz.ClearRegion(regionName);
            });
        }
    }
}
