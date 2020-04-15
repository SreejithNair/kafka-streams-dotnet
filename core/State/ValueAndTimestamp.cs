﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Kafka.Streams.Net.State
{
    public class ValueAndTimestamp<V>
    {
        public V Value { get; private set; }
        public long Timestamp { get; private set; }

        private ValueAndTimestamp(long timestamp, V value)
        {
            this.Timestamp = timestamp;
            this.Value = value;
        }

        public static ValueAndTimestamp<V> Make(V value, long timestamp)
        {
            return value == null ? null : new ValueAndTimestamp<V>(timestamp, value);
        }
    }
}
