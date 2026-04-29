namespace tomware.Tapir.Cli.Domain;

internal interface IResponseContentValidatorFactory
{
  IResponseContentValidator ResolveValidator(string contentType);
}

internal class ResponseContentValidatorFactory : IResponseContentValidatorFactory
{
  private readonly IReadOnlyDictionary<string, IResponseContentValidator> _validators;

  public ResponseContentValidatorFactory(IEnumerable<IResponseContentValidator> validators)
  {
    _validators = validators.ToDictionary(
      validator => validator.ContentType,
      StringComparer.OrdinalIgnoreCase
    );
  }

  public IResponseContentValidator ResolveValidator(string contentType)
  {
    if (_validators.TryGetValue(contentType, out var validator))
    {
      return validator;
    }

    throw new InvalidOperationException(
      $"Unsupported response content type '{contentType}'."
    );
  }
}