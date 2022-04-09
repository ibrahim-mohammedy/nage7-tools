using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIAutomation.Core;

namespace UIAutomation.API
{
    public enum Scaling
    {
        Scale = 0,
        NoScale = 1,
    }

    public enum Resolution
    {
        Standard = 0,
        Fine = 1,
    }

    public enum Rendering
    {
        Greyscale = 0,
        BW = 1,
    }

    public class Outcome
    {
        public string Id { get; set; }

        public string NextId { get; set; }

        public string OutcomeType { get; set; }

        public string Text { get; set; } = "";
        public string Label { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public bool RequiresValidMetaData { get; set; } = true;
        public bool RequiresConfirmation { get; set; } = false;
    }

    public class WorkflowStep
    {
        public const string START_STEP = "START_STEP";
        public const string END_STEP = "END_STEP";

        public string Id { get; set; }

        public string StepType { get; set; }

        public string Name { get; set; } = "";

        public List<Outcome> Outcomes { get; set; } = new List<Outcome>();

        public List<string> Capabilities { get; set; } = new List<string>();
        public List<string> RoleIds { get; set; } = new List<string>();

        public List<string> MetaDataIds = new List<string>();
        public List<string> ItemGroupingIds = new List<string>();

        // Delivery based steps
        public string LineItemMetaDataId { get; set; }

        public MetaDataCollection Mappings { get; set; } = new MetaDataCollection();
        public MetaDataCollection LineItemMappings { get; set; } = new MetaDataCollection();
        public string FinalFormCode { get; set; } = "pdf";

        // Export To Email

        public List<Recipient> Destinations { get; set; } = new List<Recipient>(); public string Subject { get; set; }
        public string Body { get; set; }

        // Export to Fax

        public string Username { get; set; }
        public string Password { get; set; }
        public string FaxNumber { get; set; }
        public string CSID { get; set; }
        public string TopLine { get; set; }
        public int RetryCount { get; set; }
        public Scaling Scaling { get; set; }
        public Resolution Resolution { get; set; }
        public Rendering Rendering { get; set; }

        // Export to FileBound

        public string FileBoundURL { get; set; } = "";
        public string ProjectId { get; set; } = "";
        public string ProjectName { get; set; } = "";
        public string File { get; set; } = "";
        public string RouteId { get; set; } = "";
        public string RouteName { get; set; } = "";
        public bool OnlyDeliveryFirstPage { get; set; }
        public string LineItemProjectName { get; set; } = "";
    }

    public class ScanSetting
    {
    }

    public class Workflow : IDocument
    {
        public string Id { get; set; } = "";

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public bool AllowRoutingSheetGeneration { get; set; } = false;

        public List<WorkflowStep> WorkflowSteps { get; set; } = new List<WorkflowStep>();
        public List<ScanSetting> ScanSettings { get; set; } = new List<ScanSetting>();
        public List<string> Prompts { get; set; } = new List<string>();
        public List<string> RolesAllowedToSubmitFrom { get; set; } = new List<string>();
    }

    public class ImporterAgent
    {
        public const string InterFAXUsername = "rdouglass";
        public const string InterFAXPassword = "Lootmo1231";

        public const string IMapUsername = "uplandintelligentcapture@gmail.com";
        public const string IMapPassword = "Hail7Hydra";
        public const string IMapHost = "imap.gmail.com";
        public const string IMapPort = "993";

        public const string Pop3Username = "uplandintelligentcapture@gmail.com";
        public const string Pop3Password = "Hail7Hydra";
        public const string Pop3Host = "pop.gmail.com";
        public const string Pop3Port = "995";

        public string AgentType { get; set; }

        // InterFAX
        public string Username { get; set; }

        public string Password { get; set; }
        public bool ReceiveForAllAccounts { get; set; }

        public string Host { get; set; }
        public string Port { get; set; }

        public static ImporterAgent InterFAX()
        {
            return new ImporterAgent
            {
                AgentType = "Upland.Hydra.Importers.Fax.InterFAX",
                Username = InterFAXUsername,
                Password = InterFAXPassword,
                ReceiveForAllAccounts = false
            };
        }

        public static ImporterAgent POP3()
        {
            return new ImporterAgent
            {
                AgentType = "Upland.Hydra.Importers.Email.Pop3",
                Host = Pop3Host,
                Port = Pop3Port,
                Username = Pop3Username,
                Password = Pop3Password,
                ReceiveForAllAccounts = false
            };
        }

        public static ImporterAgent IMap()
        {
            return new ImporterAgent
            {
                AgentType = "Upland.Hydra.Importers.Email.IMap",
                Host = IMapHost,
                Port = IMapPort,
                Username = IMapUsername,
                Password = IMapPassword,
                ReceiveForAllAccounts = false
            };
        }
    }

    public class Importer : IDocument
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";

        public string WorkflowId { get; set; }

        public ImporterAgent Agent { get; set; }
    }

    public class IntegratedApplication : IDocument
    {
        public string Id { get; set; } = "";
        public string Application { get; set; } = "";
        public bool Enabled { get; set; } = false;
        public List<string> RoleIds { get; set; } = new List<string>();
        public MetaDataCollection Data { get; set; }
    }

    // stolen from \bamba\CORE21_NG_1\Upland.Hydra.Core\Email\Addressee.cs

    public enum RecipientType { To = 1, CC = 2, BCC = 3 } // note this matches AccuRoute prMessageRecipType

    public class Addressee
    {
        public Addressee()
        {
        }

        public Addressee(string email, string name)
        {
            EmailAddress = email;
            Name = name;
        }

        public string EmailAddress { get; set; } = "";
        public string Name { get; set; } = "";

        public override string ToString() => $"<{EmailAddress}> '{Name}'";
    }

    public class Originator : Addressee
    {
        public Originator()
        {
        }

        public Originator(string email, string name) : base(email, name)
        {
        }
    }

    public class Recipient : Addressee
    {
        public Recipient()
        {
        }

        public Recipient(string email, string name, RecipientType type = RecipientType.To) : base(email, name)
        {
            Type = type;
        }

        public RecipientType Type { get; set; } = RecipientType.To;

        public override string ToString() => $"{Type}: {base.ToString()}";
    }
}