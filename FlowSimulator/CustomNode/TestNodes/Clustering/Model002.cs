using FlowGraphBase.Node.StandardVariableNode;
using System.Xml;
using Microsoft.ML;
using FlowGraphBase;
using FlowGraphBase.Node;
using System.IO;

namespace FlowSimulator.CustomNode.TestNodes.Clustering
{
    [@Category("Тестовые/Кластеризация"), Name("Модель0002")]
    public class Model002 : GenericVariableNode<ITransformer>
    {
        private static string BaseModelsRelativePath = @"../../MLSamples/MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/retailClustering.zip";
        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Model002).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        MLContext mlContext = new MLContext();

        public override string Title => "Модель 002";

        public Model002()
        {
            Value = mlContext.Model.Load(ModelPath, out var modelInputSchema);
        }

        public Model002(XmlNode node) : base(node) { }

        protected override void InitializeSlots()
        {
            SlotFlag = SlotAvailableFlag.DefaultFlagVariable;
            ControlType = VariableControlType.ReadOnly;
            AddSlot(0, string.Empty, SlotType.VarInOut, typeof(ITransformer), true, ControlType);
        }

        protected override SequenceNode CopyImpl()
        {
            Model002 node = new Model002
            {
                Value = Value
            };
            return node;
        }

        protected override void SaveValue(XmlNode node)
        {
            node.InnerText = ModelPath;

        }

        protected override object LoadValue(XmlNode node)
        {
            return mlContext.Model.Load(node.InnerText, out var modelInputSchema);
        }


    }
}
