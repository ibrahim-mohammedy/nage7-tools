using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAutomation.API
{
    public class Role : IDocument
    {
        public const string BasicUser = "Basic User";
        public const string PowerUser = "Power User";
        public const string Administrator = "Administrator";

        public string Id { get; set; } = "";

        public string TenantId { get; set; } = "";

        public string Name { get; set; } = string.Empty;

        public List<string> Permissions { get; set; } = new List<string>();
    }

    // stolen from ..\bamba\CORE21_NG_1\Upland.Hydra.Database\RolePermissions.cs
    public static class RolePermissions
    {
        public const string CAPTURE_USER = "CP_US";
        public const string ROLES_MANAGE = "RL_MG";
    }
}