namespace OnnxObjectDetection
{
    public interface IOnnxModel
    {
        string ModelPath         { get; }
        string ModelInput        { get; }
        string ModelOutput       { get; }

        string[] Labels          { get; }
        (float, float)[] Anchors { get; }
    }

    public interface IOnnxObjectPrediction
    {
        float[] PredictedLabels  { get; set; }
    }
}
