using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static NeoCortexApiSample.MultiSequenceLearning;

namespace NeoCortexApiSample
{
    class Program
    {
        static void Main(string[] args)
        {
            RunMultiSequenceSerializationExperiment();
        }

        /// <summary>
        /// In this experiment, the predictor of MultiSequenceLearning is serialized and deserialized. 
        /// The serialized predictor need to be tested, and validated.
        /// The prediction of both the normal predictor and the serialized predictor are checked, and found to be equal.
        /// The results indicate that the predictor is serialized and deserialized properly. 
        /// </summary>
        private static void RunMultiSequenceSerializationExperiment()
        {
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

            //sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0, 3.0, 4.0, 3.0, 4.0 }));
            //sequences.Add("S2", new List<double>(new double[] { 0.8, 2.0, 0.0, 3.0, 3.0, 4.0, 5.0, 6.0, 5.0, 7.0, 2.0, 7.0, 1.0, 9.0, 11.0, 11.0, 10.0, 13.0, 14.0, 11.0, 7.0, 6.0, 5.0, 7.0, 6.0, 5.0, 3.0, 2.0, 3.0, 4.0, 3.0, 4.0 }));

            sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0, }));
            sequences.Add("S2", new List<double>(new double[] { 8.0, 1.0, 2.0, 9.0, 10.0, 7.0, 11.00 }));

            // Prototype for building the prediction engine.
            MultiSequenceLearning experiment = new MultiSequenceLearning();

            //The serializedPredictor is the predictor after serialization and deserialization.
            Predictor serializedPredictor;
            //The predictor is the normal result of MultiSequenceLearning.
            //The Run() method will return not only the normal predictor, but also the serializedPredictor.
            var predictor = experiment.Run(sequences, out serializedPredictor, "predictor");

            // These list are used to make prediction. 
            // Predictor is traversing the list element by element
            // By providing more elements to the prediction, the predictor delivers more precise result.
            var list1 = new double[] { 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 };
            var list2 = new double[] { 2.0, 3.0, 4.0 };
            var list3 = new double[] { 8.0, 1.0, 2.0 };

            //The PredictNextElement() method will predict the next element using the normal predictor and the serialized predictor.
            //The prediction of both normal predictor and serialized predictor are checked, and compared. 
            predictor.Reset();
            Console.WriteLine("\n\n\t\tPrediction next elements with normal predictor: \n\n");
            //Prediction with normal predictor
            PredictNextElement(predictor, list2);

            serializedPredictor.Reset();
            Console.WriteLine("\n\n\t\tPrediction next elements with serialized predictor: \n\n");
            //Prediction with serialized predictor
            PredictNextElement(serializedPredictor, list2);

            //predictor.Reset();
            //PredictNextElement(predictor, list2);

            //predictor.Reset();
            //PredictNextElement(predictor, list3);
        }

        private static void PredictNextElement(Predictor predictor, double[] list)
        {
            Debug.WriteLine("------------------------------");

            foreach (var item in list)
            {
                var res = predictor.Predict(item);

                if (res.Count > 0)
                {
                    foreach (var pred in res)
                    {
                        Debug.WriteLine($"{pred.PredictedInput} - {pred.Similarity}");
                        Console.WriteLine($"{pred.PredictedInput} - {pred.Similarity}");
                    }

                    var tokens = res.First().PredictedInput.Split('_');
                    var tokens2 = res.First().PredictedInput.Split('-');
                    Debug.WriteLine($"Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}");
                    Console.WriteLine($"Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}\n");
                }
                else
                    Debug.WriteLine("Nothing predicted :(");
            }

            Debug.WriteLine("------------------------------");
        }
    }

}