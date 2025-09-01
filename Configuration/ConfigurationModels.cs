namespace POSSystem.Configuration;

/// <summary>
/// Root configuration class for POS system settings
/// </summary>
public class POSConfiguration
{
    public GeneralSettings? GeneralSettings { get; set; }
    public PaymentSettings? PaymentSettings { get; set; }
    public SecuritySettings? SecuritySettings { get; set; }
    public ReportingSettings? ReportingSettings { get; set; }
    public InventorySettings? InventorySettings { get; set; }
}
/// <summary>
/// General system settings
/// </summary>
public class GeneralSettings
{
    public decimal TaxRate { get; set; }
    public string Currency { get; set; } = "USD";
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
}
/// <summary>
/// Payment processing settings
/// </summary>
public class PaymentSettings
{
    public List<PaymentMethodConfig> AcceptedMethods { get; set; } = new();
    public CreditCardSettings? CreditCardProcessing { get; set; }
}
/// <summary>
/// Individual payment method configuration
/// </summary>
public class PaymentMethodConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}
/// <summary>
/// Credit card processing settings
/// </summary>
public class CreditCardSettings
{
    public string Provider { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
}
/// <summary>
/// Security and authentication settings
/// </summary>
public class SecuritySettings
{
    public int SessionTimeout { get; set; } = 30;
    public int MaxLoginAttempts { get; set; } = 3;
    public PasswordPolicy? PasswordPolicy { get; set; }
}
/// <summary>
/// Password policy settings
/// </summary>
public class PasswordPolicy
{
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireNumbers { get; set; } = true;
    public bool RequireSpecialChars { get; set; } = true;
}
/// <summary>
/// Reporting configuration
/// </summary>
public class ReportingSettings
{
    public string DailyReportTime { get; set; } = "23:59";
    public bool EmailReports { get; set; } = true;
    public List<string> ReportRecipients { get; set; } = new();
}
/// <summary>
/// Inventory management settings
/// </summary>
public class InventorySettings
{
    public int LowStockThreshold { get; set; } = 10;
    public bool AutoReorderEnabled { get; set; } = false;
    public int ReorderQuantity { get; set; } = 50;
}