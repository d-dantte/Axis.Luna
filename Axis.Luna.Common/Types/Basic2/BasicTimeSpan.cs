﻿using System;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    public struct BasicTimeSpan : IBasicValue<TimeSpan?>
    {
        private readonly BasicMetadata[] _metadata;

        public BasicTypes Type => BasicTypes.TimeSpan;

        public TimeSpan? Value { get; }

        public BasicMetadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<BasicMetadata>();

        public BasicTimeSpan(TimeSpan? value) : this(value, Array.Empty<BasicMetadata>())
        { }

        public BasicTimeSpan(TimeSpan? value, params BasicMetadata[] metadata)
        {
            Value = value;
            _metadata = metadata?.Length > 0 == true
                ? metadata.ToArray()
                : null;
        }

        public override bool Equals(object obj)
            => obj is BasicTimeSpan other
             && other.Value == Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();


        public static bool operator ==(BasicTimeSpan first, BasicTimeSpan second) => first.Value == second.Value;

        public static bool operator !=(BasicTimeSpan first, BasicTimeSpan second) => !(first == second);
    }
}
