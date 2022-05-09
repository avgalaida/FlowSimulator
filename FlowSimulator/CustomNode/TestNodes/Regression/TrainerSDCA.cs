using FlowGraphBase.Node.StandardVariableNode;
using System.Xml;
using Microsoft.ML;
using System.IO;
using FlowGraphBase;
using FlowGraphBase.Node;
using FlowSimulator.MLSamples.Regression.TaxiFarePrediction.DataStructures;
using Microsoft.ML.Trainers;

namespace FlowSimulator.CustomNode.TestNodes.Regression
{
    [@Category("Тестовые/Регрессия"), Name("Trainer SDCA Regression")]
    public class TrainerSDCA : GenericVariableNode<object>
    {
        MLContext mlContext = new MLContext();

        public override string Title => "SDCA Regression";

        public TrainerSDCA()
        {
            Value = mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features");
        }

        public TrainerSDCA(XmlNode node) : base(node) { }

        protected override void InitializeSlots()
        {
            SlotFlag = SlotAvailableFlag.DefaultFlagVariable;
            ControlType = VariableControlType.ReadOnly;
            AddSlot(0, string.Empty, SlotType.VarInOut, typeof(TrainerEstimatorBase<,>), true, ControlType);
        }

        protected override SequenceNode CopyImpl()
        {
            TrainerSDCA node = new TrainerSDCA
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
