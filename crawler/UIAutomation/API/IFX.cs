using InterFAXUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UIAutomation.InterFAXAdmin;
using UIAutomation.Tests;

namespace UIAutomation.API
{
    public static class InterFAXExtensions
    {
        public static string Get(this PropertyKeyValue[] pkvs, string name)
        {
            if (pkvs == null) return "";

            PropertyKeyValue pkv = pkvs.FirstOrDefault(x => x.Key == name);
            if (pkv == null) return "";

            return pkv.Value;
        }

        public static void Set(this PropertyKeyValue[] pkvs, string name, string value)
        {
            if (pkvs == null) return;

            PropertyKeyValue pkv = pkvs.FirstOrDefault(x => x.Key == name);
            if (pkv == null) return;

            pkv.Value = value;
        }

        public static string Get(this ContactPropertyValue[] pkvs, string name)
        {
            if (pkvs == null) return "";

            ContactPropertyValue pkv = pkvs.FirstOrDefault(x => x.Name == name);
            if (pkv == null) return "";

            return pkv.Value;
        }

        public static void Set(this ContactPropertyValue[] pkvs, string name, string value)
        {
            if (pkvs == null) return;

            ContactPropertyValue pkv = pkvs.FirstOrDefault(x => x.Name == name);
            if (pkv == null) return;

            pkv.Value = value;
        }
    }

    public static class Literals
    {
        public class Contact
        {
            public const string Name = "Person";
            public const string Company = "Company";
            public const string FaxNumber = "Fax";
            public const string PhoneNumber = "Voice";
        }

        public class UserGeneralProps
        {
            public const string Email = "Email";
            public const string ContactName = "ContactName";
            public const string UserType = "UserType";
            public const string UserID = "UserID";

            public const string CSID = "CSID";
            public const string TimeZone = "TimeZone";
            public const string ClosedUser = "ClosedUser";
            public const string SwitchGroupName = "SwitchGroupName";
            public const string AccountType = "AccountType";
            public const string CostCenterId = "CostCenterId";
            public const string OpenDate = "OpenDate";

            public const string UserTypePrimary = "Primary";

            public const string Notes = "Notes";
        }

        public class UserRxProps
        {
            public const string FormattedPhoneNumber = "FormattedPhoneNumber";
        }
    }

    public class WS
    {
        AdminSoapClient _admin;

        public AdminSoapClient Admin
        {
            get
            {
                if (_admin == null)
                {
                    _admin = new AdminSoapClient("AdminSoap");
                }

                return _admin;
            }
        }

        UtilsSoapClient _utils;

        public UtilsSoapClient Utils
        {
            get
            {
                if (_utils == null)
                {
                    _utils = new UtilsSoapClient(UtilsSoapClient.EndpointConfiguration.UtilsSoap, "https://ws.interfax.net/utils.asmx");
                }

                return _utils;
            }
        }
    }

    public class Credentials
    {
        public string Username { get; }
        public string Password { get; }
        public string Name { get; }
        public string Type { get; }
        public bool OutboundService { get; }
        public bool InboundService { get; }
        public string InboundFaxNumber { get; }
        public bool AccountManager { get; }

        public Credentials(bool accountManager, string u, string p, string n, string t, bool outbound, bool inbound, string faxnumber)
        {
            AccountManager = accountManager;
            Username = u;
            Password = p;
            Name = n;
            Type = t;
            OutboundService = outbound;
            InboundService = inbound;
            InboundFaxNumber = faxnumber;
        }
    }

    public class InterFAXObject
    {
        protected WS API { get; }
        protected Credentials Credentials { get; set; }

        public InterFAXObject(WS api, Credentials c)
        {
            API = api;
            Credentials = c;
        }

        public void As(Credentials c) => Credentials = c;
    }

    public class ContactGenerator
    {
        public ContactGenerator()
        {
        }

        public virtual string GetName(int contact) => $"{Name}{contact}";

        public virtual string GetFaxNumber(int contact) => $"{FaxNumber}{contact}";

        public virtual string GetCompany(int contact) => $"{Company}{contact}";

        public virtual string GetPhoneNumber(int contact) => $"{PhoneNumber}{contact}";

        public string Name { get; set; } = "Name ";
        public string FaxNumber { get; set; } = "+1207555121";
        public string Company { get; set; } = "Company ";
        public string PhoneNumber { get; set; } = "+1207666121";
    }

    public class ContactLists : InterFAXObject
    {
        public ContactLists(WS api, Credentials c) : base(api, c)
        {
        }

        public int Create(string name, bool isPrivate, int contactsToAdd = 10, ContactGenerator cg = null)
        {
            DeleteIfExists(cl => cl.ListName == name);

            if (cg == null) cg = new ContactGenerator();

            ContactList newList = new ContactList();

            newList.ListName = name;
            newList.IsPrivate = isPrivate;

            ContactListResult r = API.Utils.AddContactListAsync(Credentials.Username, Credentials.Password, newList).Result;
            if (r.ResultCode != 0) throw new Exception($"AddContactList failed {r.ResultCode}");

            for (int contact = 0; contact < contactsToAdd; contact++)
            {
                Contact c = new Contact();
                c.PropertiesValues = new ContactPropertyValue[4];

                c.ContactListID = r.ContactList.ContactListID;
                c.PropertiesValues[0] = new InterFAXUtils.ContactPropertyValue() { Name = Literals.Contact.Name, Value = cg.GetName(contact) };
                c.PropertiesValues[1] = new InterFAXUtils.ContactPropertyValue() { Name = Literals.Contact.FaxNumber, Value = cg.GetFaxNumber(contact) };
                c.PropertiesValues[2] = new InterFAXUtils.ContactPropertyValue() { Name = Literals.Contact.Company, Value = cg.GetCompany(contact) };
                c.PropertiesValues[3] = new InterFAXUtils.ContactPropertyValue() { Name = Literals.Contact.PhoneNumber, Value = cg.GetPhoneNumber(contact) };

                int cr = API.Utils.AddContactAsync(Credentials.Username, Credentials.Password, c).Result;
                if (cr != 0) throw new Exception($"AddContact failed {cr}");
            }

            ContactList verifyExists = Get().FirstOrDefault(cl => cl.ListName == name);
            if (verifyExists == null) throw new Exception($"Failed to verify created contactlist {name}");

            return r.ContactList.ContactListID;
        }

        public List<ContactList> Get()
        {
            ContactsLists cl = API.Utils.GetContactListsAsync(Credentials.Username, Credentials.Password, false).Result;
            if (cl.ResultCode != 0) throw new Exception($"GetContactLists failed {cl.ResultCode}");

            if (cl.Lists == null) return new List<ContactList>();
            return cl.Lists.ToList();
        }

        public List<Contact> GetContactsFor(int contactListId)
        {
            Contacts contacts = API.Utils.GetContactsAsync(Credentials.Username, Credentials.Password, contactListId).Result;
            if (contacts.ResultCode != 0) throw new Exception($"GetContactsAsync failed {contacts.ResultCode}");

            return contacts.List.ToList();
        }

        public void DeleteIfExists(Func<ContactList, bool> func)
        {
            foreach (ContactList cl in Get().Where(func))
            {
                int r = API.Utils.RemoveContactListAsync(Credentials.Username, Credentials.Password, cl.ContactListID).Result;
                if (r != 0) throw new Exception($"RemoveContactListAsync failed {r}");
            }
        }

        public void Delete(int contactListID)
        {
            int r = API.Utils.RemoveContactListAsync(Credentials.Username, Credentials.Password, contactListID).Result;
            if (r != 0) throw new Exception($"RemoveContactListAsync failed {r}");
        }
    }

    public class CostCenter
    {
        public CostCenter()
        {
        }

        public CostCenter(ExtCostCenter ecc)
        {
            Id = ecc.CostCenterID;
            Name = ecc.CostCenterName;
        }

        public uint Id { get; set; }
        public string Name { get; set; }
        public List<string> Members { get; set; }
    }

    public class CostCenters : InterFAXObject
    {
        public CostCenters(WS api, Credentials c) : base(api, c)
        {
        }

        public List<CostCenter> Get()
        {
            ExtCostCenterResult r = API.Admin.GetCostCentersAsync(Credentials.Username, Credentials.Password).Result;
            if (r.ResultCode != 0) throw new Exception($"GetCostCentersAsync failed {r.ResultCode}");

            List<CostCenter> costCenters = new List<CostCenter>();

            foreach (ExtCostCenter ecc in r.CostCenterList)
            {
                CostCenter cc = new CostCenter(ecc);

                UserInfoResult users = API.Admin.GetUsersInCostCenterAsync(Credentials.Username, Credentials.Password, cc.Id, "").Result;
                if (users.ResultCode != 0) throw new Exception($"GetUsersInCostCenterAsync failed {users.ResultCode}");

                cc.Members = users.Users.Select(u => u.UserID).ToList();

                costCenters.Add(cc);
            }

            return costCenters;
        }

        public void Create(string name, List<string> members)
        {
            DeleteIfExists(cc => cc.Name == name);

            int resultCode = API.Admin.AddNewCostCenterAsync(Credentials.Username, Credentials.Password, name).Result;
            if (resultCode != 0) throw new Exception($"AddNewCostCenterAsync failed {resultCode}");

            CostCenter costCenter = Get().Where(x => x.Name == name).FirstOrDefault();

            resultCode = API.Admin.MoveUsersToCostCenterAsync(Credentials.Username, Credentials.Password, costCenter.Id, members.ToArray()).Result;
            if (resultCode != 0) throw new Exception($"MoveUsersToCostCenterAsync failed {resultCode}");
        }

        public void DeleteIfExists(Func<CostCenter, bool> func)
        {
            foreach (CostCenter cc in Get().Where(func))
            {
                int r = API.Admin.RemoveCostCenterAsync(Credentials.Username, Credentials.Password, cc.Id).Result;
                if (r != 0) throw new Exception($"RemoveCostCenterAsync failed {r}");
            }
        }

        public CostCenter GetByName(string name)
        {
            return Get().FirstOrDefault(cc => cc.Name == name);
        }
    }

    public class UserProfile
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string UserType { get; set; }

        public string Notes { get; set; }
        public string CSID { get; set; }

        public string TimeZone { get; set; }

        public bool InboundEnabled { get; set; }
        public bool OutboundEnabled { get; set; }
        public bool Lite { get; set; }
        public bool Suspended { get; set; }
        public bool Closed { get; set; }
        public bool Primary { get; set; }
        public string FormattedPhoneNumber { get; set; }
    }

    public class UserProfiles : InterFAXObject
    {
        public UserProfiles(WS api, Credentials c) : base(api, c)
        {
        }

        public UserProfile Get()
        {
            InterFAXAdmin.UserGeneralProps userProps = API.Admin.GetUserGeneralPropertiesAsync(Credentials.Username, Credentials.Password).Result;

            User_Result up = API.Admin.GetUserPropertiesEx2Async(Credentials.Username, Credentials.Password, Credentials.Username).Result;
            if (up.ResultCode != 0) throw new Exception($"GetUserPropertiesEx2Async failed {up.ResultCode}");

            UserProfile userProfile = new UserProfile();

            userProfile.Username = Credentials.Username;
            userProfile.Email = up.Data.UserGeneralProps.Get(Literals.UserGeneralProps.Email);
            userProfile.Name = up.Data.UserGeneralProps.Get(Literals.UserGeneralProps.ContactName);
            userProfile.UserType = up.Data.UserGeneralProps.Get(Literals.UserGeneralProps.UserType);
            userProfile.Notes = up.Data.UserGeneralProps.Get(Literals.UserGeneralProps.Notes);
            userProfile.CSID = up.Data.UserGeneralProps.Get(Literals.UserGeneralProps.CSID);
            userProfile.InboundEnabled = userProps.IsRx;
            userProfile.OutboundEnabled = userProps.IsTx;
            userProfile.Suspended = userProps.IsSuspended;
            userProfile.Closed = userProps.IsClosed;
            userProfile.Lite = userProps.IsLite;
            userProfile.Primary = userProps.IsPrimary;
            userProfile.TimeZone = userProps.TZName;

            userProfile.FormattedPhoneNumber = up.Data.UserRxProps.Get(Literals.UserRxProps.FormattedPhoneNumber);

            return userProfile;
        }

        public void Update(UserProfile userProfile)
        {
            User_Result ur = API.Admin.GetUserPropertiesEx2Async(Credentials.Username, Credentials.Password, Credentials.Username).Result;
            if (ur.ResultCode != 0) throw new Exception($"GetUserPropertiesEx2Async failed {ur.ResultCode}");

            UserProps up = ur.Data;
            up.UserGeneralProps.Set(Literals.UserGeneralProps.Email, userProfile.Email);
            up.UserGeneralProps.Set(Literals.UserGeneralProps.ContactName, userProfile.Name);
            up.UserGeneralProps.Set(Literals.UserGeneralProps.TimeZone, userProfile.TimeZone);
            up.UserGeneralProps.Set(Literals.UserGeneralProps.Notes, userProfile.Notes);
            up.UserGeneralProps.Set(Literals.UserGeneralProps.CSID, userProfile.CSID);

            SetUserPropertiesExResponse r = API.Admin.SetUserPropertiesExAsync(Credentials.Username, Credentials.Password, up.UserGeneralProps).Result;
            if (r.SetUserPropertiesExResult != 0) throw new Exception("SetUserPropertiesExAsync");
        }
    }

    public class Groups : InterFAXObject
    {
        public Groups(WS api, Credentials c) : base(api, c)
        {
        }

        public void Create(string name, List<string> members)
        {
            DeleteIfExists(cl => cl.Name == name);

            UserGroupInfo group = new UserGroupInfo();

            group.Name = name;

            int resultCode = API.Admin.AddUserGroupToAccountAsync(Credentials.Username, Credentials.Password, name).Result;
            if (resultCode != 0) throw new Exception($"AddUserGroupToAccount failed {resultCode}");

            members.ForEach(x => API.Admin.AddUserToGroup(Credentials.Username, Credentials.Password, name, x));

            UserGroupInfo verifyExists = Get().FirstOrDefault(g => g.Name == name);
            if (verifyExists == null) throw new Exception($"Failed to verify created group {name}");
        }

        public List<UserGroupInfo> Get()
        {
            ResultOfArrayOfUserGroupInfo groups = API.Admin.GetGroupsForAccountAsync(Credentials.Username, Credentials.Password).Result;
            if (groups.Status != 0) throw new Exception($"GetGroupsForAccountAsync failed {groups.Status}");

            if (groups.data == null) return new List<UserGroupInfo>();
            return groups.data.ToList();
        }

        public UserGroupInfo GetByName(string groupName)
        {
            ResultOfArrayOfUserGroupInfo groups = API.Admin.GetGroupsForAccountAsync(Credentials.Username, Credentials.Password).Result;
            if (groups.Status != 0) throw new Exception($"GetGroupsForAccountAsync failed {groups.Status}");

            if (groups.data == null) return null;
            return groups.data.ToList().Find(x => x.Name == groupName);
        }

        public List<string> GetMembersFor(string groupName)
        {
            ResultOfArrayOfString result = API.Admin.GetusersInGroupAsync(Credentials.Username, Credentials.Password, groupName).Result;
            if (result.Status != 0) throw new Exception($"GetusersInGroupAsync failed {result.Status}");

            return result.data.ToList();
        }

        public void DeleteIfExists(Func<UserGroupInfo, bool> func)
        {
            foreach (UserGroupInfo group in Get().Where(func))
            {
                int r = API.Admin.RemoveUserGroupFromAccountAsync(Credentials.Username, Credentials.Password, group.Name).Result;
                if (r != 0) throw new Exception($"RemoveUserGroupFromAccountAsync failed {r}");
            }
        }

        public void AddUserToGroup(string groupName, string userName)
        {
            List<string> existingMembers = GetMembersFor(groupName);

            if (existingMembers.Contains(groupName)) return;

            int r = API.Admin.AddUserToGroupAsync(Credentials.Username, Credentials.Password, groupName, userName).Result;
            if (r != 0) throw new Exception($"AddUserToGroupAsync failed {r}");
        }
    }

    public class Account : InterFAXObject
    {
        public Account(WS api, Credentials c) : base(api, c)
        {
        }

        public List<string> GetAccountUsersNames()
        {
            AccountUsers_Result r = API.Admin.GetAccountUsersExAsync(Credentials.Username, Credentials.Password).Result;
            if (r.ResultCode != 0) new Exception($"GetAccountUsersExAsync failed {r}");

            return r.Users.Where(u => u.UserGeneralProps.Get(Literals.UserGeneralProps.ClosedUser) != "Yes").Select(u => u.UserGeneralProps.Get(Literals.UserGeneralProps.UserID)).ToList();
        }
    }

    public class CustomProperties : InterFAXObject
    {
        public CustomProperties(WS api, Credentials c) : base(api, c)
        {
        }

        public void Create(string name, DocumentPropertyDataType type)
        {
            DeleteIfExists(cl => cl.Name == name);

            AccountDocumentPropertyDefinition property = new AccountDocumentPropertyDefinition();
            property.Name = name;
            property.Type = type;

            int resultCode = API.Admin.AddAccountDocumentPropertyAsync(Credentials.Username, Credentials.Password, property).Result;
            if (resultCode != 0) throw new Exception($"AddAccountDocumentPropertyAsync failed {resultCode}");
        }

        public List<AccountDocumentPropertyDefinition> Get()
        {
            ResultOfAccountDocumentPropertyDefinition properties = API.Admin.GetAccountDocumentPropertiesAsync(Credentials.Username, Credentials.Password).Result;
            if (properties.Status != 0) throw new Exception($"GetAccountDocumentPropertiesAsync failed {properties.Status}");

            return properties.data.List.ToList();
        }

        public AccountDocumentPropertyDefinition GetByName(string name)
        {
            return Get().Where(x => x.Name == name).FirstOrDefault();
        }

        public void DeleteIfExists(Func<AccountDocumentPropertyDefinition, bool> func)
        {
            List<AccountDocumentPropertyDefinition> properties = Get().Where(func).ToList();
            foreach (AccountDocumentPropertyDefinition property in properties)
            {
                int r = API.Admin.RemoveAccountDocumentPropertyAsync(Credentials.Username, Credentials.Password, property).Result;
                if (r != 0) throw new Exception($"RemoveAccountDocumentPropertyAsync failed {r}");
            }
        }
    }

    public class IFX
    {
        WS API { get; }
        Test Test { get; }

        Credentials Credentials { get; set; }

        public IFX(Test test)
        {
            Test = test;
            API = new WS();
        }

        public bool Configured => Credentials != null;
        public bool AccountManager => Credentials != null && Credentials.AccountManager;

        public void As(Credentials c)
        {
            Credentials = c;
            if (_contactLists != null) _contactLists.As(c);
            if (_groups != null) _groups.As(c);
            if (_account != null) _account.As(c);
        }

        CostCenters _costCenters;

        public CostCenters CostCenters
        {
            get
            {
                if (_costCenters == null)
                {
                    _costCenters = new CostCenters(API, Credentials);
                }

                return _costCenters;
            }
        }

        ContactLists _contactLists;

        public ContactLists ContactLists
        {
            get
            {
                if (_contactLists == null)
                {
                    _contactLists = new ContactLists(API, Credentials);
                }

                return _contactLists;
            }
        }

        Groups _groups;

        public Groups Groups
        {
            get
            {
                if (_groups == null)
                {
                    _groups = new Groups(API, Credentials);
                }

                return _groups;
            }
        }

        CustomProperties _customProperties;

        public CustomProperties CustomProperties
        {
            get
            {
                if (_customProperties == null)
                {
                    _customProperties = new CustomProperties(API, Credentials);
                }

                return _customProperties;
            }
        }

        Account _account;

        public Account Account
        {
            get
            {
                if (_account == null)
                {
                    _account = new Account(API, Credentials);
                }

                return _account;
            }
        }

        UserProfiles _userProfiles;

        public UserProfiles UserProfiles
        {
            get
            {
                if (_userProfiles == null)
                {
                    _userProfiles = new UserProfiles(API, Credentials);
                }

                return _userProfiles;
            }
        }
    }
}