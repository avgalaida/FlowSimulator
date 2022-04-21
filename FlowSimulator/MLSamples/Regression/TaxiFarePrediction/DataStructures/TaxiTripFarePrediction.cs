using Microsoft.ML.Data;

namespace FlowSimulator.MLSamples.Regression.TaxiFarePrediction.DataStructures
{
    internal class TaxiTripFarePrediction
    {
        [ColumnName("Score")]
        public float FareAmount;
    }
}
