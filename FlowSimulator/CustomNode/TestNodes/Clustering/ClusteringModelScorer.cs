using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using FlowSimulator.MLSamples.Clustering.CustomerSegmentation.DataStructures;

namespace FlowSimulator.CustomNode.TestNodes.Clustering
{
    public class ClusteringModelScorer
    {
        private readonly string _plotLocation;
        private readonly string _csvlocation;
        private readonly MLContext _mlContext;
        private ITransformer _trainedModel;
        private IDataView _csvData;

        public ClusteringModelScorer(MLContext mlContext, IDataView data, string plotLocation, string csvlocation)
        {
            _csvData = data;
            _plotLocation = plotLocation;
            _csvlocation = csvlocation;
            _mlContext = mlContext;
        }

        public ITransformer LoadModel(ITransformer model)
        {
            _trainedModel = model;
            return _trainedModel;
        }

        public void CreateCustomerClusters()
        {
            //Apply data transformation to create predictions/clustering
            var tranfomedDataView = _trainedModel.Transform(_csvData);
            var predictions = _mlContext.Data.CreateEnumerable<ClusteringPrediction>(tranfomedDataView, false)
                            .ToArray();

            //Generate data files with customer data grouped by clusters
            SaveCustomerSegmentationCSV(predictions, _csvlocation);

            //Plot/paint the clusters in a chart and open it with the by-default image-tool in Windows
            SaveCustomerSegmentationPlotChart(predictions, _plotLocation);
            OpenChartInDefaultWindow(_plotLocation);
        }

        private static void SaveCustomerSegmentationCSV(IEnumerable<ClusteringPrediction> predictions, string csvlocation)
        {
            using (var w = new System.IO.StreamWriter(csvlocation))
            {
                w.WriteLine($"LastName,SelectedClusterId");
                w.Flush();
                predictions.ToList().ForEach(prediction => {
                    w.WriteLine($"{prediction.LastName},{prediction.SelectedClusterId}");
                    w.Flush();
                });
            }

            Console.WriteLine($"CSV location: {csvlocation}");
        }

        private static void SaveCustomerSegmentationPlotChart(IEnumerable<ClusteringPrediction> predictions, string plotLocation)
        {

            var plot = new PlotModel { Title = "Customer Segmentation", IsLegendVisible = true };

            var clusters = predictions.Select(p => p.SelectedClusterId).Distinct().OrderBy(x => x);

            foreach (var cluster in clusters)
            {
                var scatter = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerStrokeThickness = 2, Title = $"Cluster: {cluster}", RenderInLegend = true };
                var series = predictions
                    .Where(p => p.SelectedClusterId == cluster)
                    .Select(p => new ScatterPoint(p.Location[0], p.Location[1])).ToArray();
                scatter.Points.AddRange(series);
                plot.Series.Add(scatter);
            }

            plot.DefaultColors = OxyPalettes.HueDistinct(plot.Series.Count).Colors;

            var exporter = new SvgExporter { Width = 600, Height = 400 };
            using (var fs = new System.IO.FileStream(plotLocation, System.IO.FileMode.Create))
            {
                exporter.Export(plot, fs);
            }

            Console.WriteLine($"Plot location: {plotLocation}");
        }

        private static void OpenChartInDefaultWindow(string plotLocation)
        {
            Console.WriteLine("Showing chart...");
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(plotLocation)
            {
                UseShellExecute = true
            };
            p.Start();
        }
    }
}
