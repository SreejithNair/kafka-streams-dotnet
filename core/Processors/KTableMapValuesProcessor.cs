﻿using kafka_stream_core.State;
using kafka_stream_core.Stream;
using kafka_stream_core.Table.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace kafka_stream_core.Processors
{
    internal class KTableMapValuesProcessor<K, V, VR> : AbstractKTableProcessor<K, V, K, VR>
    {
        private readonly IValueMapperWithKey<K, V, VR> mapper;

        public KTableMapValuesProcessor(IValueMapperWithKey<K, V, VR> mapper, bool sendOldValues, string queryableStoreName)
            : base(queryableStoreName, sendOldValues)
        {
            this.mapper = mapper;
        }

        public override object Clone()
        {
            var p = new KTableMapValuesProcessor<K, V, VR>(this.mapper, this.sendOldValues, this.queryableStoreName);
            p.StateStores = new List<string>(this.StateStores);
            return p;
        }

        public override void Process(K key, Change<V> change)
        {
            VR newValue = computeValue(key, change.NewValue);
            VR oldValue = this.sendOldValues ? computeValue(key, change.OldValue) : default(VR);

            if (this.queryableStoreName != null)
            {
                store.put(key, ValueAndTimestamp<VR>.make(newValue, Context.Timestamp));
                //tupleForwarder.maybeForward(key, newValue, oldValue);
            }
            else
            {
                this.Forward(key, new Change<VR>(oldValue, newValue));
            }
        }

        private VR computeValue(K key, V value)
        {
            VR newValue = default(VR);

            if (value != null)
            {
                newValue = mapper.apply(key, value);
            }

            return newValue;
        }
    }
}