namespace BillPaymentProvider.Configuration
{
    /// <summary>
    /// Configuration pour les paramètres JWT
    /// </summary>
    public class JwtSettings
    {
        public const string Section = "Jwt";
        
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = "BillPaymentProvider";
        public string Audience { get; set; } = "BillPaymentProvider";
        public int LifetimeMinutes { get; set; } = 30;
        public int RefreshTokenLifetimeDays { get; set; } = 7;
        public bool RequireHttpsMetadata { get; set; } = true;
    }

    /// <summary>
    /// Configuration pour les paramètres de sécurité
    /// </summary>
    public class SecuritySettings
    {
        public const string Section = "Security";
        
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;
        public bool EnableBruteForceProtection { get; set; } = true;
        public int PasswordMinLength { get; set; } = 8;
        public bool RequirePasswordComplexity { get; set; } = true;
    }

    /// <summary>
    /// Configuration pour l'API
    /// </summary>
    public class ApiSettings
    {
        public const string Section = "ApiSettings";
        
        public int IdempotencyTimeoutHours { get; set; } = 24;
        public bool SimulateRandomErrors { get; set; } = false;
        public int DefaultProcessingDelay { get; set; } = 200;
        public int MaxConcurrentRequests { get; set; } = 100;
        public bool EnableRequestLogging { get; set; } = true;
        public string[]? AllowedFileExtensions { get; set; }
        public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    }

    /// <summary>
    /// Configuration pour les webhooks
    /// </summary>
    public class WebhookSettings
    {
        public const string Section = "Webhook";
        
        public string Url { get; set; } = string.Empty;
        public bool Enabled { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
        public string? Secret { get; set; }
    }

    /// <summary>
    /// Configuration pour la base de données
    /// </summary>
    public class DatabaseSettings
    {
        public const string Section = "Database";
        
        public string Provider { get; set; } = "SQLite";
        public string ConnectionString { get; set; } = string.Empty;
        public bool EnableSensitiveDataLogging { get; set; } = false;
        public int CommandTimeout { get; set; } = 30;
        public bool EnableDetailedErrors { get; set; } = false;
        public bool EnableServiceProviderCaching { get; set; } = true;
    }

    /// <summary>
    /// Configuration pour les logs
    /// </summary>
    public class LoggingSettings
    {
        public const string Section = "Logging";
        
        public string DefaultLevel { get; set; } = "Information";
        public bool EnableFileLogging { get; set; } = true;
        public string LogPath { get; set; } = "logs/";
        public int RetainDays { get; set; } = 30;
        public long MaxFileSizeMB { get; set; } = 100;
        public bool EnableConsoleLogging { get; set; } = true;
        public bool EnableStructuredLogging { get; set; } = true;
    }
}
