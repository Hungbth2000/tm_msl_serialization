# ML22/23-10 Implement Serialization of Temporal Memory Learning Example

## Table of contents
1. [Introduction](#introduction)
2. [Important Links](#important-links)
3. [Getting Started](#getting-started)
4. [Implementation](#implementation-of-serialization-and-deserialization-methods)
5. [Experiment](#experiment)
6. [Changed Files](#changed-files)

## Introduction
In this project, serialization and deserialization methods for Predictor class were implemented.

- Serialization method:<br/>
  	Serialize all the objects in Predictor class (Connections, CortexLayer, HtmClassifier), and save them to an output text file.
- Deserialization method:<br/> 
  	This method takes the previous serialized text file as an input, and recreate an instance of Predictor class with the previous data. All the objects in the new Predictor instance are created with previous data.

The predictor which is the result of MultiSequenceLearning is serialized and deserialized using the implemented methods for serialization within Predictor class. To verify the implementation of the project, both the normal predictor and the serialized predictor were used to make prediction. The prediction results of both predictors are checked, and compared,  which were found to be identical.

## Important links
1. SE Project Documentation: [PDF]()<br/>
2. Implemented Serialize method in Predictor class: [Predictor.Serialize()](https://github.com/Hungbth2000/tm_msl_serialization/blob/b3ee2bc2aa15ddd42bdabe0bf6c363002ade5869/source/NeoCortexApi/Predictor.cs#L74)<br/>
3. Implemented Deserialize method in Predictor class: [Predictor.Deserialize()](https://github.com/Hungbth2000/tm_msl_serialization/blob/c0c1947558579ba0aa55d3509345344a9e07d5eb/source/NeoCortexApi/Predictor.cs#L100)<br/>
4. Implemented Serialize method in CortexLayer class: [CortextLayer.Serialize()](https://github.com/Hungbth2000/tm_msl_serialization/blob/d0453cc22f39ff6af8d0631773823c5541f67204/source/NeoCortexApi/Network/CortexLayer.cs#L166)<br/>
5. Project Experiment: [Program.cs](https://github.com/Hungbth2000/tm_msl_serialization/blob/master/source/MySeProject/Program.cs)
, [MultisequenceLearning.cs](https://github.com/Hungbth2000/tm_msl_serialization/blob/master/source/MySeProject/MultisequenceLearning.cs)

## Getting Started
Go to the source folder, and from there run the command below through the command line. You should be able to see the output of the experiment.

```bash
    dotnet run --project "../source/MySeProject/MySeProject.csproj"
```
Path to the Project: [MySeProject](https://github.com/Hungbth2000/tm_msl_serialization/blob/master/source/MySeProject/Program.cs)

## Implementation of Serialization and Deserialization methods

### Serialize method for CortexLayer:
- First of all, it is necessary to implement serialize method for the CortexLayer which is an object within Predictor class, the CortexLayer in predictor has three HtmModules( ScalarEncoder, SpatialPoolerMT, TemporalMemory).
<br\>The method below shows how to serialize the cortexlayer:
```csharp
    public void Serialize(object obj, string name, StreamWriter sw)
    {
        // Serialize all the HtmModules in the CortexLayer
        if (obj is CortexLayer<object, object> layer)
        {
            HtmSerializer ser = new HtmSerializer();
            foreach (var modulePair in layer.HtmModules)
            {
                ISerializable serializableModule = modulePair.Value as ISerializable;
                string ObjType = serializableModule.GetType().Name;
                if (serializableModule != null)
                {
                    ser.SerializeBegin(ObjType, sw);

                    serializableModule.Serialize(serializableModule, null, sw);

                    ser.SerializeEnd(ObjType, sw);
                }
                else
                {
                    throw new NotImplementedException();
                }
             }
         }
    }
```

### Serialize method for Predictor class:
- In order to serialize the result of MultiSequences Learning, we must be able to serialize instance of class Predictor.
- The Predictor class inherits the interface ISerializable which defines methods for serialization.
```csharp
    public interface ISerializable
    {
        void Serialize(object obj, string name, StreamWriter sw);
        static object Deserialize<T>(StreamReader sr, string name) => throw new NotImplementedException();
    }
``` 
- Inside the Predictor we have three objects that are needed to be serialized. They are connections, layer, and HtmClassifier. The code below shows the objects in the predictor:
```
    public class Predictor : ISerializable
    {
        private Connections connections { get; set; }

        private CortexLayer<object, object> layer { get; set; }

        private HtmClassifier<string, ComputeCycle> classifier { get; set; }
```
- The Predictor.Serialize() method will serialize all the objects in the predictor. It will call the Connections.Serialize(), layer.Serialize(), and classifier.Serialize() methods to serialize the connections, cortex layer, and classifier in the Predictor instance respectively.

```csharp
    public void Serialize(object obj, string name, StreamWriter sw)
    {
        if (obj is Predictor predictor)
        {
            // Serialize the Connections in Predictor instance
            var connections = predictor.connections;
            connections.Serialize(connections, null, sw);

            // Serialize the CortexLayer in Predictor instance               
            var layer = predictor.layer;
            layer.Serialize(layer, null, sw);

            // Serialize the HtmClassifier object in Predictor instance             
            var classifier = predictor.classifier;
            classifier.Serialize(classifier, null, sw);
        }
    }
```
- In order to make the program cleaner and easier to use, Predictor.Save() method is implemented. The save method will take the name of the file where you want to save the Predictor instance to and the Predictor instance as the input arguments. When somebody invokes the Predictor.Save() method and provides a file name. The method will create a stream writer from the file name and call the Predictor.Serialize() method to serialize the Predictor instance. The code below shows how the Predictor.Save() method implemented:
```csharp
    public static void Save(object obj, string fileName)
    {
        if (obj is Predictor predictor)
        {
            HtmSerializer.Reset();
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                predictor.Serialize(obj, null, sw);
                //predictor.Serialize(sw);
            }
        }
    }
```

### De-serialize method for Predictor class: 
- Up to this point, the predictor instance which is the result of MultiSequenceLearning, was able to be serialized to an output text file. 
- Now the goal is to implement Deserialize() method for the Predictor class that can be used to recreate a Predictor instance from the serialized text file. All the objects in the recreated predictor must have the previous data. The code below shows the implemented Deserialize() within the Predictor class.
```csharp
    public static object Deserialize<T>(StreamReader sr, string name)
        {
            HtmSerializer ser = new HtmSerializer();
            // Initialize the Predictor
            Predictor predictor = new Predictor(null, null, null);
            // Initialize the CortexLayer
            CortexLayer<object, object> layer = new CortexLayer<object, object>("L1");

            // Add SP and TM objects to CortexLayer, initialize the values (null) 
            layer.HtmModules.Add("encoder", (ScalarEncoder)null);
            layer.HtmModules.Add("sp", (SpatialPoolerMT)null);
            layer.HtmModules.Add("tm", (TemporalMemory)null);

            while (sr.Peek() >= 0)
            {

                var data = sr.ReadLine();

                // Deserialize Connections object 
                if (data == ser.ReadBegin(nameof(Connections)) && (predictor.connections == null))
                {
                    var con = Connections.Deserialize<Connections>(sr, null);
                    if (con is Connections connections)
                    {
                        predictor.connections = connections;
                    }

                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                }

                // Deserialize the ScalarEncoder             
                if (data == ser.ReadBegin(nameof(ScalarEncoder)) && (layer.HtmModules["encoder"] == null))
                {
                    var en = EncoderBase.Deserialize<ScalarEncoder>(sr, null);
                    if (en is ScalarEncoder encoder)
                    {
                        layer.HtmModules["encoder"] = encoder;
                    }

                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                }

                // Deserialize Spatial Pooler object
                if (data == ser.ReadBegin(nameof(SpatialPoolerMT)) && (layer.HtmModules["sp"] == null))
                {
                    var sp = SpatialPooler.Deserialize<SpatialPoolerMT>(sr, null);
                    layer.HtmModules["sp"] = (SpatialPoolerMT)sp;

                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                }

                // Deserialize Temporal Memory object
                if (data == ser.ReadBegin(nameof(TemporalMemory)) && (layer.HtmModules["tm"] == null))
                {
                    var tm = TemporalMemory.Deserialize<TemporalMemory>(sr, null);
                    layer.HtmModules["tm"] = (TemporalMemory)tm;

                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                }

                // Deserialize the HtmClassifier object
                if (data == ser.ReadBegin(nameof(HtmClassifier<string, ComputeCycle>)) && (predictor.classifier == null))
                {
                    var cls = HtmClassifier<string, ComputeCycle>.Deserialize<HtmClassifier<string, ComputeCycle>>(sr, null);
                    predictor.classifier = (HtmClassifier<string, ComputeCycle>)cls;

                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                }
            }

            predictor.layer = layer;
            return predictor;
        }
```
- On the other hand, Predictor.Load() method is also implemented which is used to create a stream reader for the Predictor.Deserialize() method. The input argument for the Load() method is the name of the file to which the previous predictor was serialized.
```csharp
    public static T Load<T>(string fileName)
    {
        HtmSerializer.Reset();
        using StreamReader sr = new StreamReader(fileName);
        return (T)Deserialize<T>(sr, null);
    }
```

## Experiment

### Input for learning 
- S1 and S2 are the input sequences for MultiSequenceLearning. 
```csharp
    private static void RunMultiSequenceSerializationExperiment()
    {
        Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

        sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0, }));
        sequences.Add("S2", new List<double>(new double[] { 8.0, 1.0, 2.0, 9.0, 10.0, 7.0, 11.00 }));
``` 
- Run() method is called to learn the sequences, which will returns both the normal predictor and the serialized predictor.
```csharp
    //The serializedPredictor is the predictor after serialization and deserialization.
    Predictor serializedPredictor;
    //The predictor is the normal result of MultiSequenceLearning.
    //The Run() method will return not only the normal predictor, but also the serializedPredictor.
    var predictor = experiment.Run(sequences, out serializedPredictor, "predictor");
```
- The Run() method also returns the MultiSequenceLearning.RunExperiment() method which finally returns the instance of Predictor.
- For verifying the implementation of serialization methods, the RunExperiment method will return two instances of class Predictor i.e., "predictor" and "serializedPredictor". The "predictor" instance is the normal predictor which is the normal result of the learning. While, "serializedPredictor" is the instance of class Predictor which is the result after serialization and deserialization of "predictor".
```csharp
    public Predictor Run(Dictionary<string, List<double>> sequences, out Predictor serializedPredictor, string fileName)
    {
        .......
        
        return RunExperiment(inputBits, cfg, encoder, sequences, out serializedPredictor, fileName);
    }
    private Predictor RunExperiment(int inputBits, HtmConfig cfg, EncoderBase encoder, Dictionary<string, List<double>> sequences, out Predictor serializedPredictor, string fileName)
    {
        .......
        // The "predictor" is the instance of class Predictor which is result after learning. This "predictor" object later on put in the argument of Save() method for serialization.
        // The "serializedPredictor" is the instance of Predictor class which is the result after serialization and deserialization of Predictor. 
        var predictor = new Predictor(layer1, mem, cls);
        
        //Save() method is callled from Predictor Class, which serialize the instance of Predictor Class.
        Predictor.Save(predictor, fileName);

        //Load() method is callled from Predictor Class, which deserialize the instance of Predictor Class.
        serializedPredictor = Predictor.Load<Predictor>(fileName);
        return predictor;
    }
```

### Input for testing 
- After learning these two sequences, several sequences are defined to check how the prediction works. 
```csharp
    // These list are used to make prediction
    // Predictor is traversing the list element by element
    // By providing more elements to the prediction, the predictor delivers more precise result
    var list1 = new double[] { 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 };
    var list2 = new double[] { 2.0, 3.0, 4.0 };
    var list3 = new double[] { 8.0, 1.0, 2.0 };
```
- The Program.PredictNextElement() method uses instance of class Predictor to predicts the next element by transversing the given list element by element. The PredictNextElement() method is defined as below:
```csharp
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
```
- Now, we compare the prediction output from "predictor" and "serializedPredictor" instance and the result must be same to verify that serialization and deserialization for instance of Predictor class were correct. 
- As described below in the code, the "predictor" and "serializedPredictor were used as inputs for PredictNextElement() method to predict the next elements respectively. Here, the next elements for list2({ 2.0, 3.0, 4.0 }) were predicted and the results from both the normal predictor and the serialized predictor were same.
```csharp
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
```

### Testing Output
- Below shows the predicted output, which we got by using the normal predictor and the serialized predictor respectively. As shown below, the same prediction was made by both the normal predictor and the serialized predictor with the same accuracy (5, 4, 2).
```
Hello NeocortexApi! Experiment MultiSequenceLearning


                Prediction next elements with normal predictor:


S1_0-1-2-3-4-2-5 - 33.33
S2_10-7-11-8-1-2-9 - 33.33
S1_-1.0-0-1-2-3-4 - 0
Predicted Sequence: S1, predicted next element 5

S1_-1.0-0-1-2-3-4 - 100
S1_2-5-0-1-2-3-4 - 100
S1_-1.0-0-1-2-3-4-2 - 0
S1_0-1-2-3-4-2-5 - 0
S1_1-2-3-4-2-5-0 - 0
Predicted Sequence: S1, predicted next element 4

S1_-1.0-0-1-2-3-4-2 - 100
S1_5-0-1-2-3-4-2 - 100
S1_-1.0-0-1-2-3-4 - 0
S1_0-1-2-3-4-2-5 - 0
S1_1-2-3-4-2-5-0 - 0
Predicted Sequence: S1, predicted next element 2



                Prediction next elements with serialized predictor:


S1_0-1-2-3-4-2-5 - 33.33
S2_10-7-11-8-1-2-9 - 33.33
S1_-1.0-0-1-2-3-4 - 0
Predicted Sequence: S1, predicted next element 5

S1_-1.0-0-1-2-3-4 - 100
S1_2-5-0-1-2-3-4 - 100
S1_-1.0-0-1-2-3-4-2 - 0
S1_0-1-2-3-4-2-5 - 0
S1_1-2-3-4-2-5-0 - 0
Predicted Sequence: S1, predicted next element 4

S1_-1.0-0-1-2-3-4-2 - 100
S1_5-0-1-2-3-4-2 - 100
S1_-1.0-0-1-2-3-4 - 0
S1_0-1-2-3-4-2-5 - 0
S1_1-2-3-4-2-5-0 - 0
Predicted Sequence: S1, predicted next element 2


C:\SE\source\MySeProject\bin\Debug\net6.0\MySeProject.exe (process 9356) exited with code 0.
To automatically close the console when debugging stops, enable Tools->Options->Debugging->Automatically close the console when debugging stops.
Press any key to close this window . . .
```
## Changed Files
1. [Predictor.cs](https://github.com/Hungbth2000/tm_msl_serialization/blob/master/source/NeoCortexApi/Predictor.cs)<br/>
2. [CortexLayer.cs](https://github.com/Hungbth2000/tm_msl_serialization/blob/master/source/NeoCortexApi/Network/CortexLayer.cs)<br/>
3. [HtmClassifier.cs](https://github.com/Hungbth2000/tm_msl_serialization/blob/master/source/NeoCortexApi/Classifiers/HtmClassifier.cs)<br/>
4. [EncoderBase.cs](https://github.com/Hungbth2000/tm_msl_serialization/blob/master/source/NeoCortexApi/Encoders/EncoderBase.cs)<br/>
5. [ScalarEncoder.cs](https://github.com/Hungbth2000/tm_msl_serialization/blob/master/source/NeoCortexApi/Encoders/ScalarEncoder.cs)<br/>
6. [SpatialPooler.cs](https://github.com/Hungbth2000/tm_msl_serialization/blob/master/source/NeoCortexApi/SpatialPooler.cs)<br/>
7. [TemporalMemory.cs](https://github.com/Hungbth2000/tm_msl_serialization/blob/master/source/NeoCortexApi/TemporalMemory.cs)<br/>
8. [Connections.cs](https://github.com/Hungbth2000/tm_msl_serialization/blob/master/source/NeoCortexEntities/Entities/Connections.cs)<br/>
9. [HtmSerializer.cs](https://github.com/Hungbth2000/tm_msl_serialization/blob/master/source/NeoCortexEntities/HtmSerializer.cs)<br/>

