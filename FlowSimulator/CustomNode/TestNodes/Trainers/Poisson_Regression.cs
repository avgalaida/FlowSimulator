using FlowGraphBase.Node.StandardVariableNode;
using System.Xml;
using Microsoft.ML;
using FlowGraphBase;
using FlowGraphBase.Node;

namespace FlowSimulator.CustomNode.TestNodes.Trainers
{
    [@Category("Алгоритмы Обучения/Регрессия"), Name("Poisson Regression")]
    public class PoissonRegression : GenericVariableNode<IEstimator<ITransformer>>
    {
        MLContext mlContext = new MLContext();

        public override string Title => "Регрессия Пуассона";

        public PoissonRegression()
        {
            Value = mlContext.Regression.Trainers.LbfgsPoissonRegression(labelColumnName: "Label", featureColumnName: "Features");
        }

        public PoissonRegression(XmlNode node) : base(node) { }

        protected override void InitializeSlots()
        {
            SlotFlag = SlotAvailableFlag.DefaultFlagVariable;
            ControlType = VariableControlType.ReadOnly;
            AddSlot(0, string.Empty, SlotType.VarInOut, typeof(IEstimator<ITransformer>), true, ControlType);
        }

        protected override SequenceNode CopyImpl()
        {
            PoissonRegression node = new PoissonRegression
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
            return mlContext.Regression.Trainers.LbfgsPoissonRegression(labelColumnName: "Label", featureColumnName: "Features");
        }
    }
}
