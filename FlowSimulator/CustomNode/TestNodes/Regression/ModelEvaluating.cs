using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;
using FlowSimulator.MLSamples.Regression.TaxiFarePrediction.DataStructures;
using System;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace FlowSimulator.CustomNode.TestNodes.Regression
{
    [Category("Тестовые/Регрессия"), Name("Оценка Точности")]
    public class ModelEvaluating: ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out,
            ModelIn,
            TestDataIn,
            Result
        }

        public override string Title => "Оценка Точности";

        public ModelEvaluating(XmlNode node_) : base(node_)
        {

        }

        public ModelEvaluating()
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
                //dynamic InModel = GetValueFromSlot((int)NodeSlotId.ModelIn);
                //dynamic trainedModel = InModel.trainedModel;

                dynamic trainedModel = GetValueFromSlot((int)NodeSlotId.ModelIn);
                dynamic testDataView = GetValueFromSlot((int)NodeSlotId.TestDataIn);
                IDataView predictions = trainedModel.Transform(testDataView);
                var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");
                PrintRegressionMetrics(metrics);
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
            return new ModelEvaluating();
        }

        public static void PrintRegressionMetrics(Microsoft.ML.Data.RegressionMetrics metrics)
        {
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"Оценка точности модели");
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"*************************************************");
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"*       LossFn:        {metrics.LossFunction:0.##}");
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"*       R2 Score:      {metrics.RSquared:0.##}");
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"*       Absolute loss: {metrics.MeanAbsoluteError:#.##}");
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"*       Squared loss:  {metrics.MeanSquaredError:#.##}");
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"*       RMS loss:      {metrics.RootMeanSquaredError:#.##}");
            LogManager.Instance.WriteLine(LogVerbosity.Info, $"*************************************************");

        }

    }
}
