using FlowGraphBase.Node.StandardVariableNode;
using System.Xml;
using Microsoft.ML;
using FlowGraphBase;
using FlowGraphBase.Node;

namespace FlowSimulator.CustomNode.TestNodes.Trainers
{
    [@Category("Алгоритмы Обучения/Регрессия"), Name("OGD")]
    public class OGD_Regression : GenericVariableNode<IEstimator<ITransformer>>
    {
        MLContext mlContext = new MLContext();

        public override string Title => "Градиентный спуск";

        public OGD_Regression()
        {
            Value = mlContext.Regression.Trainers.OnlineGradientDescent(labelColumnName: "Label", featureColumnName: "Features");
        }

        public OGD_Regression(XmlNode node) : base(node) { }

        protected override void InitializeSlots()
        {
            SlotFlag = SlotAvailableFlag.DefaultFlagVariable;
            ControlType = VariableControlType.ReadOnly;
            AddSlot(0, string.Empty, SlotType.VarInOut, typeof(IEstimator<ITransformer>), true, ControlType);
        }

        protected override SequenceNode CopyImpl()
        {
            OGD_Regression node = new OGD_Regression
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
            return mlContext.Regression.Trainers.OnlineGradientDescent(labelColumnName: "Label", featureColumnName: "Features");
        }
    }
}
