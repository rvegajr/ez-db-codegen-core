using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EzDbCodeGen.Core.Extensions;
using EzDbSchema.Core.Extentions;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Classes
{
    public class DirectoryName : IEquatable<DirectoryName>
    {
        private string _value;
        private int _hashCode;

        public DirectoryName(string d)
        {
            _value = d;
            _hashCode = d.GetStableHashCode();
        }

        public static implicit operator DirectoryName(string d)
        {
            return new DirectoryName(d);
        }

        public static implicit operator string(DirectoryName d)
        {
            return d._value;
        }

        public override bool Equals(object? obj)
        {
            if (obj is DirectoryName other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(DirectoryName? other)
        {
            if (other is null)
            {
                return false;
            }
            return _hashCode == other._hashCode;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return _value;
        }
    }
}
