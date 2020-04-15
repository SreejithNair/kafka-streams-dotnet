﻿using Kafka.Streams.Net.Crosscutting;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kafka.Streams.Net.Processors.Internal
{
    internal class RecordQueue<T>
    {
        private readonly int maxSize;
        private readonly Queue<T> queue;
        private readonly string logPrefix;
        private readonly ILog log = Logger.GetLogger(typeof(RecordQueue<T>));
        private readonly string nameQueue;

        public int Size => queue.Count;
        public int MaxSize => maxSize;

        public RecordQueue(int maxSize, string logPrefix, string nameQueue)
        {
            this.maxSize = maxSize;
            this.logPrefix = logPrefix;
            this.nameQueue = nameQueue;
            this.queue = new Queue<T>(this.maxSize);
        }

        public bool AddRecord(T record)
        {
            if (maxSize >= queue.Count)
            {
                log.Debug($"{logPrefix}Add record in queue {nameQueue}");
                queue.Enqueue(record);
                return true;
            }
            else
            {
                log.Warn($"{logPrefix}Impossible to add record in tempory queue because his max size is attempt.");
                return false;
            }
        }

        public T GetNextRecord() => queue.Peek();

        public void Commit()
        {
            log.Debug($"{logPrefix}Commit record in queue {nameQueue}");
            queue.Dequeue();
        }
    }
}
