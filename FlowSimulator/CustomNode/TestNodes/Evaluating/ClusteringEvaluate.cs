using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;
using System;
using Microsoft.ML;

namespace FlowSimulator.CustomNode.TestNodes.Evaluating
{
    [Category("Оценка модели"), Name("Оценка Кластеризации")]
    public class ClusteringEvaluate: ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out,
            ModelIn,
            TestDataIn,
            Result
        }

        public override string Title => "Оценка Модели";

        public ClusteringEvaluate(XmlNode node_) : base(node_)
        {

        }

        public ClusteringEvaluate()
        {

        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);
            AddSlot((int)NodeSlotId.ModelIn, "Модель", SlotType.VarIn, typeof(ITransformer));
            AddSlot((int)NodeSlotId.TestDataIn, "Данные", SlotType.VarIn, typeof(IDataView));
        }

        public override ProcessingInfo ActivateLogic(ProcessingContext context, NodeSlot slot)
        {
            ProcessingInfo info = new ProcessingInfo
            {
                State = LogicState.Ok
            };

            MLContext mlContext = new MLContext();

            try
            {
                dynamic trainedModel = GetValueFromSlot((int)NodeSlotId.ModelIn);
                dynamic testDataView = GetValueFromSlot((int)NodeSlotId.TestDataIn);
                IDataView predictions = trainedModel.Transform(testDataView);
                var metrics = mlContext.Clustering.Evaluate(predictions, scoreColumnName: "Score", featureColumnName: "Features");
                PrintClusteringMetrics(metrics);
                ActivateOutputLink(context, (int)NodeSlotId.Out);
            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteLine(LogVerbosity.Error, "Недопустимое значение входных данных.");
            }

            return info;
        }

        protected override SequenceNode CopyImpl()
        {
            return new ClusteringEvaluate();
        }

        public static void PrintClusteringMetrics(Microsoft.ML.Data.ClusteringMetrics metrics)
        {
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"Оценка точности модели: ");
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"*************************************************");
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"*       Среднее Расстояние: {metrics.AverageDistance}");
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"*       Индекс Дэвиса–Булдина: {metrics.DaviesBouldinIndex}");
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"*************************************************");

        }
    }
}
