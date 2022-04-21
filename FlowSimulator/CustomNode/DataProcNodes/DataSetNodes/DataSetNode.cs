using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;

namespace FlowSimulator.CustomNode.DataProcNodes.DataSetNodes
{
    [Category("Данные"), Name("Набор данных")]
    public class DataSetNode : VariableNode
    {
        public override object Value { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        protected override SequenceNode CopyImpl()
        {
            throw new System.NotImplementedException();
        }

        protected override object LoadValue(XmlNode node)
        {
            throw new System.NotImplementedException();
        }

        protected override void SaveValue(XmlNode node)
        {
            throw new System.NotImplementedException();
        }
    }
}
