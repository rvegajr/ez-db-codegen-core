using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Enums
{

    public enum ReturnCode
    {
        Ok = 0,
        OkAddDels = 0,
        OkNoAddDels = 1,
        Error = 100
    }

    public class ReturnCodes : Dictionary<string, ReturnCode> {
        public ReturnCode Result
        {
            get
            {
                var ret = ReturnCode.Ok;
                foreach (var rc in this.Values) if (rc > ret) ret = rc;
                return ret;
            }
        }
        public ReturnCodes(string TemplateFileNameProcessed, ReturnCode rc)
        {
            this.Add(TemplateFileNameProcessed, rc);
        }
        public ReturnCodes()
        {
        }
        /// <summary>
        /// Merges a passed ReturnCodes object with this object
        /// </summary>
        /// <param name="rcs">Another ReturnCodes Object</param>
        /// <returns>This instance</returns>
        public ReturnCodes Merge(ReturnCodes rcs)
        {
            foreach (var rcKey in rcs.Keys) this.Add(rcKey, rcs[rcKey]);
            return this;
        }
    }

    public enum TemplateFileAction
    {
        None,
        Update,
        Delete,
        Add,
        Unknown
    }

}
