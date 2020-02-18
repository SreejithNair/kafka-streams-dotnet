﻿using kafka_stream_core.Processors.Internal;

namespace kafka_stream_core.Stream.Internal.Graph.Nodes
{
    internal class RootNode : StreamGraphNode
    {
        public RootNode() : base("ROOT-NODE")
        {
        }

        public override void writeToTopology(InternalTopologyBuilder builder)
        {
        }
    }
}