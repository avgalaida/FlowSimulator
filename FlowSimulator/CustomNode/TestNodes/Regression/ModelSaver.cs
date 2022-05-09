using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;
using FlowSimulator.MLSamples.Regression.TaxiFarePrediction.DataStructures;
using System;
using Microsoft.ML;
using System.IO;

namespace FlowSimulator.CustomNode.TestNodes.Regression
{
    [Category("Тестовые/Регрессия"), Name("Сохранение модели")]
    public class ModelSaver: ActionNode
    {
        private static string BaseModelsRelativePath = @"../../MLSamples/MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/TaxiFareModel.zip";
        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(ModelSaver).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        public enum NodeSlotId
        {
            In,
            Out,
            ModelIn,
            NameIn
        }

        public override string Title => "Сохранить модель";

        public ModelSaver(XmlNode node_) : base(node_)
        {

        }

        public ModelSaver()
        {

        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);
            AddSlot((int)NodeSlotId.ModelIn, "Модель", SlotType.VarIn, typeof(ITransformer));
            AddSlot((int)NodeSlotId.NameIn, "Имя", SlotType.VarIn, typeof(string));
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
                //dynamic trainingDataView = InModel.trainingDataView;
                //mlContext.Model.Save(trainedModel, trainingDataView.Schema, ModelPath);

                LogManager.Instance.WriteLine(LogVerbosity.Info, $"Модель 001 сохранена.");
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
            return new ModelSaver();
        }
    }
}
