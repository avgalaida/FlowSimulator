using System.Xml;
using FlowGraphBase.Process;

namespace FlowGraphBase.Node
{
    public abstract
#if EDITOR
    partial
#endif
    class EventNode: SequenceNode
    {
        public EventNode(XmlNode node): base(node)
        {
        }

        protected override void InitializeSlots()
        {
            SlotFlag = SlotAvailableFlag.DefaultFlagEvent;
        }

        public void Triggered(ProcessingContext context, int index, object para)
        {
            TriggeredImpl(para);
            ActivateOutputLink(context, index);
        }

        protected abstract void TriggeredImpl(object para);
    }
}
