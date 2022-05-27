using System;
using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;

namespace FlowSimulator.CustomNode.TestNodes
{
    [Category("Визуализация"), Name("Тест1")]
    public class TestNode1 : ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out,
            VarMinIn,
            VarMaxIn,
            VarResultOut
        }

        public override string Title => "test";

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

            AddSlot((int)NodeSlotId.VarMinIn, "Мин.", SlotType.VarIn, typeof(int));
            AddSlot((int)NodeSlotId.VarMaxIn, "Макс.", SlotType.VarIn, typeof(int));
            AddSlot((int)NodeSlotId.VarResultOut, "Результат", SlotType.VarOut, typeof(int));
        }

        public override ProcessingInfo ActivateLogic(ProcessingContext context, NodeSlot slot)
        {
            ProcessingInfo info = new ProcessingInfo
            {
                State = LogicState.Ok
            };

            int min = (int)GetValueFromSlot((int)NodeSlotId.VarMinIn);
            int max = (int)GetValueFromSlot((int)NodeSlotId.VarMaxIn);
            Random Random = new Random();
            int result = min + (int)(Random.NextDouble() * (max - min));

            SetValueInSlot((int)NodeSlotId.VarResultOut, result);
            ActivateOutputLink(context, (int)NodeSlotId.Out);

            return info;
        }

        protected override SequenceNode CopyImpl()
        {
            return new TestNode1();
        }
    }
}
