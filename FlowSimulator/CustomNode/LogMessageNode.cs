using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;

namespace FlowSimulator.CustomNode
{
    [Category("Журнал логирования"), Name("Вывод в лог")]
    public class LogMessageNode : ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out,
            Message
        }

        public override string Title => "Вывод в лог";

        public LogMessageNode(XmlNode node_): base(node_)
        {
        }

        public LogMessageNode()
        {  
        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);
            AddSlot((int)NodeSlotId.Message, "Текст", SlotType.VarIn, typeof(string));
        }

        public override ProcessingInfo ActivateLogic(ProcessingContext context_, NodeSlot slot_)
        {
            ProcessingInfo info = new ProcessingInfo
            {
                State = LogicState.Ok
            };
            object val = GetValueFromSlot((int)NodeSlotId.Message);

            if (val == null)
            {
                info.State = LogicState.Warning;
                info.ErrorMessage = "Соединяется только со строковым узлом";

                LogManager.Instance.WriteLine(LogVerbosity.Warning,
                    "{0} : Сообщение не отображается, поскольку узел переменной не подключен. {1}.",
                    Title, info.ErrorMessage);
            }
            else
            {
                LogManager.Instance.WriteLine(LogVerbosity.Info, val.ToString());
            }

            ActivateOutputLink(context_, (int)NodeSlotId.Out);

            return info;
        }

        protected override SequenceNode CopyImpl()
        {
            return new LogMessageNode();
        }
    }
}
