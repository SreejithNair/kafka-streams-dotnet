﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace kafka_stream_core.Crosscutting
{
    public class BytesComparer : IEqualityComparer<Bytes>
    {
        public bool Equals(Bytes x, Bytes y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Bytes obj)
        {
            return obj.GetHashCode();
        }
    }

    public class Bytes : IEquatable<Bytes>
    {
        public byte[] Get { get; }

        public Bytes(byte[] bytes)
        {
            this.Get = bytes;
        }

        public override int GetHashCode()
        {
            return new BigInteger(this.Get).GetHashCode();
        }

        public bool Equals(Bytes other)
        {
            return this.Get.SequenceEqual(other.Get);
        }
    }
}