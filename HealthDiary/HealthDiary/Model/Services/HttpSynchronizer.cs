using HealthDiary.Model.Context;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HealthDiary.Model.Services
{
    //TODO: add sync for deleted items on server and sync packages from server (not all content)
    //maybe will be flags on update/delete etc.

    /*like this
     * if (Find(id)) {
     *  if (isReadOnly)
     *      UPDATE
     *  else {
     *      1.Finded to buffer
     *      2.UPDATE
     *      3.ADD from buffer
     *  }
     * }
     * else
     *  ADD
     */

    //TODO: think about syncs after errors such as User Info sync / PlanCompletions sync

    public static class HttpSynchronizer
    {
        public static async Task<SynchronizerResponse> SavePlanCompletions(Uri connection, string dbPath, int user_id, PlanCompletion planCompletion)
        {
            Uri uri = new Uri(connection + "/plancompletion?userid=" + user_id);
            StringContent body = new StringContent(JsonSerializer.Serialize(planCompletion), Encoding.UTF8, "application/json");
            string content = "";
            
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.PostAsync(uri, body).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        content = await response.Content.ReadAsStringAsync();
                    else
                        return SynchronizerResponse.ServerError;
                }
            }
            catch
            {
                return SynchronizerResponse.NetworkError;
            }

            if (!String.IsNullOrEmpty(content))
            {
                try
                {
                    //var array = JArray.Parse(content);
                    //var json = JObject.FromObject(array[0]);
                    var json = JObject.Parse(content);
                    var pc = JsonSerializer.Deserialize<PlanCompletion>(json.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    return SynchronizerResponse.DataFormatError;
                }
            }
            else
                return SynchronizerResponse.EmptyDataError;

            return SynchronizerResponse.Success;
        }

        public static async Task<SynchronizerResponse> InitPlanCompletions(Uri connection, string dbPath, int user_id)
        {
            Uri uri = new Uri(connection + "/plancompletion?userid=" + user_id);
            List<PlanCompletion> planCompletions = new List<PlanCompletion>();
            string content = "";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(uri).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        content = await response.Content.ReadAsStringAsync();
                    else
                        return SynchronizerResponse.ServerError;
                }
            }
            catch
            {
                return SynchronizerResponse.NetworkError;
            }

            if (!String.IsNullOrEmpty(content))
            {
                try
                {
                    var array = JArray.Parse(content);
                    if (array.Count > 0)
                    {
                        foreach (var obj in array)
                        {
                            var json = JObject.FromObject(obj);
                            var planCompletion = JsonSerializer.Deserialize<PlanCompletion>(json.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                            planCompletions.Add(planCompletion);
                        }
                    }
                    else
                        return SynchronizerResponse.EmptyDataError;
                }
                catch
                {
                    return SynchronizerResponse.DataFormatError;
                }
            }
            else
                return SynchronizerResponse.EmptyDataError;

            try
            {
                using (ApplicationContext db = new ApplicationContext(dbPath))
                {
                    db.PlanCompletions.RemoveRange(db.PlanCompletions.Where(x => x.UserId == user_id));
                    db.SaveChanges();
                }

                //TODO: change properties read-write to db boolean value in plancompletion
                List<int> pc_ids = new List<int>();
                foreach (PlanCompletion planCompletion in planCompletions)
                {
                    planCompletion.Id = 0;
                    using (ApplicationContext db = new ApplicationContext(dbPath))
                    {
                        db.PlanCompletions.Add(planCompletion);
                        db.SaveChanges();
                    }
                    pc_ids.Add(planCompletion.Id);
                }

                List<int> synced = new List<int>();
                //TODO: add name constant
                if (App.Current.Properties.TryGetValue("pc_already_sync", out object pc_already_sync))
                {
                    try
                    {
                        synced = JsonSerializer.Deserialize<List<int>>(pc_already_sync.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    }
                    catch
                    {
                        //return SynchronizerResponse.SaveDataError;
                    }
                }
                synced.AddRange(pc_ids);
                App.Current.Properties["pc_already_sync"] = JsonSerializer.Serialize(synced, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return SynchronizerResponse.SaveDataError;
            }

            return SynchronizerResponse.Success;
        }

        public static async Task<SynchronizerResponse> UpdateUser(Uri connection, string dbPath, User user)
        {
            if (user != null)
            {
                Uri uri = new Uri(connection + "/user/" + user.Id);
                StringContent body = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
                string a = await body.ReadAsStringAsync();
                string content = "";

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var response = client.PutAsync(uri, body).Result;
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            content = await response.Content.ReadAsStringAsync();
                        else
                            return SynchronizerResponse.ServerError;
                    }
                }
                catch
                {
                    return SynchronizerResponse.NetworkError;
                }

                if (!String.IsNullOrEmpty(content))
                {
                    try
                    {
                        //var array = JArray.Parse(content);
                        //var json = JObject.FromObject(array[0]);
                        var json = JObject.Parse(content);
                        var u = JsonSerializer.Deserialize<User>(json.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    }
                    catch
                    {
                        return SynchronizerResponse.DataFormatError;
                    }
                }
                else
                    return SynchronizerResponse.EmptyDataError;

                return SynchronizerResponse.Success;
            }
            else
                return SynchronizerResponse.EmptyDataError;
        }

        public static async Task<SynchronizerResponse> RegisterUser(Uri connection, string dbPath, string login, string password)
        {
            Uri uriGet = new Uri(connection + "/user?login=" + login);
            User user = null;
            string content = "";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(uriGet).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        content = await response.Content.ReadAsStringAsync();
                    else
                        return SynchronizerResponse.ServerError;
                }
            }
            catch
            {
                return SynchronizerResponse.NetworkError;
            }

            if (!String.IsNullOrEmpty(content))
            {
                return SynchronizerResponse.UserAlreadyExistError;
            }
            else
            {
                Uri uriPost = new Uri(connection + "/user");
                StringContent body = new StringContent($"{{\"Login\":\"{login}\",\"Password\":\"{password}\"}}", Encoding.UTF8, "application/json");
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var response = client.PostAsync(uriPost, body).Result;
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            content = await response.Content.ReadAsStringAsync();
                        else
                            return SynchronizerResponse.ServerError;
                    }
                }
                catch
                {
                    return SynchronizerResponse.NetworkError;
                }

                if (!String.IsNullOrEmpty(content))
                {
                    try
                    {
                        //var array = JArray.Parse(content);
                        //var json = JObject.FromObject(array[0]);
                        var json = JObject.Parse(content);
                        user = JsonSerializer.Deserialize<User>(json.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    }
                    catch
                    {
                        return SynchronizerResponse.DataFormatError;
                    }
                }
                else
                    return SynchronizerResponse.EmptyDataError;

                if (user != null)
                {
                    try
                    {
                        using (ApplicationContext db = new ApplicationContext(dbPath))
                        {
                            db.Users.Add(user);
                            db.SaveChanges();
                        }
                    }
                    catch
                    {
                        return SynchronizerResponse.SaveDataError;
                    }

                    return SynchronizerResponse.Success;
                }
                else
                    return SynchronizerResponse.EmptyDataError;
            }
        }

        public static async Task<SynchronizerResponse> LoginUser(Uri connection, string dbPath, string login, string password)
        {
            Uri uri = new Uri(connection + "/user" + "?login=" + login);
            User user = null;
            string content = "";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(uri).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        content = await response.Content.ReadAsStringAsync();
                    else
                        return SynchronizerResponse.ServerError;
                }
            }
            catch
            {
                return SynchronizerResponse.NetworkError;
            }

            if (!String.IsNullOrEmpty(content))
            {
                try
                {
                    //var array = JArray.Parse(content);
                    //var json = JObject.FromObject(array[0]);
                    var json = JObject.Parse(content);
                    user = JsonSerializer.Deserialize<User>(json.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    return SynchronizerResponse.DataFormatError;
                }
            }
            else
                return SynchronizerResponse.EmptyDataError;

            if (user != null)
            {
                if (password != user.Password)
                    return SynchronizerResponse.InvalidPasswordError;

                try
                {
                    using (ApplicationContext db = new ApplicationContext(dbPath))
                    {
                        db.Users.Add(user);
                        db.SaveChanges();
                    }
                }
                catch
                {
                    return SynchronizerResponse.SaveDataError;
                }

                SynchronizerResponse response =  await InitPlanCompletions(connection, dbPath, user.Id);
                //TODO: add handler for sync plancompletions
                switch (response)
                {
                    default:
                        return SynchronizerResponse.Success;
                }
            }
            else
                return SynchronizerResponse.EmptyDataError;
        }

        public static async Task<SynchronizerResponse> InitProducts(Uri connection, string dbPath)
        {
            Uri uri = new Uri(connection + "/products");
            List<Product> products = new List<Product>();
            string content = "";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(uri).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        content = await response.Content.ReadAsStringAsync();
                    else
                        return SynchronizerResponse.ServerError;
                }
            }
            catch
            {
                return SynchronizerResponse.NetworkError;
            }

            try
            {
                var array = JArray.Parse(content);
                if (array.Count > 0)
                {
                    foreach (var obj in array)
                    {
                        var json = JObject.FromObject(obj);
                        var product = JsonSerializer.Deserialize<Product>(json.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        products.Add(product);
                    }
                }
                else
                    return SynchronizerResponse.EmptyDataError;
            }
            catch
            {
                return SynchronizerResponse.DataFormatError;
            }

            try
            {
                using (ApplicationContext db = new ApplicationContext(dbPath))
                {
                    db.Products.RemoveRange(db.Products);
                    db.SaveChanges();
                }

                foreach (Product product in products)
                {
                    using (ApplicationContext db = new ApplicationContext(dbPath))
                    {
                        db.Products.Add(product);
                        db.SaveChanges();
                    }
                }
            }
            catch
            {
                return SynchronizerResponse.SaveDataError;
            }

            return SynchronizerResponse.Success;
        }

        public static async Task<SynchronizerResponse> InitPhysicalActivityTypes(Uri connection, string dbPath)
        {
            Uri uri = new Uri(connection + "/types");
            List<PhysicalActivityType> types = new List<PhysicalActivityType>();
            string content = "";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(uri).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        content = await response.Content.ReadAsStringAsync();
                    else
                        return SynchronizerResponse.ServerError;
                }
            }
            catch
            {
                return SynchronizerResponse.NetworkError;
            }

            try
            {
                var array = JArray.Parse(content);
                if (array.Count > 0)
                {
                    foreach (var obj in array)
                    {
                        var json = JObject.FromObject(obj);
                        var type = JsonSerializer.Deserialize<PhysicalActivityType>(json.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        types.Add(type);
                    }
                }
                else
                    return SynchronizerResponse.EmptyDataError;
            }
            catch
            {
                return SynchronizerResponse.DataFormatError;
            }

            try
            {
                using (ApplicationContext db = new ApplicationContext(dbPath))
                {
                    db.PhysicalActivityTypes.RemoveRange(db.PhysicalActivityTypes);
                    db.SaveChanges();
                }

                foreach (PhysicalActivityType type in types)
                {
                    using (ApplicationContext db = new ApplicationContext(dbPath))
                    {
                        db.PhysicalActivityTypes.Add(type);
                        db.SaveChanges();
                    }
                }
            }
            catch
            {
                return SynchronizerResponse.SaveDataError;
            }

            return SynchronizerResponse.Success;
        }

        public static async Task<SynchronizerResponse> InitGenders(Uri connection, string dbPath)
        {
            Uri uri = new Uri(connection + "/genders");
            List<Gender> genders = new List<Gender>();
            string content = "";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(uri).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        content = await response.Content.ReadAsStringAsync();
                    else
                        return SynchronizerResponse.ServerError;
                }
            }
            catch
            {
                return SynchronizerResponse.NetworkError;
            }

            try
            {
                var array = JArray.Parse(content);
                if (array.Count > 0)
                {
                    foreach (var obj in array)
                    {
                        var json = JObject.FromObject(obj);
                        var gender = JsonSerializer.Deserialize<Gender>(json.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        genders.Add(gender);
                    }
                }
                else
                    return SynchronizerResponse.EmptyDataError;
            }
            catch
            {
                return SynchronizerResponse.DataFormatError;
            }

            try
            {
                using (ApplicationContext db = new ApplicationContext(dbPath))
                {
                    db.Genders.RemoveRange(db.Genders);
                    db.SaveChanges();
                }

                foreach (Gender gender in genders)
                {
                    using (ApplicationContext db = new ApplicationContext(dbPath))
                    {
                        db.Genders.Add(gender);
                        db.SaveChanges();
                    }
                }
            }
            catch
            {
                return SynchronizerResponse.SaveDataError;
            }

            return SynchronizerResponse.Success;
        }

        public static async Task<SynchronizerResponse> InitUnits(Uri connection, string dbPath)
        {
            Uri uri = new Uri(connection + "/units");
            List<Unit> units = new List<Unit>();
            string content = "";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(uri).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        content = await response.Content.ReadAsStringAsync();
                    else
                        return SynchronizerResponse.ServerError;
                }
            }
            catch
            {
                return SynchronizerResponse.NetworkError;
            }

            try
            {
                var array = JArray.Parse(content);
                if (array.Count > 0)
                {
                    foreach (var obj in array)
                    {
                        var json = JObject.FromObject(obj);
                        var unit = JsonSerializer.Deserialize<Unit>(json.ToString(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                        units.Add(unit);
                    }
                }
                else
                    return SynchronizerResponse.EmptyDataError;
            }
            catch
            {
                return SynchronizerResponse.DataFormatError;
            }

            try
            {
                using (ApplicationContext db = new ApplicationContext(dbPath))
                {
                    db.Units.RemoveRange(db.Units);
                    db.SaveChanges();
                }

                foreach (Unit unit in units)
                {
                    using (ApplicationContext db = new ApplicationContext(dbPath))
                    {
                        db.Units.Add(unit);
                        db.SaveChanges();
                    }
                }
            }
            catch
            {
                return SynchronizerResponse.SaveDataError;
            }

            return SynchronizerResponse.Success;
        }
    }

    public enum SynchronizerResponse
    {
        None,
        Success,
        NetworkError,
        ServerError,
        DataFormatError,
        EmptyDataError,
        SaveDataError,
        InvalidPasswordError,
        UserAlreadyExistError
    }
}
