using System.Xml;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;
using System;
using Microsoft.ML;
using System.IO;
using FlowGraphBase;

namespace FlowSimulator.CustomNode.TestNodes.Clustering
{
    [@Category("Тестовые/Кластеризация"), Name("Построение кластеров")]
    public class BuildClusters: ActionNode
    {
        private static string assetsRelativePath = @"../../MLSamples/Clustering/CustomerSegmentation/assets";
        private static string assetsPath = GetAbsolutePath(assetsRelativePath);
        private static string plotSvg = Path.Combine(assetsPath, "outputs", "customerSegmentation.svg");
        private static string plotCsv = Path.Combine(assetsPath, "outputs", "customerSegmentation.csv");

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(BuildClusters).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;
            string fullPath = Path.Combine(assemblyFolderPath, relativePath);
            return fullPath;
        }

        public enum NodeSlotId
        {
            In,
            Out,
            ModelIn,
            DataIn
        }

        public override string Title => "Построение кластеров";

        public BuildClusters(XmlNode node_) : base(node_)
        {

        }

        public BuildClusters()
        {

        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);
            AddSlot((int)NodeSlotId.DataIn, "Данные", SlotType.VarIn, typeof(IDataView));
            AddSlot((int)NodeSlotId.ModelIn, "Модель", SlotType.VarIn, typeof(ITransformer));
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
                LogManager.Instance.WriteLine(LogVerbosity.Info, $"Построение кластеров...");
                dynamic pivotCsv = GetValueFromSlot((int)NodeSlotId.DataIn);
                var clusteringModelScorer = new ClusteringModelScorer(mlContext, pivotCsv, plotSvg, plotCsv);
                dynamic model = GetValueFromSlot((int)NodeSlotId.ModelIn);
                clusteringModelScorer.LoadModel(model);
                clusteringModelScorer.CreateCustomerClusters();

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
            return new BuildClusters();
        }
    }
}
