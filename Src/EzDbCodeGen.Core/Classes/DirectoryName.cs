using System;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbSchema.Core.Extentions;

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
        public override bool Equals(object obj)
        {
            return Equals(obj as FileName);
        }
        public bool Equals(FileName obj)
        {
            return obj != null && obj.GetHashCode() == this.GetHashCode();
        }
    }
}
