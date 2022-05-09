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
    [Category("Тестовые/Регрессия"), Name("Однократный Прогноз")]
    public class TestSinglePrediction : ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out,
            ModelIn,
            TripTimeIn,
            TripDistanceIn,
            Result
        }

        public override string Title => "Однократный Прогноз";

        public TestSinglePrediction(XmlNode node_) : base(node_)
        {

        }

        public TestSinglePrediction()
        {

        }

        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);
            AddSlot((int)NodeSlotId.ModelIn, "Модель", SlotType.VarIn, typeof(ITransformer));
            AddSlot((int)NodeSlotId.TripTimeIn, "Время поездки", SlotType.VarIn, typeof(float));
            AddSlot((int)NodeSlotId.TripDistanceIn, "Расстояние", SlotType.VarIn, typeof(float));
            AddSlot((int)NodeSlotId.Result, "Результат", SlotType.VarOut, typeof(float));

            //SetValueInSlot((int)NodeSlotId.TripTimeIn, (float)1000);
            //SetValueInSlot((int)NodeSlotId.TripDistanceIn, (float)4);

        }

        public override ProcessingInfo ActivateLogic(ProcessingContext context, NodeSlot slot)
        {
            ProcessingInfo info = new ProcessingInfo
            {
                State = LogicState.Ok
            };


            //Create ML Context with seed for repeatable/deterministic results
            MLContext mlContext = new MLContext();

            var taxiTripSample = new TaxiTrip()
            {
                VendorId = "VTS",
                RateCode = "1",
                PassengerCount = 1,
                //TripTime = ((float)GetValueFromSlot((int)NodeSlotId.TripTimeIn)),
                //TripDistance = ((float)GetValueFromSlot((int)NodeSlotId.TripDistanceIn)),
                TripTime = (float)GetValueFromSlot((int)NodeSlotId.TripTimeIn),
                TripDistance = (float)GetValueFromSlot((int)NodeSlotId.TripDistanceIn),
                PaymentType = "CRD",
                FareAmount = 0
            };

            try
            {
                dynamic trainedModel = GetValueFromSlot((int)NodeSlotId.ModelIn);
                var predEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel);
                var resultprediction = predEngine.Predict(taxiTripSample);

                SetValueInSlot((int)NodeSlotId.Result, resultprediction.FareAmount);

                ActivateOutputLink(context, (int)NodeSlotId.Out);
            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteLine(LogVerbosity.Error, "Недопустимое значение модели для узла Однократный Прогноз.");
            }

            return info;
        }

        protected override SequenceNode CopyImpl()
        {
            return new TestSinglePrediction();
        }
    }
}
