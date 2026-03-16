using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Api.Services;

/// <summary>
/// Background service that periodically expires abandoned sessions, deletes their
/// temporary files, and purges their in-memory vector store entries.
///
/// This enforces the application's promise of temporary, session-scoped data handling.
/// Sessions are expired after the configured SessionTimeoutMinutes of inactivity.
/// </summary>
public class SessionExpiryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _sessionTimeout;
    private readonly TimeSpan _checkInterval;
    private readonly ILogger<SessionExpiryService> _logger;

    public SessionExpiryService(IConfiguration config, IServiceProvider serviceProvider, ILogger<SessionExpiryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var timeoutMinutes = config.GetValue<int>("SafeQueryAI:SessionTimeoutMinutes", 60);
        _sessionTimeout = TimeSpan.FromMinutes(timeoutMinutes);
        // Check for expired sessions at half the timeout interval
        _checkInterval = TimeSpan.FromMinutes(Math.Max(1, timeoutMinutes / 2));

        _logger.LogInformation(
            "Session expiry configured: timeout={Timeout}, check interval={Interval}.",
            _sessionTimeout, _checkInterval);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_checkInterval, stoppingToken);
            ExpireOldSessions();
        }
    }

    private void ExpireOldSessions()
    {
        // Singletons are resolved directly from the root provider
        var sessionService = _serviceProvider.GetRequiredService<ISessionService>();
        var fileStorage    = _serviceProvider.GetRequiredService<IFileStorageService>();
        var indexing       = _serviceProvider.GetRequiredService<IDocumentIndexingService>();

        var expiredIds = sessionService.GetExpiredSessionIds(_sessionTimeout);

        foreach (var sessionId in expiredIds)
        {
            try
            {
                fileStorage.DeleteSessionFiles(sessionId);
                indexing.RemoveSessionIndex(sessionId);
                sessionService.ClearSession(sessionId);

                _logger.LogInformation(
                    "Session {SessionId} expired and all associated data removed.", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while expiring session {SessionId}.", sessionId);
            }
        }
    }
}
