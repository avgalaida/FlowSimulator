using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;

namespace FlowSimulator.CustomNode.TestNodes
{
    [Category("Тестовые"), Name("Тест1")]
    public class TestNode1 : ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out
        }

        public override string Title => "Тест_1";

        public TestNode1(XmlNode node_) : base(node_)
        {

        }

        public TestNode1()
        {

        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);

        }

        public override ProcessingInfo ActivateLogic(ProcessingContext context, NodeSlot slot)
        {
            ProcessingInfo info = new ProcessingInfo
            {
                State = LogicState.Ok
            };

            LogManager.Instance.WriteLine(LogVerbosity.Info, "Тест вывода.");

            ActivateOutputLink(context, (int)NodeSlotId.Out);

            return info;
        }

        protected override SequenceNode CopyImpl()
        {
            return new TestNode1();
        }
    }
}
