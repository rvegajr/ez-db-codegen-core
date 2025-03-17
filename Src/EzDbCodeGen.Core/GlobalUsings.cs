// Global using directives
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Runtime.CompilerServices;
global using Microsoft.Data.SqlClient;
global using Newtonsoft.Json;
global using HandlebarsDotNet;
global using EzDbCodeGen.Core.Extensions;
global using EzDbCodeGen.Core.Extentions;
global using EzDbCodeGen.Core.Extentions.Objects;
global using EzDbCodeGen.Core.Config;
global using EzDbCodeGen.Core.Enums;
global using EzDbCodeGen.Core.Classes;
global using EzDbCodeGen.Core.Compare;
global using EzDbCodeGen.Internal;
global using EzDbSchema.Core;
global using EzDbSchema.Core.Interfaces;
global using EzDbSchema.Core.Objects;
global using EzDbSchema.Core.Enums;
global using EzDbSchema.MsSql;
global using CoreInterfaces = EzDbSchema.Core.Interfaces;
global using CoreEnums = EzDbSchema.Core.Enums;
global using CoreObjects = EzDbSchema.Core.Objects;
global using System.Text.RegularExpressions;
global using System.Globalization;
global using Pluralize.NET;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]
