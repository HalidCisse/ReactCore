using System;
using Newtonsoft.Json;
using Polly.Retry;

namespace Acembly.Ftx.Domain
{   
    public class AppSettings
    {
        public JsonSerializerSettings JsonSettings { get; set; }
        public bool IsDev { get; set; } 
        public ConnectionStringsSettings ConnectionStrings { get; set; } = new ConnectionStringsSettings();
        public EncryptionSettings Encryption { get; set; }               = new EncryptionSettings();
        public EmailingSettings Emailing { get; set; }                   = new EmailingSettings();
        public BrandingSettings Branding { get; set; }                   = new BrandingSettings();
        public ElasticSearchSettings ElasticSearch { get; set; }         = new ElasticSearchSettings();
        public ExternalServicesSettings ExternalServices { get; set; }   = new ExternalServicesSettings();
        public ResilienceSettings Resilience { get; set; }               = new ResilienceSettings();
        public JwtSettings Jwt { get; set; } = new JwtSettings();
    }
        
    public class JwtSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
    }

    public class ResilienceSettings
    {
        public AsyncRetryPolicy RetryPolicy { get; set; }
        public int RetryCount { get; set; }
        public int Timeout { get; set; }
    }
        
    public class ExternalServicesSettings
    {
        public Uri Be { get; set; }

        /// <summary>
        /// Authorization token life span in seconds
        /// </summary>
        public int TokenLifeSpan { get; set; }
    }
    
    public class ElasticSearchSettings
    {
        public Uri[] Nodes { get; set; }
    }

    public class EncryptionSettings
    {
        public string Key { get; set; }
    }

    public class ConnectionStringsSettings
    {
        public string Acembly { get; set; }
        public string Redis { get; set; }
    }

    public class EmailingSettings
    {
        public string SendGridApiKey { get; set; }
    }

    public class BrandingSettings
    {
        public string Brand { get; set; }
        public string SupportTeam { get; set; }
        public string LoginPage { get; set; }
        public string AppPage { get; set; }
        public string AlertsEmail { get; set; }
        public string TestEmail { get; set; }
        public int TestUser { get; set; }
        public string Logo { get; set; }
    }

   
}