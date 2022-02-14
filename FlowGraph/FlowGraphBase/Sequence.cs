using System;
using System.Linq;
using System.Xml;
using FlowGraphBase.Node;

namespace FlowGraphBase
{
    public enum SequenceState
    {
        Running,
        Pause,
        Stop
    }

    public class Sequence : SequenceBase
    {
        public const string XmlAttributeTypeValue = "Sequence";

        public Sequence(string name)
            : base(name)
        { }

        public Sequence(XmlNode node)
            : base(node)
        { }

        public bool ContainsEventNodeWithType(Type type)
        {
            return SequenceNodes.Any(pair => pair.Value is EventNode && pair.Value.GetType() == type);
        }

        public override void Save(XmlNode node)
        {
            base.Save(node);

            XmlNode graphNode = node.SelectSingleNode("Graph[@id='" + Id + "']");
            graphNode.AddAttribute("type", XmlAttributeTypeValue);
        }
    }
}
