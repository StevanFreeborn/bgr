namespace BGR.Console.Removal.Models;

internal interface IModelFactory
{
  Model Create(string resourceName);
}