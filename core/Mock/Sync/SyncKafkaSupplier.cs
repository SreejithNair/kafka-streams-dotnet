﻿using Confluent.Kafka;
using Streamiz.Kafka.Net.Kafka;
using System.Collections.Generic;
using System.Linq;

namespace Streamiz.Kafka.Net.Mock.Sync
{
    internal class SyncKafkaSupplier : IKafkaSupplier
    {
        private SyncProducer producer = null;

        public IAdminClient GetAdmin(AdminClientConfig config) => new SyncAdminClient();

        public IConsumer<byte[], byte[]> GetConsumer(ConsumerConfig config, IConsumerRebalanceListener rebalanceListener)
        {
            var consumer = new SyncConsumer(config, producer);
            consumer.SetRebalanceListener(rebalanceListener);
            return consumer;
        }

        public IProducer<byte[], byte[]> GetProducer(ProducerConfig config)
        {
            if(producer == null)
                producer = new SyncProducer(config);
            return producer;
        }

        public IConsumer<byte[], byte[]> GetRestoreConsumer(ConsumerConfig config)
            => GetConsumer(config, null);
    }
}