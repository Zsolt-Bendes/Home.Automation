using Home.Automation.Api.Domain.Common;
using Throw;

namespace Home.Automation.Api.Domain.Devices.ValueObjects;

public sealed class DeviceName(string name) : ValueObject
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
