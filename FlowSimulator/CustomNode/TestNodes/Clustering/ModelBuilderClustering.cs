using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;
using System;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using FlowSimulator.MLSamples.Clustering.CustomerSegmentation.DataStructures;

namespace FlowSimulator.CustomNode.TestNodes.Clustering
{
    [Category("Тестовые/Кластеризация"), Name("Обучение без учителя")]
    public class ModelBuilderClustering: ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out,
            TrainerIn,
            TrainingDataIn,
            Result,
            ClustNum

        }

        public override string Title => "Обучение без учителя";

        public ModelBuilderClustering(XmlNode node_) : base(node_)
        {

        }

        public ModelBuilderClustering()
        {

        }


        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);
            AddSlot((int)NodeSlotId.TrainingDataIn, "Выборка", SlotType.VarIn, typeof(IDataView));
            AddSlot((int)NodeSlotId.TrainerIn, "Алгоритм", SlotType.VarIn, typeof(IEstimator<ITransformer>));
            AddSlot((int)NodeSlotId.Result, "Модель", SlotType.VarOut, typeof(ITransformer));
            AddSlot((int)NodeSlotId.ClustNum, "", SlotType.VarIn, typeof(int));
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
                LogManager.Instance.WriteLine(LogVerbosity.Info, $"Началось обучение модели...");

                var dataProcessPipeline = mlContext.Transforms.ProjectToPrincipalComponents(outputColumnName: "PCAFeatures", inputColumnName: "Features", rank: 2)
                 .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "LastNameKey", inputColumnName: nameof(PivotData.LastName), OneHotEncodingEstimator.OutputKind.Indicator));

                dynamic trainer = GetValueFromSlot((int)NodeSlotId.TrainerIn);
                var trainingPipeline = dataProcessPipeline.Append(trainer);

                dynamic trainingDataView = GetValueFromSlot((int)NodeSlotId.TrainingDataIn);

                var trainedModel = trainingPipeline.Fit(trainingDataView);

                LogManager.Instance.WriteLine(LogVerbosity.Info, $"Обучение модели закончено");

                SetValueInSlot((int)NodeSlotId.Result, trainedModel);

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
            return new ModelBuilderClustering();
        }
    }
}
