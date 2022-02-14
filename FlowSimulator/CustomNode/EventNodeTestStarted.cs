﻿using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Node;

namespace FlowSimulator.CustomNode
{
    [Category("Event"), Name("Test Started")]
    public class EventNodeTestStarted : EventNode
    {
        public override string Title => "Test Started Event";

        public EventNodeTestStarted(XmlNode node)
            : base(node)
        {

        }

        public EventNodeTestStarted()
        {

        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot(0, "Started", SlotType.NodeOut);
            AddSlot(1, "Task name", SlotType.VarOut, typeof(string));
        }

        protected override void TriggeredImpl(object para)
        {
            SetValueInSlot(1, para);
        }

        /*protected override void Load(XmlNode node_)
        {
            base.Load();
        }*/

        protected override SequenceNode CopyImpl()
        {
            return new EventNodeTestStarted();
        }
    }
}