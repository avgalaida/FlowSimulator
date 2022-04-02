using System;
using System.Xml;
using FlowGraphBase.Logger;
using FlowGraphBase.Process;

namespace FlowGraphBase.Node.StandardActionNode
{
    [Category("Time"), Name("Delay")]
    public class DelayNode
        : ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out,
            Delay
        }

        //TimeSpan _StartTime = TimeSpan.Zero;

        public override string Title => "Delay";

        public DelayNode(XmlNode node)
            : base(node)
        {

        }

        public DelayNode()
        {

        }
        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "Completed", SlotType.NodeOut);

            AddSlot((int)NodeSlotId.Delay, "Duration (ms)", SlotType.VarIn, typeof(int));
        }

        public override ProcessingInfo ActivateLogic(ProcessingContext context, NodeSlot slot)
        {
            ProcessingInfo info = new ProcessingInfo
            {
                State = LogicState.Ok
            };

            object intVal = GetValueFromSlot((int)NodeSlotId.Delay);

            if (intVal == null)
            {
                info.State = LogicState.Ok;
                info.ErrorMessage = "Please connect a integer variable node";

                LogManager.Instance.WriteLine(LogVerbosity.Error,
                    "{0} : {1}.",
                    Title, info.ErrorMessage);

                return info;
            }

            MemoryStackItem memoryItem = context.CurrentFrame.GetValueFromId(Id);

            if (memoryItem == null)
            {
                memoryItem = context.CurrentFrame.Allocate(Id, TimeSpan.Zero);
            }

            TimeSpan startTime = (TimeSpan)memoryItem.Value;

            int delay = (int)intVal;
            double delayDouble = delay / 1000.0;
            TimeSpan t = DateTime.Now.TimeOfDay.Subtract(startTime);
            double totalSecs = t.TotalSeconds;

            //this is the first time, so we set the current time
            if (startTime == TimeSpan.Zero)
            {
                totalSecs = 0.0;
                startTime = DateTime.Now.TimeOfDay;
                memoryItem.Value = startTime;
            }
            else if (totalSecs >= delayDouble)
            {
                startTime = TimeSpan.Zero;
                memoryItem.Value = startTime;
                ActivateOutputLink(context, (int)NodeSlotId.Out);
                CustomText = String.Empty;
                return info;
            }

            CustomText = $"Delay ({delayDouble - totalSecs:0.000} seconds left)";

            context.RegisterNextExecution(GetSlotById((int)NodeSlotId.In));
            return info;
        }
        protected override SequenceNode CopyImpl()
        {
            return new DelayNode();
        }
    }
}
