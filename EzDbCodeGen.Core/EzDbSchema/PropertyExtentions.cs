using System;
using EzDbSchema.Core.Interfaces;

namespace EzDbCodeGen.Core
{

    /// <summary>
    /// Extention methods to support code generation for EzDbSchema nuget package
    /// </summary>
    public static class EzDbSchemaPropertyExtentions
    {
		/// <summary>
        /// Useful for rendering the primary keys for a comma delimited parameter list
        /// Will return the primary keys in the following format
        ///   [0]Parm1 (number), Parm2(text), Parm3 (number)
        ///   will return
        ///   int Parm1, string Parm2, int Parm3
        /// </summary>
        /// <param name="delimiter">Defaults to ","</param>
        /// <param name="elementSet">Defaults to " "</param>
        /// <returns></returns>
		private static string AsParmString(this IPrimaryKeyProperties This, string prefix, string delimiter, string elementSet)
		{
			var ret = "";
            for (int i = 0; i < This.Count; i++)
            {
                var property = This[i];
                ret += (i > 0 ? delimiter + @" " : @" ") + prefix + property.Type.ToNetType() + elementSet + property.Name.ToSingular();
            }
            return ret;
		}

		/// <summary>
        /// Useful for rendering the primary keys for a comma delimited parameter list
        /// Will return the primary keys in the following format
        ///   [0]Parm1 (number), Parm2(text), Parm3 (number)
        ///   will return
        ///   int Parm1, string Parm2, int Parm3
        /// </summary>
		public static string AsParmString(this IPrimaryKeyProperties This)
        {
			return This.AsParmString("[FromODataUri] ", ",", " ");
        }

        /// <summary>
        /// Useful for rendering the primary keys for a comma delimited parameter list
        /// Will return the primary keys in the following format
        ///   [0]Parm1 (number), Parm2(text), Parm3 (number)
        ///   will return
        ///   int Parm1, string Parm2, int Parm3
        /// </summary>
        /// <param name="delimiter">Defaults to ","</param>
        /// <param name="elementSet">Defaults to " "</param>
        /// <returns></returns>
		private static string AsParmString(this IProperty This, string prefix, string delimiter, string elementSet)
        {
			return This.Parent.PrimaryKeys.AsParmString(prefix, delimiter, elementSet);
        }

        /// <summary>
        /// Useful for rendering the primary keys for a comma delimited parameter list
        /// Will return the primary keys in the following format
        ///   [0]Parm1 (number), Parm2(text), Parm3 (number)
        ///   will return
        ///   int Parm1, string Parm2, int Parm3
        /// </summary>
		public static string AsParmString(this IProperty This )
        {
            return This.AsParmString("[FromODataUri] ", ",", " ");
        }

		/// <summary>
        /// Useful for rendering the primary keys for a linq search query
        /// Will return the primary keys in the following format
        ///   [0]Parm1 (number), Parm2(text), Parm3 (number)
        ///   will return
        ///   t.Parm1 == Parm1 and t.Parm2 == Parm2 and t.Parm3 == Parm3
        /// </summary>
        /// <param name="prefix">Defaults to "t"</param>
        /// <param name="delimiter">Defaults to " and "</param>
        /// <param name="elementSet">Defaults to " == "</param>
		/// <param name="prefixSetter">Defaults to ""</param>
		/// <returns></returns>
		public static string AsLinqEquationString(this IPrimaryKeyProperties This, string prefix, string delimiter, string elementSet, string prefixSetter)
        {
            // t.@_Model[key].PrimaryKeys[0].Name == @key.ToSingular()
            var ret = "";
            for (int i = 0; i < This.Count; i++)
            {
                var property = This[i];
				ret += (i > 0 ? delimiter + @" " : @" ") + prefix + (prefix.Length > 0 ? "." : "") + property.Name + elementSet + prefixSetter + (prefixSetter.Length > 0 ? "." : "") + property.Name.ToSingular();
            }
            return ret;
        }

        /// <summary>
        /// Useful for rendering the primary keys for a linq search query
        /// Will return the primary keys in the following format
        ///   [0]Parm1 (number), Parm2(text), Parm3 (number)
        ///   will return
        ///   t.Parm1 == Parm1 and t.Parm2 == Parm2 and t.Parm3 == Parm3
        /// </summary>
        /// <returns>t.Parm1 == Parm1 and t.Parm2 == Parm2 and t.Parm3 == Parm3</returns>
		public static string AsLinqEquationString(this IPrimaryKeyProperties This)
        {
            return This.AsLinqEquationString("t", " &&", "==", "");
        }
        /// <summary>
        /// Useful for rendering the primary keys for a linq search query
        /// Will return the primary keys in the following format
        ///   [0]Parm1 (number), Parm2(text), Parm3 (number)
        ///   will return
        ///   t.Parm1 == Parm1 and t.Parm2 == Parm2 and t.Parm3 == Parm3
        /// </summary>
        /// <param name="prefix">Defaults to "t"</param>
        /// <param name="delimiter">Defaults to " and "</param>
        /// <param name="elementSet">Defaults to " == "</param>
        /// <returns></returns>
		public static string AsLinqEquationString(this IProperty This, string prefix, string delimiter, string elementSet, string prefixSetter)
        {
			return This.Parent.PrimaryKeys.AsLinqEquationString(prefix, delimiter, elementSet, prefixSetter);
        }

        /// <summary>
        /// Useful for rendering the primary keys for a linq search query
        /// Will return the primary keys in the following format
        ///   [0]Parm1 (number), Parm2(text), Parm3 (number)
        ///   will return
        ///   t.Parm1 == Parm1 and t.Parm2 == Parm2 and t.Parm3 == Parm3
        /// </summary>
        /// <returns>t.Parm1 == Parm1 and t.Parm2 == Parm2 and t.Parm3 == Parm3</returns>
		public static string AsLinqEquationString(this IProperty This )
        {
			return This.AsLinqEquationString("t", " &&", "==", "");
        }
		/// <summary>
		/// This will return the primary keys a an OData Route String
		/// </summary>
		/// <returns>The primary keys <see langword="async"/> an oid.</returns>
		/// <param name="This">This.</param>
		public static string AsODataRouteString(this IProperty This) {
			return This.Parent.PrimaryKeys.AsODataRouteString(); 
		}
    
        /// <summary>
        /// This will return the primary keys a an OData Route String
        /// </summary>
        /// <returns>The primary keys <see langword="async"/> an oid.</returns>
        /// <param name="This">This.</param>
		public static string AsODataRouteString(this IPrimaryKeyProperties This )
        {
            var ret = "";
			if (This.Count == 0)
            {
                return "";
            }
            //To keep previous functionality,  if there is only 1 key,  it will only render the single parm name
			else if (This.Count == 1)
            {
				var property = This[0];
                ret = "{" + property.Name + "}";
            }
            else
            {
				for (int i = 0; i < This.Count; i++)
                {
					var property = This[i];
                    ret += (i > 0 ? @", " : @" ") + property.Name + "={" + property.Name + "}";
                }
            }
            return "(" + ret + ")";
        }

		/// <summary>
		/// Useful for rendering the primary keys for a FindAsync
		/// Will return the primary keys in the following format
		///   [0]Parm1 (number), Parm2(text), Parm3 (number)
		///   will return
		///   Parm1, Parm2, Parm3
		/// </summary>
		/// <param name="varPrefix">What the variable will be prefixed with.</param>
		public static string AsCsvString(this IProperty This, string varPrefix)
		{
			return This.Parent.PrimaryKeys.AsCsvString(varPrefix);
		}

        /// <summary>
        /// Useful for rendering the primary keys for a FindAsync
        /// Will return the primary keys in the following format
        ///   [0]Parm1 (number), Parm2(text), Parm3 (number)
        ///   will return
        ///   Parm1, Parm2, Parm3
        /// </summary>
        /// <param name="varPrefix">What the variable will be prefixed with.</param>
		public static string AsCsvString(this IPrimaryKeyProperties This, string varPrefix)
        {
            var ret = "";
			for (int i = 0; i < This.Count; i++)
            {
				var property = This[i];
                ret += (i > 0 ? @", " : @" ") + ((varPrefix.Length > 0) ? varPrefix + "." : "") + property.Name;
            }
            return ret;
        }

        /// <summary>
		/// Useful for rendering the primary keys for a FindAsync
        /// Will return the primary keys in the following format
        ///   [0]Parm1 (number), Parm2(text), Parm3 (number)
        ///   will return
        ///   Parm1, Parm2, Parm3
        /// </summary>
        /// <param name="varPrefix">What the variable will be prefixed with.</param>
		public static string AsCsvString(this IProperty This)
        {
			return This.AsCsvString("");
        }

		/// <summary>
		/// Useful for rendering the keys for comparing if a request will be valid
		/// Will return the primary keys in the following format
		///   [0]Parm1 (number), Parm2(text), Parm3 (number) with varPrefix=item
		///   will return
		///   (Parm1 == item.Parm1) && (Parm2 == item.Parm2) && (Parm3 == item.Parm3)
		/// </summary>
		/// <param name="varPrefix">What the variable will be prefixed with.</param>
		/// <param name="op">The operator you wish to apply to the operands.. typically '==' or '!=' </param>
		public static string AsParmBooleanCheck(this IProperty This, string varPrefix, string op)
		{
			return This.Parent.PrimaryKeys.AsParmBooleanCheck(varPrefix, op);
		}

        /// <summary>
        /// Useful for rendering the keys for comparing if a request will be valid
        /// Will return the primary keys in the following format
        ///   [0]Parm1 (number), Parm2(text), Parm3 (number) with varPrefix=item
        ///   will return
        ///   (Parm1 == item.Parm1) && (Parm2 == item.Parm2) && (Parm3 == item.Parm3)
        /// </summary>
        /// <param name="varPrefix">What the variable will be prefixed with.</param>
        /// <param name="op">The operator you wish to apply to the operands.. typically '==' or '!=' </param>
		public static string AsParmBooleanCheck(this IPrimaryKeyProperties This, string varPrefix, string op)
        {
            var ret = "";
			for (int i = 0; i < This.Count; i++)
            {
				var property = This[i];
                ret += (i > 0 ? @" && " : @"") + property.Name + " " + op.Trim() + " " + varPrefix + "." + property.Name;
            }
            return ret;
        }

        /// <summary>
        /// Useful for rendering the keys for comparing if a request will be valid
        /// Will return the primary keys in the following format
        ///   [0]Parm1 (number), Parm2(text), Parm3 (number) with varPrefix=item
        ///   will return
        ///   (Parm1 == item.Parm1) && (Parm2 == item.Parm2) && (Parm3 == item.Parm3)
        /// </summary>
        /// <param name="varPrefix">What the variable will be prefixed with.</param>
		public static string AsParmBooleanCheck(this IProperty This, string varPrefix)
        {
			return This.AsParmBooleanCheck(varPrefix, "==");
        }

    }
}
