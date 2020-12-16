using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Extentions.Objects
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Will search an object array and safely return a string.  If the item doesn't exist, this will return 
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="objectArray">Object array that has the item to return</param>
        /// <param name="index">IIndex to return</param>
        public static string AsString(this object[] objectArray, int index)
        {
            if (objectArray.Count() > index)
            {
                if (objectArray[index] != null)
                {
                    return objectArray[index].ToString();
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
        /// <summary>
        /// This will return a string, but if the variable is null, it will return a zero length string.
        /// </summary>
        /// <returns>The safe string.</returns>
        /// <param name="word">Word.</param>
        public static string ToSafeString(this object word)
        {
            return (word == null) ? "" : word.ToString();
        }
        /// <summary>
        /// Ases the nullable boolean.
        /// </summary>
        /// <returns>The nullable boolean.</returns>
        /// <param name="obj">Object.</param>
        public static bool? AsNullableBoolean(this object obj)
        {
            bool? ret = null;
            if (obj != null)
            {
                var objAsStr = obj.ToString();
                ret = (objAsStr == "1") || (objAsStr.ToUpper().StartsWith("T")) || (objAsStr.ToUpper().StartsWith("Y"));
            }
            return ret;
        }
        /// <summary>
        /// Ases the boolean.
        /// </summary>
        /// <returns><c>true</c>, if boolean was ased, <c>false</c> otherwise.</returns>
        /// <param name="obj">Object.</param>
        public static bool AsBoolean(this object obj)
        {
            bool ret = false;
            if (obj != null)
            {
                var objAsStr = obj.ToString();
                ret = (objAsStr == "1") || (objAsStr.ToUpper().StartsWith("T")) || (objAsStr.ToUpper().StartsWith("Y"));
            }
            return ret;
        }
        /// <summary>
        /// Returns an object as an Int
        /// </summary>
        /// <returns>The int.</returns>
        /// <param name="obj">Object.</param>
        /// <param name="ValueIfNull">Value if null.</param>
        public static int AsInt(this object obj, int ValueIfNull)
        {
            int ret = ValueIfNull;
            if (obj != null)
            {
                if (obj.ToString().Contains("."))
                {
                    Double temp;
                    Boolean isOk = Double.TryParse(obj.ToString(), out temp);
                    ret = isOk ? (int)temp : 0;
                }
                else
                {
                    int.TryParse(obj.ToString(), out ret);
                }
            }
            return ret;
        }
        /// <summary>
        /// Ases the int nullable.
        /// </summary>
        /// <returns>The int nullable.</returns>
        /// <param name="obj">Object.</param>
        /// <param name="ValueIfNull">Value if null.</param>
        public static int? AsIntNullable(this object obj, int? ValueIfNull)
        {
            int? ret = ValueIfNull;
            if (obj != null)
            {
                if (obj.ToString().Contains("."))
                {
                    Double temp;
                    Boolean isOk = Double.TryParse(obj.ToString(), out temp);
                    ret = isOk ? (int?)temp : 0;
                }
                else
                {
                    int i = 0;
                    int.TryParse(obj.ToString(), out i);
                    ret = (int?)i;
                }
            }
            return ret;
        }
        /// <summary>
        /// Will return the variable as a DateTime
        /// </summary>
        /// <returns>The date time.</returns>
        /// <param name="obj">Object.</param>
        /// <param name="ValueIfNull">Value if null.</param>
        public static DateTime AsDateTime(this object obj, DateTime ValueIfNull)
        {
            DateTime ret = ValueIfNull;
            if (obj != null)
            {
                DateTime temp;
                Boolean isOk = DateTime.TryParse(obj.ToString(), out temp);
                ret = isOk ? temp : ValueIfNull;

            }
            return ret;
        }

        /// <summary>
        /// Return the object as a AsDateTimeNullable
        /// </summary>
        /// <returns>The date time nullable.</returns>
        /// <param name="obj">Object.</param>
        /// <param name="ValueIfNull">Value if null.</param>
        public static DateTime? AsDateTimeNullable(this object obj, DateTime? ValueIfNull)
        {
            DateTime? ret = ValueIfNull;
            if (obj != null)
            {
                DateTime temp;
                Boolean isOk = DateTime.TryParse(obj.ToString(), out temp);
                ret = isOk ? temp : ValueIfNull;

            }
            return ret;
        }
        /// <summary>
        /// This will return the variable as a Double 
        /// </summary>
        /// <returns>The double.</returns>
        /// <param name="obj">Object.</param>
        /// <param name="ValueIfNull">Value if null.</param>
        public static double AsDouble(this object obj, double ValueIfNull)
        {
            double ret = ValueIfNull;
            if (obj != null)
            {
                Double temp;
                Boolean isOk = Double.TryParse(obj.ToString(), out temp);
                ret = isOk ? (double)temp : 0;
            }
            return ret;
        }
    }
}
