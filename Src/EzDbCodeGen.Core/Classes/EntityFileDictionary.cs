using System;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbSchema.Core.Extentions;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core
{
    internal class EntityFileDictionary : MultiKeyDictionary<FileName, EntityName, string>
    {
        public EntityFileDictionary()
        {
        }
    }

    public class FileName
    {
        readonly string _value;
        public FileName(string value)
        {
            this._value = value;
        }
        public static implicit operator string(FileName d)
        {
            return d._value;
        }
        public static implicit operator FileName(string d)
        {
            return new FileName(d);
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

    internal class EntityName
    {
        readonly string _value;
        public EntityName(string value)
        {
            this._value = value;
        }
        public static implicit operator string(EntityName d)
        {
            return d._value;
        }
        public static implicit operator EntityName(string d)
        {
            return new EntityName(d);
        }

        public override int GetHashCode()
        {
            return _value.GetStableHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as EntityName);
        }
        public bool Equals(EntityName obj)
        {
            return obj != null && obj.GetHashCode() == this.GetHashCode();
        }

    }
}
