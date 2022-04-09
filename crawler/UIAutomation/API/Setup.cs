using MongoDB.Bson;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIAutomation.Tests;

namespace UIAutomation.API
{
    public class Setup
    {
        RESTAPI IC { get; }
        Test Test { get; }

        public Setup(RESTAPI api, Test test)
        {
            IC = api;
            Test = test;
        }

        public User DefaultTestingUser(string prefix = "")
        {
            return User(prefix + Test.TestingUserEmail, prefix + Test.TestingUserName, Test.TestingUserPassword, API.Role.BasicUser, false);
        }

        public User DefaultTestingUser_Locked()
        {
            return User(Test.TestingUserEmail, Test.TestingUserName, Test.TestingUserPassword, API.Role.BasicUser, true);
        }

        public User DefaultTestingUserWithRole(string roleName)
        {
            return User(Test.TestingUserEmail, Test.TestingUserName, Test.TestingUserPassword, roleName, true);
        }

        public User User(string email, string name, string password, string role, bool userLocked)
        {
            IC.Users.DeleteIfExists(u => u.Email == email);

            API.Role basicUserRole = IC.Roles.Find(r => r.Name == role);
            Assert.NotNull(basicUserRole);

            API.CreateUser cu = new API.CreateUser();

            cu.SendWelcomeMessage = false;
            cu.Password = password;

            cu.User.LoginType = LoginTypes.ApplicationBased;
            cu.User.Email = email;
            cu.User.Name = name;
            cu.User.Locked = userLocked;
            cu.User.RoleIds.Add(basicUserRole.Id);

            ApiResult<User> result = IC.Users.Create(cu);
            Assert.True(result.Success);

            Test.AutomaticCleanup(result.Object);
            return result.Object;
        }

        public void CleanupDefaultTestingUser()
        {
            IC.Users.DeleteIfExists(u => u.Email == Test.TestingUserEmail);
        }

        public void CleanupDefaultTestingRole()
        {
            IC.Roles.DeleteIfExists(r => r.Name == Test.TestingRoleName);
        }

        public void CleanupDefaultTestingImporter()
        {
            IC.Importers.DeleteIfExists(i => i.Name == Test.TestingImporterName);
        }

        public void CleanupDefaultTestingWorkflow()
        {
            IC.Workflows.DeleteIfExists(i => i.Name == Test.SanitizedTestName);
        }

        public void CleanupDefaultTestingMetaData()
        {
            IC.MetaData.DeleteIfExists(md => md.Name == Test.TestingMetaDataName);
        }

        public void CleanupDefaultTestingDataSource()
        {
            DataSource datasource = IC.DataSources.Find(md => md.Name == Test.TestingDataSourceName);
            if (datasource == null) return;

            IC.DataSourceRows.DeleteIfExists(md => md.DataSourceId == datasource.Id);
            IC.DataSources.DeleteIfExists(md => md.Id == datasource.Id);
        }

        public Role DefaultTestingRole(string prefix = "")
        {
            return Role(prefix + Test.TestingRoleName, new List<string> { RolePermissions.CAPTURE_USER });
        }

        public Role Role(string name, List<string> permissions)
        {
            IC.Roles.DeleteIfExists(x => x.Name == name);

            Role r = new Role();

            r.Name = name;
            r.Permissions.AddRange(permissions);

            ApiResult<Role> result = IC.Roles.Create(r);
            Assert.True(result.Success);

            Test.AutomaticCleanup(result.Object);
            return result.Object;
        }

        public MetaData DefaultTestingMetaData(string prefix = "", bool autoCleanup = true)
        {
            string name = $"{prefix}{Test.TestingMetaDataName}";

            IC.MetaData.DeleteIfExists(x => x.Name == name);

            MetaData md = new MetaData();

            md.Name = name;
            md.Label = $"Display {Test.TestingMetaDataName}";
            md.Type = MetaDataType.Text;

            ApiResult<MetaData> result = IC.MetaData.Create(md);
            Assert.True(result.Success);

            if (autoCleanup) Test.AutomaticCleanup(result.Object);
            return result.Object;
        }

        public DataSource DefaultTestingDataSource(string prefix = "", bool autoCleanup = true, int numberOfColumns = 3, int numberOfRows = 0)
        {
            string name = $"{prefix}{Test.TestingDataSourceName}";

            IC.DataSources.DeleteIfExists(x => x.Name == name);

            DataSource ds = new DataSource()
            {
                Name = name,
                Columns = new List<DBColumn>()
            };

            for (int i = 1; i <= numberOfColumns; i++)
            {
                ds.Columns.Add(new DBColumn() { Id = ObjectId.GenerateNewId().ToString(), Name = $"Col{i}", Type = 2 });
            }

            ApiResult<DataSource> result = IC.DataSources.Create(ds);
            Assert.True(result.Success);

            if (autoCleanup) Test.AutomaticCleanup(result.Object);
            return result.Object;
        }

        public DataSourceRow DefaultTestingDataSourceRow(bool autoCleanup = true)
        {
            DataSource dataSource = DefaultTestingDataSource();

            DataSourceRow row = new DataSourceRow() { DataSourceId = dataSource.Id, Cells = new List<DataSourceCell>() };
            for (int c = 0; c < dataSource.Columns.Count; c++)
            {
                row.Cells.Add(new DataSourceCell() { ColumnId = dataSource.Columns[c].Id, Value = $"Value {c + 1}" });
            }

            ApiResult<DataSourceRow> result = IC.DataSourceRows.Create(row);
            Assert.True(result.Success);

            if (autoCleanup) Test.AutomaticCleanup(result.Object);
            return result.Object;
        }

        public MetaData DefaultTestingMetaData(MetaDataType type)
        {
            string name = $"{Test.TestingMetaDataName}";

            IC.MetaData.DeleteIfExists(x => x.Name == name);

            MetaData textMetaData = null;
            MetaData md = new MetaData();

            md.Name = name;
            md.Label = $"Display {Test.TestingMetaDataName}";
            md.Type = type;

            // handle List, Lookup, Date, Line Items
            if (type == MetaDataType.Date)
            {
                md.GroupingFormat = "yyyy-mm-dd";
                md.DefaultValue = "1/1/2021";
            }
            else if (type == MetaDataType.LineItems)
            {
                textMetaData = DefaultTestingMetaData("for-lineitem-", false);
                md.LineItemConfigurations.Add(new LineItemConfiguration(textMetaData.Id));
            }
            else if (type == MetaDataType.List)
            {
                md.DataSource.DataSet.Add(new DataSourceItem() { Name = "One", Value = "0001" });
                md.DataSource.DataSet.Add(new DataSourceItem() { Name = "Two", Value = "0002" });
                md.DataSource.DataSet.Add(new DataSourceItem() { Name = "Three", Value = "0003" });
                md.DefaultValue = "0001";
            }
            else if (type == MetaDataType.Lookup)
            {
                md.Lookup.Type = "Upland.Hydra.FileBound.FileBoundLookup";
                md.Lookup.FileBoundURL = "https://accuroute.fileboundrs.com";
                md.Lookup.Username = "admin";
                md.Lookup.Password = "Welcome2FB!1701";
                md.Lookup.ProjectId = "1";
                md.Lookup.ProjectName = "AccuRoute-Integration-Testing";
                md.Lookup.LookupField = 1;
            }
            else if (type == MetaDataType.Integer)
            {
                md.DefaultValue = "100";
            }
            else if (type == MetaDataType.Decimal || type == MetaDataType.Currency)
            {
                md.DefaultValue = "100.00";
            }

            ApiResult<MetaData> result = IC.MetaData.Create(md);
            Assert.True(result.Success);

            Test.AutomaticCleanup(result.Object);
            Test.AutomaticCleanup(textMetaData);

            return result.Object;
        }

        public Importer DefaultTestingImporter(string wfid, ImporterAgent agent, string prefix = "")
        {
            string name = $"{prefix}{Test.SanitizedTestName}";

            IC.Importers.DeleteIfExists(x => x.Name == name);

            Importer importer = new Importer();

            importer.Name = name;
            importer.WorkflowId = wfid;
            importer.Agent = agent;

            ApiResult<Importer> result = IC.Importers.Create(importer);
            Assert.True(result.Success);

            Test.AutomaticCleanup(result.Object);
            return result.Object;
        }

        public IntegratedApplication DefaultTestIntegratedApplication(string application, bool enabled)
        {
            if (application == "CloudFAX") application = "InterFAX"; // internal name remains "InterFAX"

            API.IntegratedApplication integratedApplication = IC.IntegratedApplications.Find(ia => ia.Application == application);
            Assert.NotNull(integratedApplication);

            integratedApplication.Enabled = enabled;
            integratedApplication.RoleIds.Clear();

            API.Role basicUserRole = IC.Roles.Find(r => r.Name == API.Role.BasicUser);
            Assert.NotNull(basicUserRole);
            integratedApplication.RoleIds.Add(basicUserRole.Id);

            IC.IntegratedApplications.Update(integratedApplication);

            return integratedApplication;
        }

        public IntegratedApplication DefaultTestFileBoundIntegratedApplication()
        {
            API.IntegratedApplication integratedApplication = (API.IntegratedApplication)IC.IntegratedApplications.Find(ia => ia.Application == "FileBound");
            Assert.NotNull(integratedApplication);

            integratedApplication.Data.Set("FileBoundUrl", "");
            IC.IntegratedApplications.Update(integratedApplication);

            return integratedApplication;
        }

        public Workflow Workflow(string name, string description = null, List<WorkflowStep> steps = null)
        {
            IC.Workflows.DeleteIfExists(x => x.Name == name);

            if (steps == null)
            {
                steps = new List<WorkflowStep>();

                WorkflowStep start = new WorkflowStep() { Id = "000000000000000000000000", Name = "Start", StepType = "start" };
                start.Capabilities.Add(WorkflowStep.START_STEP);

                WorkflowStep review = new WorkflowStep() { Id = "000000000000000000000002", Name = "Review", StepType = "interactivestep" };

                WorkflowStep done = new WorkflowStep() { Id = "000000000000000000000001", Name = "Done", StepType = "completed" };
                done.Capabilities.Add(WorkflowStep.END_STEP);

                Outcome gotoReview = new Outcome() { Id = "100000000000000000000001", NextId = review.Id, Label = "Review", Enabled = true, OutcomeType = "interactiveoutcome" };
                Outcome gotoFinished = new Outcome() { Id = "100000000000000000000002", NextId = done.Id, Label = "Done", Enabled = true, OutcomeType = "succeeded" };

                start.Outcomes.Add(gotoReview);
                review.Outcomes.Add(gotoFinished);

                steps.Add(start);
                steps.Add(review);
                steps.Add(done);
            }

            Workflow wf = new Workflow();

            wf.Name = name;
            wf.Description = (description == null) ? $"{name} description" : description;
            wf.AllowRoutingSheetGeneration = false;
            wf.WorkflowSteps = steps;

            ApiResult<Workflow> result = IC.Workflows.Create(wf);
            Assert.True(result.Success);

            Test.AutomaticCleanup(result.Object);

            return result.Object;
        }
    }
}