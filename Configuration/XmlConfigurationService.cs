using System.Xml;

namespace POSSystem.Configuration;

/// <summary>
/// Service for managing XML-based configuration
/// Demonstrates XML parsing, serialization, and validation
/// </summary>
public interface IConfigurationService
{
    POSConfiguration LoadConfiguration();
    Task<POSConfiguration> LoadConfigurationAsync();
    void SaveConfiguration(POSConfiguration config);
    T GetSetting<T>(string settingPath, T defaultValue = default!);
    bool ValidateConfiguration(POSConfiguration config, out List<string> errors);
}

/// <summary>
/// Configuration service implementation using XML
/// </summary>
public class XmlConfigurationService : IConfigurationService
{
    private readonly string _configFilePath;
    private readonly ILogger<XmlConfigurationService> _logger;
    private POSConfiguration? _cachedConfig;
    private DateTime _lastLoadTime;

    public XmlConfigurationService(ILogger<XmlConfigurationService> logger)
    {
        _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "pos-config.xml");
        _logger = logger;
    }

    public POSConfiguration LoadConfiguration()
    {
        try
        {
            if (_cachedConfig != null && (DateTime.UtcNow - _lastLoadTime).TotalMinutes < 5)
            {
                return _cachedConfig;
            }

            if (!File.Exists(_configFilePath))
            {
                _logger.LogWarning("Configuration file not found at {Path}. Using default configuration.", _configFilePath);
                return CreateDefaultConfiguration();
            }

            var doc = new XmlDocument();
            doc.Load(_configFilePath);

            var config = ParseConfigurationFromXml(doc);

            if (ValidateConfiguration(config, out var errors))
            {
                _cachedConfig = config;
                _lastLoadTime = DateTime.UtcNow;
                _logger.LogInformation("Configuration loaded successfully from {Path}", _configFilePath);
                return config;
            }
            else
            {
                _logger.LogError("Configuration validation failed: {Errors}", string.Join(", ", errors));
                throw new InvalidOperationException($"Invalid configuration: {string.Join(", ", errors)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration from {Path}", _configFilePath);
            throw;
        }
    }

    public async Task<POSConfiguration> LoadConfigurationAsync()
    {
        return await Task.Run(LoadConfiguration);
    }

    public void SaveConfiguration(POSConfiguration config)
    {
        try
        {
            if (!ValidateConfiguration(config, out var errors))
            {
                throw new ArgumentException($"Invalid configuration: {string.Join(", ", errors)}");
            }

            var doc = SerializeConfigurationToXml(config);

            var directory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            doc.Save(_configFilePath);
            _cachedConfig = config;
            _lastLoadTime = DateTime.UtcNow;

            _logger.LogInformation("Configuration saved successfully to {Path}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration to {Path}", _configFilePath);
            throw;
        }
    }

    public T GetSetting<T>(string settingPath, T defaultValue = default!)
    {
        try
        {
            var config = LoadConfiguration();
            var value = GetSettingValue(config, settingPath);

            if (value == null)
                return defaultValue;

            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting setting {SettingPath}, returning default value", settingPath);
            return defaultValue;
        }
    }

    public bool ValidateConfiguration(POSConfiguration config, out List<string> errors)
    {
        errors = new List<string>();

        if (config.GeneralSettings == null)
        {
            errors.Add("GeneralSettings section is required");
        }
        else
        {
            if (config.GeneralSettings.TaxRate < 0 || config.GeneralSettings.TaxRate > 1)
                errors.Add("TaxRate must be between 0 and 1");

            if (string.IsNullOrWhiteSpace(config.GeneralSettings.CompanyName))
                errors.Add("CompanyName is required");
        }

        if (config.PaymentSettings?.AcceptedMethods == null || !config.PaymentSettings.AcceptedMethods.Any())
        {
            errors.Add("At least one payment method must be configured");
        }

        if (config.SecuritySettings != null)
        {
            if (config.SecuritySettings.SessionTimeout <= 0)
                errors.Add("SessionTimeout must be greater than 0");

            if (config.SecuritySettings.MaxLoginAttempts <= 0)
                errors.Add("MaxLoginAttempts must be greater than 0");
        }

        return !errors.Any();
    }

    private POSConfiguration ParseConfigurationFromXml(XmlDocument doc)
    {
        var config = new POSConfiguration();

        // Parse General Settings
        var generalNode = doc.SelectSingleNode("//GeneralSettings");
        if (generalNode != null)
        {
            config.GeneralSettings = new GeneralSettings
            {
                TaxRate = ParseDecimal(generalNode, "TaxRate", 0.08m),
                Currency = ParseString(generalNode, "Currency", "USD"),
                CompanyName = ParseString(generalNode, "CompanyName", ""),
                CompanyAddress = ParseString(generalNode, "CompanyAddress", ""),
                CompanyPhone = ParseString(generalNode, "CompanyPhone", ""),
                CompanyEmail = ParseString(generalNode, "CompanyEmail", "")
            };
        }

        // Parse Payment Settings
        var paymentNode = doc.SelectSingleNode("//PaymentSettings");
        if (paymentNode != null)
        {
            config.PaymentSettings = new PaymentSettings
            {
                AcceptedMethods = ParsePaymentMethods(paymentNode),
                CreditCardProcessing = ParseCreditCardSettings(paymentNode)
            };
        }

        // Parse Security Settings
        var securityNode = doc.SelectSingleNode("//SecuritySettings");
        if (securityNode != null)
        {
            config.SecuritySettings = new SecuritySettings
            {
                SessionTimeout = ParseInt(securityNode, "SessionTimeout", 30),
                MaxLoginAttempts = ParseInt(securityNode, "MaxLoginAttempts", 3),
                PasswordPolicy = ParsePasswordPolicy(securityNode)
            };
        }

        return config;
    }

    private List<PaymentMethodConfig> ParsePaymentMethods(XmlNode paymentNode)
    {
        var methods = new List<PaymentMethodConfig>();
        var methodNodes = paymentNode.SelectNodes(".//AcceptedMethods/Method");

        if (methodNodes != null)
        {
            foreach (XmlNode methodNode in methodNodes)
            {
                if (methodNode.Attributes != null)
                {
                    methods.Add(new PaymentMethodConfig
                    {
                        Id = int.Parse(methodNode.Attributes["id"]?.Value ?? "0"),
                        Name = methodNode.Attributes["name"]?.Value ?? "",
                        Enabled = bool.Parse(methodNode.Attributes["enabled"]?.Value ?? "true")
                    });
                }
            }
        }

        return methods;
    }

    private CreditCardSettings ParseCreditCardSettings(XmlNode paymentNode)
    {
        var ccNode = paymentNode.SelectSingleNode(".//CreditCardProcessing");
        if (ccNode == null) return new CreditCardSettings();

        return new CreditCardSettings
        {
            Provider = ParseString(ccNode, "Provider", ""),
            ApiEndpoint = ParseString(ccNode, "ApiEndpoint", ""),
            Timeout = ParseInt(ccNode, "Timeout", 30)
        };
    }

    private PasswordPolicy ParsePasswordPolicy(XmlNode securityNode)
    {
        var policyNode = securityNode.SelectSingleNode(".//PasswordPolicy");
        if (policyNode == null) return new PasswordPolicy();

        return new PasswordPolicy
        {
            MinLength = ParseInt(policyNode, "MinLength", 8),
            RequireUppercase = ParseBool(policyNode, "RequireUppercase", true),
            RequireLowercase = ParseBool(policyNode, "RequireLowercase", true),
            RequireNumbers = ParseBool(policyNode, "RequireNumbers", true),
            RequireSpecialChars = ParseBool(policyNode, "RequireSpecialChars", true)
        };
    }

    private static string ParseString(XmlNode node, string elementName, string defaultValue)
    {
        return node.SelectSingleNode(elementName)?.InnerText ?? defaultValue;
    }

    private static decimal ParseDecimal(XmlNode node, string elementName, decimal defaultValue)
    {
        var text = node.SelectSingleNode(elementName)?.InnerText;
        return decimal.TryParse(text, out var result) ? result : defaultValue;
    }

    private static int ParseInt(XmlNode node, string elementName, int defaultValue)
    {
        var text = node.SelectSingleNode(elementName)?.InnerText;
        return int.TryParse(text, out var result) ? result : defaultValue;
    }

    private static bool ParseBool(XmlNode node, string elementName, bool defaultValue)
    {
        var text = node.SelectSingleNode(elementName)?.InnerText;
        return bool.TryParse(text, out var result) ? result : defaultValue;
    }

    private XmlDocument SerializeConfigurationToXml(POSConfiguration config)
    {
        var doc = new XmlDocument();
        var declaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
        doc.AppendChild(declaration);

        var root = doc.CreateElement("POSConfiguration");
        doc.AppendChild(root);

        // Add General Settings
        if (config.GeneralSettings != null)
        {
            var generalNode = doc.CreateElement("GeneralSettings");
            AddElement(doc, generalNode, "TaxRate", config.GeneralSettings.TaxRate.ToString());
            AddElement(doc, generalNode, "Currency", config.GeneralSettings.Currency);
            AddElement(doc, generalNode, "CompanyName", config.GeneralSettings.CompanyName);
            AddElement(doc, generalNode, "CompanyAddress", config.GeneralSettings.CompanyAddress);
            AddElement(doc, generalNode, "CompanyPhone", config.GeneralSettings.CompanyPhone);
            AddElement(doc, generalNode, "CompanyEmail", config.GeneralSettings.CompanyEmail);
            root.AppendChild(generalNode);
        }

        return doc;
    }

    private static void AddElement(XmlDocument doc, XmlNode parent, string name, string value)
    {
        var element = doc.CreateElement(name);
        element.InnerText = value ?? "";
        parent.AppendChild(element);
    }

    private object? GetSettingValue(POSConfiguration config, string settingPath)
    {
        var parts = settingPath.Split('.');

        return parts[0].ToLowerInvariant() switch
        {
            "general" when parts.Length > 1 => GetGeneralSetting(config.GeneralSettings, parts[1]),
            "payment" when parts.Length > 1 => GetPaymentSetting(config.PaymentSettings, parts[1]),
            "security" when parts.Length > 1 => GetSecuritySetting(config.SecuritySettings, parts[1]),
            _ => null
        };
    }

    private object? GetGeneralSetting(GeneralSettings? settings, string property)
    {
        if (settings == null) return null;

        return property.ToLowerInvariant() switch
        {
            "taxrate" => settings.TaxRate,
            "currency" => settings.Currency,
            "companyname" => settings.CompanyName,
            "companyaddress" => settings.CompanyAddress,
            "companyphone" => settings.CompanyPhone,
            "companyemail" => settings.CompanyEmail,
            _ => null
        };
    }

    private object? GetPaymentSetting(PaymentSettings? settings, string property)
    {
        if (settings == null) return null;

        return property.ToLowerInvariant() switch
        {
            "acceptedmethods" => settings.AcceptedMethods,
            _ => null
        };
    }

    private object? GetSecuritySetting(SecuritySettings? settings, string property)
    {
        if (settings == null) return null;

        return property.ToLowerInvariant() switch
        {
            "sessiontimeout" => settings.SessionTimeout,
            "maxloginattempts" => settings.MaxLoginAttempts,
            _ => null
        };
    }

    private POSConfiguration CreateDefaultConfiguration()
    {
        return new POSConfiguration
        {
            GeneralSettings = new GeneralSettings
            {
                TaxRate = 0.08m,
                Currency = "USD",
                CompanyName = "Modern POS System",
                CompanyAddress = "123 Business St, City, State 12345",
                CompanyPhone = "(555) 123-4567",
                CompanyEmail = "info@modernpos.com"
            },
            PaymentSettings = new PaymentSettings
            {
                AcceptedMethods = new List<PaymentMethodConfig>
                    {
                        new() { Id = 1, Name = "Cash", Enabled = true },
                        new() { Id = 2, Name = "CreditCard", Enabled = true },
                        new() { Id = 3, Name = "DebitCard", Enabled = true },
                        new() { Id = 4, Name = "DigitalWallet", Enabled = true }
                    }
            }
        };
    }
}