using FlowGraphBase.Node.StandardVariableNode;
using System.Xml;
using Microsoft.ML;
using System.IO;
using FlowGraphBase;
using FlowGraphBase.Node;

namespace FlowSimulator.CustomNode.TestNodes.Regression
{
    [@Category("Тестовые/Регрессия"), Name("Модель")]
    public class TestModelNode : GenericVariableNode<ITransformer>
    {
        private static string BaseModelsRelativePath = @"../../MLSamples/MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/TaxiFareModel.zip";
        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(TestModelNode).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        MLContext mlContext = new MLContext();

        public override string Title => "Модель 001";

        public TestModelNode()
        {
            Value = mlContext.Model.Load(ModelPath, out var modelInputSchema); 
        }

        public TestModelNode(XmlNode node) : base(node) { }

        protected override void InitializeSlots()
        {
            SlotFlag = SlotAvailableFlag.DefaultFlagVariable;
            ControlType = VariableControlType.ReadOnly;
            AddSlot(0, string.Empty, SlotType.VarInOut, typeof(ITransformer), true, ControlType);
        }

        protected override SequenceNode CopyImpl()
        {
            TestModelNode node = new TestModelNode
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
