using System.Xml;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;
using System;
using Microsoft.ML;
using System.IO;
using FlowGraphBase;

namespace FlowSimulator.CustomNode.TestNodes.test
{
    [@Category("Тестовые/функ"), Name("ф1 function")]
    public class t1 : ActionNode
    {

        public enum NodeSlotId
        {
            Out,
            DataIn
        }

        public override string Title => "ф1 function";

        public t1(XmlNode node_) : base(node_)
        {

        }

        public t1()
        {

        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);
            AddSlot((int)NodeSlotId.DataIn, "Вход", SlotType.VarOut, typeof(int));
        }

        public override ProcessingInfo ActivateLogic(ProcessingContext context, NodeSlot slot)
        {
            ProcessingInfo info = new ProcessingInfo
            {
                State = LogicState.Ok
            };

            MLContext mlContext = new MLContext();

            try
            {
                ActivateOutputLink(context, (int)NodeSlotId.Out);
            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteLine(LogVerbosity.Error, "Недопустимое значение входных данных.");
            }

            return info;
        }

        protected override SequenceNode CopyImpl()
        {
            return new t1();
        }
    }
}
