using System.Xml;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Process;
using FlowSimulator.MLSamples.Regression.TaxiFarePrediction.DataStructures;
using System;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace FlowSimulator.CustomNode.TestNodes.Regression
{
    [Category("Тестовые/Регрессия"), Name("Model Builder")]
    public class ModelBuilder : ActionNode
    {
        public enum NodeSlotId
        {
            In,
            Out,
            TrainerIn,
            TrainingDataIn,
            Result
        }

        public override string Title => "Model Builder";

        public ModelBuilder(XmlNode node_) : base(node_)
        {

        }

        public ModelBuilder()
        {

        }


        protected override void InitializeSlots()
        {
            base.InitializeSlots();

            AddSlot((int)NodeSlotId.In, "", SlotType.NodeIn);
            AddSlot((int)NodeSlotId.Out, "", SlotType.NodeOut);
            AddSlot((int)NodeSlotId.TrainingDataIn, "Training Data", SlotType.VarIn, typeof(IDataView));
            AddSlot((int)NodeSlotId.TrainerIn, "Trainer algorithm", SlotType.VarIn, typeof(TrainerEstimatorBase<,>));
            AddSlot((int)NodeSlotId.Result, "Модель", SlotType.VarOut, typeof(ITransformer));
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
                LogManager.Instance.WriteLine(LogVerbosity.Info, $"Началось обучение модели...");
                var dataProcessPipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(TaxiTrip.FareAmount))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "VendorIdEncoded", inputColumnName: nameof(TaxiTrip.VendorId)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "RateCodeEncoded", inputColumnName: nameof(TaxiTrip.RateCode)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "PaymentTypeEncoded", inputColumnName: nameof(TaxiTrip.PaymentType)))
                .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(TaxiTrip.PassengerCount)))
                .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(TaxiTrip.TripTime)))
                .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(TaxiTrip.TripDistance)))
                .Append(mlContext.Transforms.Concatenate("Features", "VendorIdEncoded", "RateCodeEncoded", "PaymentTypeEncoded", nameof(TaxiTrip.PassengerCount)
                , nameof(TaxiTrip.TripTime), nameof(TaxiTrip.TripDistance)));

                dynamic trainer = GetValueFromSlot((int)NodeSlotId.TrainerIn);
                var trainingPipeline = dataProcessPipeline.Append(trainer);

                dynamic trainingDataView = GetValueFromSlot((int)NodeSlotId.TrainingDataIn);

                var trainedModel = trainingPipeline.Fit(trainingDataView);

                LogManager.Instance.WriteLine(LogVerbosity.Info, $"Обучение модели закончено");

                SetValueInSlot((int)NodeSlotId.Result, trainedModel);

                //Model model;
                //model.trainingDataView = trainingDataView;
                //model.trainedModel = trainedModel;
                //SetValueInSlot((int)NodeSlotId.Result, model);

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
            return new ModelBuilder();
        }
    }
}
