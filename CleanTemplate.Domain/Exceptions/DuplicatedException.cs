namespace CleanTemplate.Domain.Exceptions;

public class DuplicatedException(string resourceType, string resourceIdentifier) : DomainException($"{resourceType}: ${resourceIdentifier} already exists")
{
}

