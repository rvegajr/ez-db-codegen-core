using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EzDbCodeGen.Core.Extentions.Objects
{
    public static class HandlebarsExtentions
    {//EncodedTextWriter output, Context context, Arguments arguments
        /// <summary>
        /// Will search an object array and safely return a string.  If the item doesn't exist, this will return 
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="Arguments">arguments that has the item to return</param>
        /// <param name="index">IIndex to return</param>
        public static string AsString(this Arguments arguments, int index)
        {
            if (arguments.Count() > index)
            {
                if (arguments[index] != null)
                {
                    return arguments[index].ToString();
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
    }
}
