using System.Net;

namespace CleanTemplate.Domain.Exceptions;

public class NotFoundException(string resourceType, string resourceIdentifier) 
    : DomainException($"{resourceType} with id: ${resourceIdentifier} was not found")
{
}

