using FlowGraphBase.Node.StandardVariableNode;
using System.Xml;
using Microsoft.ML;
using FlowGraphBase;
using FlowGraphBase.Node;

namespace FlowSimulator.CustomNode.TestNodes.Trainers
{
    [@Category("Алгоритмы Обучения/Регрессия"), Name("SDCA")]
    public class SDCA_Regression : GenericVariableNode<IEstimator<ITransformer>>
    {
        MLContext mlContext = new MLContext();

        public override string Title => "Стохастический Двухкоординатный Подъём";

        public SDCA_Regression()
        {
            Value = mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features");
        }

        public SDCA_Regression(XmlNode node) : base(node) { }

        protected override void InitializeSlots()
        {
            SlotFlag = SlotAvailableFlag.DefaultFlagVariable;
            ControlType = VariableControlType.ReadOnly;
            AddSlot(0, string.Empty, SlotType.VarInOut, typeof(IEstimator<ITransformer>), true, ControlType);
        }

        protected override SequenceNode CopyImpl()
        {
            SDCA_Regression node = new SDCA_Regression
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
            return mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features");
        }
    }
}
