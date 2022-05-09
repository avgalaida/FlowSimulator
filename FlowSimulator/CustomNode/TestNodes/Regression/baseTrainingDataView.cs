using FlowGraphBase.Node.StandardVariableNode;
using System.Xml;
using Microsoft.ML;
using System.IO;
using FlowGraphBase;
using FlowGraphBase.Node;
using FlowSimulator.MLSamples.Regression.TaxiFarePrediction.DataStructures;

namespace FlowSimulator.CustomNode.TestNodes.Regression
{
    [@Category("Тестовые/Регрессия"), Name("baseTrainingDataView")]
    public class baseTrainingDataView : GenericVariableNode<IDataView>
    {
        private static string BaseModelsRelativePath = @"../../MLSamples/Data";
        private static string TrainDataRelativePath = $"{BaseModelsRelativePath}/taxi-fare-test.csv";
        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(baseTrainingDataView).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        MLContext mlContext = new MLContext();

        public override string Title => "baseTrainingDataView";

        public baseTrainingDataView()
        {
            Value = mlContext.Data.LoadFromTextFile<TaxiTrip>(TrainDataPath, hasHeader: true, separatorChar: ',');
        }

        public baseTrainingDataView(XmlNode node) : base(node) { }

        protected override void InitializeSlots()
        {
            SlotFlag = SlotAvailableFlag.DefaultFlagVariable;
            ControlType = VariableControlType.ReadOnly;
            AddSlot(0, string.Empty, SlotType.VarInOut, typeof(IDataView), true, ControlType);
        }

        protected override SequenceNode CopyImpl()
        {
            baseTrainingDataView node = new baseTrainingDataView
            {
                Value = Value
            };
            return node;
        }

        protected override void SaveValue(XmlNode node)
        {
            node.InnerText = TrainDataPath;

        }

        protected override object LoadValue(XmlNode node)
        {
            return mlContext.Data.LoadFromTextFile<TaxiTrip>(node.InnerText, hasHeader: true, separatorChar: ',');
        }
    }
}
