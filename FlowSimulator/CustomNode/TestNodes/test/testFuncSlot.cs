﻿using System.Xml;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;
using System;
using Microsoft.ML;
using System.IO;
using FlowGraphBase;

namespace FlowSimulator.CustomNode.TestNodes.test
{
    [@Category("Тестовые"), Name("ТестПриём")]
    public class testFuncSlot: ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out,
            TestIn
        }

        public override string Title => "ТестПриём";

        public testFuncSlot(XmlNode node_) : base(node_)
        {

        }

        public testFuncSlot()
        {

        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);
            AddSlot((int)NodeSlotId.TestIn, "Вход", SlotType.VarIn, typeof(string));
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
                dynamic str = GetValueFromSlot((int)NodeSlotId.TestIn);
                LogManager.Instance.WriteLine(LogVerbosity.Info, "str=" + str);

            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteLine(LogVerbosity.Error, "Недопустимое значение входных данных.");
            }

            return info;
        }

        protected override SequenceNode CopyImpl()
        {
            return new testFuncSlot();
        }
    }
}
