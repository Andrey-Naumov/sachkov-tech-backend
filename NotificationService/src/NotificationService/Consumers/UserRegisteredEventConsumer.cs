using AccountService.Communication;
using AccountService.Contracts.Messaging;
using CSharpFunctionalExtensions;
using EmailNotification.Contracts.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;
using NotificationService.Infrastructure;

namespace NotificationService.Consumers;

public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IAccountService _accountService;
    private readonly NotificationSettingsDbContext _dbContext;
    private readonly NotificationFactory _notificationFactory;
    private readonly AccountConfirmationDataProvider _accountConfirmationDataProvider;

    public UserRegisteredEventConsumer(
        IAccountService accountService,
        NotificationSettingsDbContext dbContext,
        NotificationFactory notificationFactory,
        AccountConfirmationDataProvider accountConfirmationDataProvider)
    {
        _accountService = accountService;
        _dbContext = dbContext;
        _notificationFactory = notificationFactory;
        _accountConfirmationDataProvider = accountConfirmationDataProvider;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        //check settings
        var settings = await _dbContext.NotificationSettings.FirstOrDefaultAsync(s => s.UserId == context.Message.UserId);

        if (context.Message.UserId == Guid.Empty)
            return;

        if (settings is null)
            throw new Exception();

        // data generation
        var data = await _accountConfirmationDataProvider.GetNotificationDataAsync(context.Message.UserId);

        // sending

        var senders = _notificationFactory.GetSenders(settings, context.CancellationToken);

        foreach (INotificationSender sender in senders)
        {
            await sender.SendAsync(context.Message.UserId, data.Value, context.CancellationToken);
        }
    }
}

public record SendWebCommand(Guid UserId, string Subject, string Template, Dictionary<string, string> Data);

public record SendTelegramCommand(Guid UserId, string Subject, string Template, Dictionary<string, string> Data);

public interface INotificationDataProvider
{
    Task<Result<NotificationData>> GetNotificationDataAsync(Guid userId);
}

public class AccountConfirmationDataProvider : INotificationDataProvider
{
    private readonly IAccountService _accountService;

    public AccountConfirmationDataProvider(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<Result<NotificationData>> GetNotificationDataAsync(Guid userId)
    {
        var confirmationResult = await _accountService.GetConfirmationLink(userId);

        if (confirmationResult.IsFailure)
            return Result.Failure<NotificationData>(confirmationResult.Error);

        var data = new Dictionary<string, string>
        {
            {
                "FullName", confirmationResult.Value.UserName
            },
            {
                "ConfirmationLink", confirmationResult.Value.ConfirmationLink
            }
        };

        return new NotificationData(data, "Subject", "email-confirmation");
    }
}

public class NotificationData
{
    public Dictionary<string, string> Data { get; }
    public string Subject { get; }
    public string Template { get; }

    public NotificationData(Dictionary<string, string> data, string subject, string template)
    {
        Data = data;
        Subject = subject;
        Template = template;
    }
}

public class NotificationFactory
{
    private readonly IEnumerable<INotificationSender> _senders;

    public NotificationFactory(IEnumerable<INotificationSender> senders)
    {
        _senders = senders.ToList();
    }

    public IEnumerable<INotificationSender> GetSenders(UserNotificationSettings userNotificationSetting, CancellationToken cancellationToken)
        => _senders.Where(sender => sender.CanSend(userNotificationSetting, cancellationToken));
}

public interface INotificationSender
{
    Task SendAsync(Guid userId, NotificationData notificationData, CancellationToken cancellationToken);

    bool CanSend(UserNotificationSettings userNotificationSetting, CancellationToken cancellationToken);
}

public class EmailNotificationSender : INotificationSender
{
    private readonly IPublishEndpoint _publishEndpoint;
    public EmailNotificationSender(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task SendAsync(Guid userId, NotificationData notificationData, CancellationToken cancellationToken)
    {
        var sendEmailCommand = new SendEmailCommand(
            userId,
            notificationData.Subject,
            notificationData.Template,
            notificationData.Data);

        await _publishEndpoint.Publish(sendEmailCommand, cancellationToken);
    }

    public bool CanSend(UserNotificationSettings userNotificationSetting, CancellationToken cancellationToken)
        => userNotificationSetting.SendEmail;
}