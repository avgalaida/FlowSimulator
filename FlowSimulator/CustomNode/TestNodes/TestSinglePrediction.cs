using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;
using FlowSimulator.MLSamples.Regression.TaxiFarePrediction.DataStructures;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;


namespace FlowSimulator.CustomNode.TestNodes
{
    [Category("Тестовые"), Name("Однократный Прогноз")]
    public class TestSinglePrediction : ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out,
            ModelIn,
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
            AddSlot((int)NodeSlotId.Result, "Результат", SlotType.VarOut, typeof(String));
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
                TripTime = 1140,
                TripDistance = 3.75f,
                PaymentType = "CRD",
                FareAmount = 0 // To predict. Actual/Observed = 15.5
            };

            try
            {
                //
                dynamic trainedModel = GetValueFromSlot((int)NodeSlotId.ModelIn);
                // Create prediction engine related to the loaded trained model
                var predEngine = mlContext.Model.CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(trainedModel);
                //Score
                var resultprediction = predEngine.Predict(taxiTripSample);
                ///

                SetValueInSlot((int)NodeSlotId.Result, $"Прогноз: {resultprediction.FareAmount:0.####}, Фактическое значение: 15.5");

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
