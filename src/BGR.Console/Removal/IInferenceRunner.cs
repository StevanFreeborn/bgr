namespace BGR.Console.Removal;

internal interface IInferenceRunner
{
  ITensor<float> Run(byte[] model, ITensor<float> inputTensor);
}