using FluentValidation;
using Home.Automation.Api.Domain.Rooms.Events;
using Home.Automation.Api.Domain.Rooms.ValueObjects;
using Home.Automation.Api.Features.Dashboard;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Attributes;
using Wolverine.Http;

namespace Home.Automation.Api.Features.Room;

public sealed record RegisterRoom(string Name)
{
    public sealed class RegisterRoomValidator : AbstractValidator<RegisterRoom>
    {
        public RegisterRoomValidator()
        {
            RuleFor(_ => _.Name)
                .NotEmpty()
                .MaximumLength(RoomName._maxLength);
        }
    }
}

public sealed record RegisterRoomResponse(Guid Id);

public static class RegisterRoomEndpoint
{
    internal const string _endpoint = "/rooms/register";

    public static async Task<ProblemDetails> LoadAsync(
        RegisterRoom command,
        IDocumentSession documentSession,
        CancellationToken cancellationToken)
    {
        var roomNameExists = await documentSession.Query<DashboardView>()
            .AnyAsync(_ => _.Id == 1 && _.Rooms.Any(_ => _.Name == command.Name), cancellationToken);
        if (roomNameExists)
        {
            return new ProblemDetails
            {
                Title = "Room already exists",
                Status = 400,
            };
        }

        return WolverineContinue.NoProblems;
    }

    [WolverinePost(_endpoint)]
    [Transactional]
    public static RegisterRoomResponse Post(
        RegisterRoom command,
        TimeProvider timeProvider,
        IDocumentSession documentSession)
    {
        var id = Guid.CreateVersion7();

        _ = documentSession.Events.StartStream(id, new RoomRegistered(
            id,
            new RoomName(command.Name),
            timeProvider.GetLocalNow()));

        return new RegisterRoomResponse(id);
    }
}