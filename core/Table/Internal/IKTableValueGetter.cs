﻿using Kafka.Streams.Net.State;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kafka.Streams.Net.Table.Internal
{
    internal interface IKTableValueGetter<K,V>
    {
        void Init(ProcessorContext context);

        ValueAndTimestamp<V> Get(K key);

        void Close();
    }
}
