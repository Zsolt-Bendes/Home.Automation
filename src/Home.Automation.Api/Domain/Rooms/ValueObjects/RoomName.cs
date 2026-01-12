using Home.Automation.Api.Domain.Common;
using Throw;

namespace Home.Automation.Api.Domain.Rooms.ValueObjects;

public sealed class RoomName(string name) : ValueObject
{
    internal const int _maxLength = 20;

    public string Name { get; } = name.Throw()
            .IfNullOrEmpty(_ => _)
            .Throw()
            .IfLongerThan(_maxLength);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}
