using FlowGraphBase.Node.StandardVariableNode;
using System.Xml;
using Microsoft.ML;
using System.IO;
using FlowGraphBase;
using FlowGraphBase.Node;
using Microsoft.ML.Data;
using FlowSimulator.MLSamples.Clustering.CustomerSegmentation.DataStructures;

namespace FlowSimulator.CustomNode.TestNodes.Clustering
{
    [Category("Тестовые/Кластеризация"), Name("PivotDataView")]
    public class PivotDataView : GenericVariableNode<IDataView>
    {
        private static string assetsRelativePath = @"../../MLSamples/Clustering/CustomerSegmentation/assets";
        private static string assetsPath = GetAbsolutePath(assetsRelativePath);

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(PivotDataView).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        string pivotCsv = Path.Combine(assetsPath, "inputs", "pivot.csv");

        MLContext mlContext = new MLContext();

        public override string Title => "PivotDataView";

        public PivotDataView()
        {
            Value = mlContext.Data.LoadFromTextFile(path: pivotCsv,
                                            columns: new[]
                                                        {
                                                        new TextLoader.Column("Features", DataKind.Single, new[] {new TextLoader.Range(0, 31) }),
                                                        new TextLoader.Column(nameof(PivotData.LastName), DataKind.String, 32)
                                                        },
                                            hasHeader: true,
                                            separatorChar: ',');
        }

        public PivotDataView(XmlNode node) : base(node) { }

        protected override void InitializeSlots()
        {
            SlotFlag = SlotAvailableFlag.DefaultFlagVariable;
            ControlType = VariableControlType.ReadOnly;
            AddSlot(0, string.Empty, SlotType.VarInOut, typeof(IDataView), true, ControlType);
        }

        protected override SequenceNode CopyImpl()
        {
            PivotDataView node = new PivotDataView
            {
                Value = Value
            };
            return node;
        }

        protected override void SaveValue(XmlNode node)
        {
            node.InnerText = pivotCsv;

        }

        protected override object LoadValue(XmlNode node)
        {
            return mlContext.Data.LoadFromTextFile(path: node.InnerText,
                                            columns: new[]
                                                        {
                                                        new TextLoader.Column("Features", DataKind.Single, new[] {new TextLoader.Range(0, 31) }),
                                                        new TextLoader.Column(nameof(PivotData.LastName), DataKind.String, 32)
                                                        },
                                            hasHeader: true,
                                            separatorChar: ',');
        }
    }
}
