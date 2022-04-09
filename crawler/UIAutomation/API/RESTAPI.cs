using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UIAutomation.API
{
    public class RESTAPI
    {
        public RESTAPI(string url)
        {
            Url = url;

            Users = new Container<User>(this, "users");
            Roles = new Container<Role>(this, "roles");
            MetaData = new Container<MetaData>(this, "metadata");
            DataSources = new Container<DataSource>(this, "datasources");
            DataSourceRows = new Container<DataSourceRow>(this, "datasourcerows");
            Workflows = new Container<Workflow>(this, "workflows");
            Importers = new Container<Importer>(this, "importers");
            IntegratedApplications = new ContainerEx<IntegratedApplication>(this, "integratedapplications");
        }

        public Container<User> Users { get; }
        public Container<Role> Roles { get; }
        public Container<MetaData> MetaData { get; }
        public Container<DataSource> DataSources { get; }
        public Container<DataSourceRow> DataSourceRows { get; }
        public Container<Workflow> Workflows { get; }
        public Container<Importer> Importers { get; }
        public ContainerEx<IntegratedApplication> IntegratedApplications { get; }

        public void Cleanup(IDocument d)
        {
            IDeleteIfExists container = GetContainerFor(d);
            container.DeleteIfExists(e => e.Id == d.Id);
        }

        IDeleteIfExists GetContainerFor(IDocument d)
        {
            if (d is User) return Users;
            if (d is Role) return Roles;
            if (d is MetaData) return MetaData;
            if (d is DataSource) return DataSources;
            if (d is DataSourceRow) return DataSourceRows;
            if (d is Workflow) return Workflows;
            if (d is Importer) return Importers;

            throw new NotSupportedException(d.GetType().Name);
        }

        HttpClient Client = new HttpClient();

        public string Url { get; set; }

        public void Authenticate(string username, string password)
        {
            var authRequest = new { Username = username, Password = password };

            var postData = new StringContent(JsonConvert.SerializeObject(authRequest), Encoding.UTF8, "application/json");

            var r = Client.PostAsync($"{Url}/api/user/authenticate", postData).Result;
            r.EnsureSuccessStatusCode();

            AuthenticationResult ar = JsonConvert.DeserializeObject<AuthenticationResult>(r.Content.ReadAsStringAsync().Result);

            Client.DefaultRequestHeaders.Add("Authorization", $"bearer {ar.Token}");
        }

        public List<T> Enumerate<T>(string objects)
        {
            var r = Client.GetAsync($"{Url}/api/{objects}").Result;
            r.EnsureSuccessStatusCode();

            string json = r.Content.ReadAsStringAsync().Result;

            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        public List<T> EnumerateEx<T>(string objects)
        {
            var r = Client.GetAsync($"{Url}/api/{objects}/all").Result;
            r.EnsureSuccessStatusCode();

            string json = r.Content.ReadAsStringAsync().Result;

            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        public void Delete(string objects, string id)
        {
            var r = Client.DeleteAsync($"{Url}/api/{objects}/{id}").Result;
            r.EnsureSuccessStatusCode();
        }

        public ApiResult<T> Put<T>(string objects, object objectToPost)
        {
            var postData = new StringContent(JsonConvert.SerializeObject(objectToPost), Encoding.UTF8, "application/json");

            var r = Client.PutAsync($"{Url}/api/{objects}", postData).Result;
            r.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<ApiResult<T>>(r.Content.ReadAsStringAsync().Result);
        }

        public ApiResult<T> Post<T>(string objects, IDocument document)
        {
            var postData = new StringContent(JsonConvert.SerializeObject(document), Encoding.UTF8, "application/json");

            var r = Client.PostAsync($"{Url}/api/{objects}/{document.Id}", postData).Result;
            r.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<ApiResult<T>>(r.Content.ReadAsStringAsync().Result);
        }

        public ApiResult<T> Post<T>(string objects, string id, object o)
        {
            var postData = new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");

            var r = Client.PostAsync($"{Url}/api/{objects}/{id}", postData).Result;
            r.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<ApiResult<T>>(r.Content.ReadAsStringAsync().Result);
        }
    }
}