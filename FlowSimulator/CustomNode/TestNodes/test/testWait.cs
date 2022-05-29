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
    [@Category("Тестовые/функ"), Name("ТестЖдун")]
    public class testWait: ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out,
            TestOut
        }

        public override string Title => "ТестЖдун";

        public testWait(XmlNode node_) : base(node_)
        {

        }

        public testWait()
        {

        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);
            AddSlot((int)NodeSlotId.TestOut, "Выход", SlotType.VarOut, typeof(string));
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
                LogManager.Instance.WriteLine(LogVerbosity.Info, "Ждём");
                for (double i = 0; i < 9999999999; i++)
                {
                    if (i == 9999999998)
                    {
                        LogManager.Instance.WriteLine(LogVerbosity.Info, "подождали");
                    }
                }
                SetValueInSlot((int)NodeSlotId.TestOut, "Щищ");

            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteLine(LogVerbosity.Error, "Недопустимое значение входных данных.");
            }

            return info;
        }

        protected override SequenceNode CopyImpl()
        {
            return new testWait();
        }
    }
}
