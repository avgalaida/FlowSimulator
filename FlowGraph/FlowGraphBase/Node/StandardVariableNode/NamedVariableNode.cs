using System;
using System.ComponentModel;
using System.Xml;

namespace FlowGraphBase.Node.StandardVariableNode
{
    [Visible(false)]
    public class NamedVariableNode : VariableNode
    {
        NamedVariable _value;

        public override string Title => _value.Name;

        public string VariableName => _value.Name;

        public Type VariableType => _value.VariableType;

        public override object Value
        {
            get => _value.Value;
            set => _value.InternalValueContainer.Value = value;
        }

        public NamedVariableNode(XmlNode node)
            : base(node)
        {
            InitializeSlots();
        }

        public NamedVariableNode(string name)
        {
            _value = NamedVariableManager.Instance.GetNamedVariable(name);
            _value.PropertyChanged += OnNamedVariablePropertyChanged;
            AddSlot(0, string.Empty, SlotType.VarInOut, _value.VariableType);
        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            if (_value != null) // call only when loaded with xml
            {
                AddSlot(0, string.Empty, SlotType.VarInOut, _value.VariableType);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override SequenceNode CopyImpl()
        {
            return new NamedVariableNode(_value.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNamedVariablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node_"></param>
        /// <returns></returns>
        protected override object LoadValue(XmlNode node)
        {
            return NamedVariableManager.Instance.GetNamedVariable(node.Attributes["varName"].Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node_"></param>
        protected override void SaveValue(XmlNode node)
        {
            node.AddAttribute("varName", _value.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node_"></param>
        protected override void Load(XmlNode node)
        {
            base.Load(node);
            _value = (NamedVariable)LoadValue(node.SelectSingleNode("Value"));
        }
    }
}
