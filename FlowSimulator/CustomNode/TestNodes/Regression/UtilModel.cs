using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;
using FlowSimulator.MLSamples.Regression.TaxiFarePrediction.DataStructures;
using System;
using Microsoft.ML;

namespace FlowSimulator.CustomNode.TestNodes.Regression
{
    struct Model
    {
        public IDataView trainingDataView;
        public ITransformer trainedModel;
    }
}
