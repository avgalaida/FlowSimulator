using FlowGraphBase.Node.StandardVariableNode;
using System.Xml;
using Microsoft.ML;
using System.IO;
using FlowGraphBase;
using FlowGraphBase.Node;
using FlowSimulator.MLSamples.Regression.TaxiFarePrediction.DataStructures;

namespace FlowSimulator.CustomNode.TestNodes.Regression
{
    [@Category("Тестовые/Регрессия"), Name("testDataView")]
    public class testDataView : GenericVariableNode<IDataView>
    {
        private static string BaseModelsRelativePath = @"../../MLSamples/Data";
        private static string TestDataRelativePath = $"{BaseModelsRelativePath}/taxi-fare-test.csv";
        private static string TestDataPath = GetAbsolutePath(TestDataRelativePath);

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(testDataView).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        MLContext mlContext = new MLContext();

        public override string Title => "testDataView";

        public testDataView()
        {
            Value = mlContext.Data.LoadFromTextFile<TaxiTrip>(TestDataPath, hasHeader: true, separatorChar: ',');
        }

        public testDataView(XmlNode node) : base(node) { }

        protected override void InitializeSlots()
        {
            SlotFlag = SlotAvailableFlag.DefaultFlagVariable;
            ControlType = VariableControlType.ReadOnly;
            AddSlot(0, string.Empty, SlotType.VarInOut, typeof(IDataView), true, ControlType);
        }

        protected override SequenceNode CopyImpl()
        {
            testDataView node = new testDataView
            {
                Value = Value
            };
            return node;
        }

        protected override void SaveValue(XmlNode node)
        {
            node.InnerText = TestDataPath;

        }

        protected override object LoadValue(XmlNode node)
        {
            return mlContext.Data.LoadFromTextFile<TaxiTrip>(node.InnerText, hasHeader: true, separatorChar: ',');
        }
    }
}
