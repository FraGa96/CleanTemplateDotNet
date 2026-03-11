using System.Net;

namespace CleanTemplate.Domain.Exceptions;

public class DomainException(string message) : Exception(message)
{
}

