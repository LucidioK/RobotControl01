using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using System.Collections.Generic;
using System.Linq;

namespace OnnxObjectDetection
{
    public class OnnxModelConfigurator
    {
        private readonly MLContext mlContext = new MLContext();
        private readonly ITransformer mlModel;

        public OnnxModelConfigurator(IOnnxModel onnxModel) => mlModel = SetupMlNetModel(onnxModel);

        private ITransformer SetupMlNetModel(IOnnxModel onnxModel) =>
            mlContext
                .Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: onnxModel.ModelInput, imageWidth: ImageSettings.imageWidth, imageHeight: ImageSettings.imageHeight, inputColumnName: nameof(ImageInputData.Image))
                           .Append(mlContext.Transforms.ExtractPixels(outputColumnName: onnxModel.ModelInput))
                           .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: onnxModel.ModelPath, outputColumnName: onnxModel.ModelOutput, inputColumnName: onnxModel.ModelInput))
                           .Fit(mlContext.Data.LoadFromEnumerable(new List<ImageInputData>()));
/*
        {
            var dataView = mlContext.Data.LoadFromEnumerable(new List<ImageInputData>());
#if true
            Microsoft.ML.Data.EstimatorChain<Microsoft.ML.Transforms.Onnx.OnnxTransformer> pipeline = mlContext.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: onnxModel.ModelInput, imageWidth: ImageSettings.imageWidth, imageHeight: ImageSettings.imageHeight, inputColumnName: nameof(ImageInputData.Image))
                            .Append(mlContext.Transforms.ExtractPixels(outputColumnName: onnxModel.ModelInput))
                            .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: onnxModel.ModelPath, outputColumnName: onnxModel.ModelOutput, inputColumnName: onnxModel.ModelInput));
            var mlNetModel = pipeline.Fit(dataView);
#else
            var mlNetModel = new Microsoft.ML.Data.EstimatorChain<Microsoft.ML.Transforms.Onnx.OnnxTransformer>()
                            .Append(mlContext.Transforms.ExtractPixels(outputColumnName: onnxModel.ModelInput))
                            .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: onnxModel.ModelPath, outputColumnName: onnxModel.ModelOutput, inputColumnName: onnxModel.ModelInput))
                            .Fit(dataView);
#endif

            return mlNetModel;
        }
*/
        public PredictionEngine<ImageInputData, T> GetMlNetPredictionEngine<T>()
            where T : class, IOnnxObjectPrediction, new() => mlContext.Model.CreatePredictionEngine<ImageInputData, T>(mlModel);

        public void SaveMLNetModel(string mlnetModelFilePath) => mlContext.Model.Save(mlModel, null, mlnetModelFilePath);
    }
}
