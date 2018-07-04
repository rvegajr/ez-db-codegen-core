using System;
namespace EzDbCodeGen.Core.Enums
{

    public enum ReturnCode
    {
        Ok = 0,
        OkAddDels = 0,
        OkNoAddDels = 1,
        Error = 100
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
