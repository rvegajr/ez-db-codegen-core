using System;
using EzDbCodeGen.Core.Extensions;
using EzDbSchema.Core.Extentions;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core
{
    public class DirectoryName
    {
        readonly string _value;
        public DirectoryName(string value)
        {
            this._value = value;
        }
        public static implicit operator string(DirectoryName d)
        {
            return d._value;
        }
        public static implicit operator DirectoryName(string d)
        {
            return new DirectoryName(d);
        }
        public override int GetHashCode()
        {
            return _value.GetStableHashCode();
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as DirectoryName);
        }
        public bool Equals(DirectoryName? obj)
        {
            return obj != null && obj.GetHashCode() == this.GetHashCode();
        }
    }
}
