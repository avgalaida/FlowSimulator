using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;
using FlowSimulator.MLSamples.Regression.TaxiFarePrediction.DataStructures;
using System;
using Microsoft.ML;

namespace FlowSimulator.CustomNode.TestNodes.Regression
{
    [Category("Тестовые/Регрессия"), Name("Тест Фильтр")]
    public class TestFilter: ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out,
            DataIn,
            Result
        }

        public override string Title => "Тест Фильтр";

        public TestFilter(XmlNode node_) : base(node_)
        {

        }

        public TestFilter()
        {

        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);
            AddSlot((int)NodeSlotId.DataIn, "Данные", SlotType.VarIn, typeof(IDataView));
            AddSlot((int)NodeSlotId.Result, "Результат", SlotType.VarOut, typeof(IDataView));
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
                LogManager.Instance.WriteLine(LogVerbosity.Info, $"Фильтрация данных");

                dynamic baseTrainingDataView = GetValueFromSlot((int)NodeSlotId.DataIn);
                IDataView trainingDataView = mlContext.Data.FilterRowsByColumn(baseTrainingDataView,
                    nameof(TaxiTrip.FareAmount), lowerBound: 1, upperBound: 150); ;
                SetValueInSlot((int)NodeSlotId.Result, trainingDataView);

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
            return new TestFilter();
        }
    }
}
