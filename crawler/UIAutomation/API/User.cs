using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAutomation.API
{
    public enum LoginTypes
    {
        UseTenantSettings = 0,
        ApplicationBased = 1
    };

    public class User : IDocument
    {
        public string Id { get; set; } = "";

        public string TenantId { get; set; } = "";

        public DateTime DateLastLoginUTC { get; set; }

        public string Email { get; set; } = "";

        public string Name { get; set; } = "";

        public string ResetCode { get; set; } = "";

        public DateTime ResetCodeExpiration { get; set; }

        public string MobileClientSecret { get; set; } = "";

        public bool Locked { get; set; }

        public List<string> RoleIds { get; set; } = new List<string>();

        public LoginTypes LoginType { get; set; } = LoginTypes.UseTenantSettings;
    }
}