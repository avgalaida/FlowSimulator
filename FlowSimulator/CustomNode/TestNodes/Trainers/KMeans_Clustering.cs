using FlowGraphBase.Node.StandardVariableNode;
using System.Xml;
using Microsoft.ML;
using FlowGraphBase;
using FlowGraphBase.Node;

namespace FlowSimulator.CustomNode.TestNodes.Trainers
{
    [@Category("Алгоритмы Обучения/Кластеризация"), Name("KMeans")]
    public class KMeans_Clustering: GenericVariableNode<IEstimator<ITransformer>>
    {
        MLContext mlContext = new MLContext();

        public override string Title => "Метод k-средних";

        public KMeans_Clustering()
        {
            Value = mlContext.Clustering.Trainers.KMeans(featureColumnName: "Features", numberOfClusters: 3);
        }

        public KMeans_Clustering(XmlNode node) : base(node) { }

        protected override void InitializeSlots()
        {
            SlotFlag = SlotAvailableFlag.DefaultFlagVariable;
            AddSlot(0, string.Empty, SlotType.VarInOut, typeof(IEstimator<ITransformer>), true, VariableControlType.ReadOnly);
        }

        protected override SequenceNode CopyImpl()
        {
            KMeans_Clustering node = new KMeans_Clustering
            {
                Value = Value
            };
            return node;
        }

        protected override void SaveValue(XmlNode node)
        {
            node.InnerText = "";

        }

        protected override object LoadValue(XmlNode node)
        {
            return mlContext.Clustering.Trainers.KMeans(featureColumnName: "Features", numberOfClusters: 3);
        }
    }
}
