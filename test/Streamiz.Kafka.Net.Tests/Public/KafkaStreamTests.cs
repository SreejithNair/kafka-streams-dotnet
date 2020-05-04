﻿using NUnit.Framework;
using Streamiz.Kafka.Net.Errors;
using Streamiz.Kafka.Net.Mock.Kafka;
using Streamiz.Kafka.Net.Mock.Sync;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.State;
using Streamiz.Kafka.Net.Table;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Streamiz.Kafka.Net.Tests.Public
{
    public class KafkaStreamTests
    {
        #region State Test

        [Test]
        public void TestCreatedState()
        {
            var state = KafkaStream.State.CREATED;
            Assert.AreEqual(0, state.Ordinal);
            Assert.AreEqual("CREATED", state.Name);
            Assert.AreEqual(new HashSet<int> { 1, 3 }, state.Transitions);
            Assert.IsFalse(state.IsRunning());
            Assert.IsFalse(state.IsValidTransition(KafkaStream.State.ERROR));
        }

        [Test]
        public void TestErrorState()
        {
            var state = KafkaStream.State.ERROR;
            Assert.AreEqual(5, state.Ordinal);
            Assert.AreEqual("ERROR", state.Name);
            Assert.AreEqual(new HashSet<int> { 3 }, state.Transitions);
            Assert.IsFalse(state.IsRunning());
            Assert.IsFalse(state.IsValidTransition(KafkaStream.State.CREATED));
            Assert.IsTrue(state.IsValidTransition(KafkaStream.State.PENDING_SHUTDOWN));
        }

        [Test]
        public void TestNotRunningState()
        {
            var state = KafkaStream.State.NOT_RUNNING;
            Assert.AreEqual(4, state.Ordinal);
            Assert.AreEqual("NOT_RUNNING", state.Name);
            Assert.AreEqual(new HashSet<int> { }, state.Transitions);
            Assert.IsFalse(state.IsRunning());
            Assert.IsFalse(state.IsValidTransition(KafkaStream.State.ERROR));
        }

        [Test]
        public void TestPendingShutdownState()
        {
            var state = KafkaStream.State.PENDING_SHUTDOWN;
            Assert.AreEqual(3, state.Ordinal);
            Assert.AreEqual("PENDING_SHUTDOWN", state.Name);
            Assert.AreEqual(new HashSet<int> { 4 }, state.Transitions);
            Assert.IsFalse(state.IsRunning());
            Assert.IsFalse(state.IsValidTransition(KafkaStream.State.CREATED));
            Assert.IsTrue(state.IsValidTransition(KafkaStream.State.NOT_RUNNING));
        }

        [Test]
        public void TestRebalancingState()
        {
            var state = KafkaStream.State.REBALANCING;
            Assert.AreEqual(1, state.Ordinal);
            Assert.AreEqual("REBALANCING", state.Name);
            Assert.AreEqual(new HashSet<int> { 2, 3, 5 }, state.Transitions);
            Assert.IsTrue(state.IsRunning());
            Assert.IsFalse(state.IsValidTransition(KafkaStream.State.CREATED));
            Assert.IsTrue(state.IsValidTransition(KafkaStream.State.RUNNING));
        }

        [Test]
        public void TestRunningState()
        {
            var state = KafkaStream.State.RUNNING;
            Assert.AreEqual(2, state.Ordinal);
            Assert.AreEqual("RUNNING", state.Name);
            Assert.AreEqual(new HashSet<int> { 1, 2, 3, 5 }, state.Transitions);
            Assert.IsTrue(state.IsRunning());
            Assert.IsFalse(state.IsValidTransition(KafkaStream.State.NOT_RUNNING));
            Assert.IsTrue(state.IsValidTransition(KafkaStream.State.PENDING_SHUTDOWN));
        }

        #endregion

        [Test]
        public void StartKafkaStream()
        {
            var source = new CancellationTokenSource();
            var config = new StreamConfig<StringSerDes, StringSerDes>();
            config.ApplicationId = "test";

            var builder = new StreamBuilder();
            builder.Stream<string, string>("topic").To("topic2");

            var t = builder.Build();
            var stream = new KafkaStream(t, config, new SyncKafkaSupplier());
            stream.Start(source.Token);
            Thread.Sleep(1500);
            source.Cancel();
            stream.Close();
        }

        [Test]
        public void StartKafkaStreamWaitRunningState()
        {
            var timeout = TimeSpan.FromSeconds(10);
            var source = new CancellationTokenSource();
            bool isRunningState = false;
            DateTime dt = DateTime.Now;

            var config = new StreamConfig<StringSerDes, StringSerDes>();
            config.ApplicationId = "test";

            var builder = new StreamBuilder();
            builder.Stream<string, string>("topic").To("topic2");

            var t = builder.Build();
            var stream = new KafkaStream(t, config, new SyncKafkaSupplier());

            stream.StateChanged += (old, @new) =>
            {
                if (@new.Equals(KafkaStream.State.RUNNING))
                    isRunningState = true;
            };
            stream.Start(source.Token);
            while (!isRunningState)
            {
                Thread.Sleep(250);
                if (DateTime.Now > dt + timeout)
                    break;
            }
            source.Cancel();
            stream.Close();
            Assert.IsTrue(isRunningState);
        }

        [Test]
        public void GetStateStore()
        {
            var timeout = TimeSpan.FromSeconds(10);
            var source = new CancellationTokenSource();
            bool isRunningState = false;
            DateTime dt = DateTime.Now;

            var config = new StreamConfig<StringSerDes, StringSerDes>();
            config.ApplicationId = "test";

            var builder = new StreamBuilder();
            builder.Table("topic", InMemory<string, string>.As("store"));

            var t = builder.Build();
            var stream = new KafkaStream(t, config, new SyncKafkaSupplier());

            stream.StateChanged += (old, @new) =>
            {
                if (@new.Equals(KafkaStream.State.RUNNING))
                    isRunningState = true;
            };
            stream.Start(source.Token);
            while (!isRunningState)
            {
                Thread.Sleep(250);
                if (DateTime.Now > dt + timeout)
                    break;
            }
            Assert.IsTrue(isRunningState);

            if (isRunningState)
            {
                var store = stream.Store(StoreQueryParameters.FromNameAndType("store", QueryableStoreTypes.KeyValueStore<string, string>())); ;
                Assert.IsNotNull(store);
            }

            source.Cancel();
            stream.Close();
        }

        [Test]
        public void GetStateStoreDoesntExists()
        {
            var timeout = TimeSpan.FromSeconds(10);
            var source = new CancellationTokenSource();
            bool isRunningState = false;
            DateTime dt = DateTime.Now;
            var supplier = new SyncKafkaSupplier();

            var config = new StreamConfig<StringSerDes, StringSerDes>();
            config.ApplicationId = "test";

            var builder = new StreamBuilder();
            builder.Table("topic", InMemory<string, string>.As("store"));

            var t = builder.Build();
            var stream = new KafkaStream(t, config, supplier);

            stream.StateChanged += (old, @new) =>
            {
                if (@new.Equals(KafkaStream.State.RUNNING))
                    isRunningState = true;
            };
            stream.Start(source.Token);
            while (!isRunningState)
            {
                Thread.Sleep(250);
                if (DateTime.Now > dt + timeout)
                    break;
            }
            Assert.IsTrue(isRunningState);

            if (isRunningState)
            {
                Assert.Throws<InvalidStateStoreException>(() => stream.Store(StoreQueryParameters.FromNameAndType("stodfdsfdsfre", QueryableStoreTypes.KeyValueStore<string, string>())));
            }

            source.Cancel();
            stream.Close();
        }

        [Test]
        public void GetElementInStateStore()
        {
            var timeout = TimeSpan.FromSeconds(10);
            var source = new CancellationTokenSource();
            bool isRunningState = false;
            DateTime dt = DateTime.Now;

            var config = new StreamConfig<StringSerDes, StringSerDes>();
            config.ApplicationId = "test";
            config.PollMs = 10;
            var supplier = new SyncKafkaSupplier();
            var producer = supplier.GetProducer(config.ToProducerConfig());

            var builder = new StreamBuilder();
            builder.Table("topic", InMemory<string, string>.As("store"));

            var t = builder.Build();
            var stream = new KafkaStream(t, config, supplier);

            stream.StateChanged += (old, @new) =>
            {
                if (@new.Equals(KafkaStream.State.RUNNING))
                    isRunningState = true;
            };
            stream.Start(source.Token);
            while (!isRunningState)
            {
                Thread.Sleep(250);
                if (DateTime.Now > dt + timeout)
                    break;
            }
            Assert.IsTrue(isRunningState);

            if (isRunningState)
            {
                var serdes = new StringSerDes();
                producer.Produce("topic",
                    new Confluent.Kafka.Message<byte[], byte[]>
                    {
                        Key = serdes.Serialize("key1"),
                        Value = serdes.Serialize("coucou")
                    });
                Thread.Sleep(50);
                var store = stream.Store(StoreQueryParameters.FromNameAndType("store", QueryableStoreTypes.KeyValueStore<string, string>()));
                Assert.IsNotNull(store);
                Assert.AreEqual(1, store.ApproximateNumEntries());
                var item = store.Get("key1");
                Assert.IsNotNull(item);
                Assert.AreEqual("coucou", item);
            }

            source.Cancel();
            stream.Close();
        }

        [Test]
        public void GetStateStoreBeforeRunningState()
        {
            var source = new CancellationTokenSource();

            var config = new StreamConfig<StringSerDes, StringSerDes>();
            config.ApplicationId = "test";

            var builder = new StreamBuilder();
            builder.Table("topic", InMemory<string, string>.As("store"));

            var t = builder.Build();
            var stream = new KafkaStream(t, config, new SyncKafkaSupplier());
            Assert.Throws<IllegalStateException>(() => stream.Store(StoreQueryParameters.FromNameAndType("store", QueryableStoreTypes.KeyValueStore<string, string>())));
            source.Cancel();
            stream.Close();
        }
    }
}