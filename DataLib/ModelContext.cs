using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataLib
{
    public partial class ModelContext
    {
        public static string ConnectionStringTemplate = "metadata=res://*/ChildCareModel.csdl|res://*/ChildCareModel.ssdl|res://*/ChildCareModel.msl;provider=System.Data.SqlClient;provider connection string=\"data source=@source@;initial catalog=ChildCare;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework\"";
        public static string DefaultConnectionString = "";

        public ModelContext(string connectionString) : base (connectionString)
        {

        }

        public static ModelContext New
        {
            get
            {
                if (string.IsNullOrEmpty(DefaultConnectionString)) PrepareConnectionString();
                ModelContext result = new ModelContext(DefaultConnectionString);
                result.Database.CommandTimeout = 300;
                return result;
            }
        }

        private static bool CheckConnection()
        {
            using (ModelContext check = new ModelContext(DefaultConnectionString))
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
            if (System.IO.File.Exists("connection"))
            {
                DefaultConnectionString = System.IO.File.ReadAllText("connection", Encoding.Default);
                if (CheckConnection()) return;
            }

            string LocalMachine = System.Environment.MachineName.ToUpper();
            using (DataTable sqlSources = SqlDataSourceEnumerator.Instance.GetDataSources())
            {
                foreach (DataRow source in sqlSources.Rows.Cast<DataRow>().OrderByDescending(x => x["ServerName"].ToString().ToUpper() == LocalMachine))
                {
                    string servername = source["ServerName"].ToString();
                    string instanceName = source["InstanceName"].ToString();
                    if (!string.IsNullOrEmpty(instanceName))
                        servername += '\\' + source["InstanceName"].ToString();
                    DefaultConnectionString = ConnectionStringTemplate.Replace("@source@", servername);
                    if (CheckConnection())
                    {
                        System.IO.File.WriteAllText("connection", DefaultConnectionString, Encoding.Default);
                        return;
                    }
                }
            }
            MessageBox.Show("Не удалось подключиться к базе данных! Сервер не доступен!", "Внимание", MessageBoxButton.OK);
            Application.Current.Shutdown();
        }

    }
}
