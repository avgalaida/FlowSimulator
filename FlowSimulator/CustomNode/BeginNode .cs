using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Node;

namespace FlowSimulator.CustomNode
{
    [Category("Событие"), Name("Начало")]
    public class BeginNode : EventNode
    {
        public override string Title => "Начальный узел";

        public BeginNode(XmlNode node)
            : base(node)
        {

        }

        public BeginNode()
        {

        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot(0, "Начало", SlotType.NodeOut);
        }

        protected override void TriggeredImpl(object para)
        {
            SetValueInSlot(0, para);
        }

        protected override SequenceNode CopyImpl()
        {
            return new BeginNode();
        }
    }
}
