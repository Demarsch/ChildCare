using System;
using System.Data;
using System.Data.Sql;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Core;

namespace DataLib
{
    public class ModelContextProvider : IDataContextProvider
    {
        private ModelContext staticModelContext;

        public IDataContext StaticDataContext { get { return staticModelContext ?? (staticModelContext = GetNewDataContext() as ModelContext);} }

        public IDataContext GetNewDataContext()
        {
            if (String.IsNullOrEmpty(defaultConnectionString)) PrepareConnectionString();
            var result = new ModelContext(defaultConnectionString);
#if DEBUG
            result.Database.Log = x => Debug.Write(x);
#endif
            result.Database.CommandTimeout = 300;
            return result;
        }

        private IDataContext GetNewLiteDataContext()
        {
            if (String.IsNullOrEmpty(defaultConnectionString)) PrepareConnectionString();
            var result = new ModelContext(defaultConnectionString);
#if DEBUG
            result.Database.Log = x => Debug.Write(x);
#endif
            result.Configuration.LazyLoadingEnabled = false;
            result.Configuration.ProxyCreationEnabled = false;
            result.Database.CommandTimeout = 300;
            return result;
        }

        private const string ConnectionStringTemplate = "metadata=res://*/ChildCareModel.csdl|res://*/ChildCareModel.ssdl|res://*/ChildCareModel.msl;provider=System.Data.SqlClient;provider connection string=\"data source=@source@;initial catalog=ChildCare;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework\"";

        private static string defaultConnectionString = "";

        private static bool CheckConnection()
        {
            using (var check = new ModelContext(defaultConnectionString))
            {
                try
                {
                    check.Database.Connection.Open();
                    check.Database.Connection.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static void PrepareConnectionString()
        {
            if (File.Exists("connection"))
            {
                defaultConnectionString = File.ReadAllText("connection", Encoding.Default);
                if (CheckConnection())
                    return;
            }
            var localMachine = Environment.MachineName.ToUpper();
            using (var sqlSources = SqlDataSourceEnumerator.Instance.GetDataSources())
            {
                foreach (var source in sqlSources.Rows.Cast<DataRow>().OrderByDescending(x => x["ServerName"].ToString().ToUpper() == localMachine))
                {
                    var servername = source["ServerName"].ToString();
                    var instanceName = source["InstanceName"].ToString();
                    if (!String.IsNullOrEmpty(instanceName))
                        servername += '\\' + source["InstanceName"].ToString();
                    defaultConnectionString = ConnectionStringTemplate.Replace("@source@", servername);
                    if (!CheckConnection())
                        continue;
                    File.WriteAllText("connection", defaultConnectionString, Encoding.Default);
                    return;
                }
            }
        }
    }
}
