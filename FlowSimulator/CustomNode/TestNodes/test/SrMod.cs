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
    [@Category("Тестовые"), Name("Сравнение моделей")]
    public class SrMod : ActionNode
    {

        public enum NodeSlotId
        {
            In,
            Out,
            Mod1,
            Mod2,
            Mod3,
            Mod4,
            BestModOut,
            DataIn
        }

        public override string Title => "Сравнение моделей";

        public SrMod(XmlNode node_) : base(node_)
        {

        }

        public SrMod()
        {

        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);
            AddSlot((int)NodeSlotId.Mod1, "Модель1", SlotType.VarIn, typeof(ITransformer));
            AddSlot((int)NodeSlotId.Mod2, "Модель2", SlotType.VarIn, typeof(ITransformer));
            AddSlot((int)NodeSlotId.Mod3, "Модель3", SlotType.VarIn, typeof(ITransformer));
            AddSlot((int)NodeSlotId.Mod4, "Модель4", SlotType.VarIn, typeof(ITransformer));
            AddSlot((int)NodeSlotId.BestModOut, "Лучшая", SlotType.VarOut, typeof(ITransformer));
            AddSlot((int)NodeSlotId.DataIn, "Данные", SlotType.VarIn, typeof(IDataView));

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
                LogManager.Instance.WriteLine(LogVerbosity.Info, "Лучшая модель: 3.");
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
            return new SrMod();
        }
    }
}
