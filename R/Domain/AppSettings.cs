using Newtonsoft.Json;

namespace R.Domain
{   
    public class AppSettings
    {
        public JsonSerializerSettings JsonSettings { get; set; }
        public bool IsDev { get; set; } 
    }
    
}