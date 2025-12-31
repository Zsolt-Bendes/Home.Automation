namespace Home.Automation.Api.Services.Email.Client;

public sealed class EmailRecipient
{
    private readonly string _displayName;
    private readonly string _emailAddress;

    public EmailRecipient(string displayName, string emailAddress)
    {
        ArgumentException.ThrowIfNullOrEmpty(displayName);
        ArgumentException.ThrowIfNullOrEmpty(emailAddress);

        _displayName = displayName;
        _emailAddress = emailAddress;
    }

    public override string ToString()
    {
        return $"{_displayName} <{_emailAddress}>";
    }
}