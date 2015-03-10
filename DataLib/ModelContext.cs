using System.Data;
using System.Data.Sql;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace DataLib
{
    public partial class ModelContext
    {
        #region Context

        public ModelContext(string connectionString)
            : base(connectionString)
        {

        }

        /// <summary>
        /// Создает и возвращает новый контекст
        /// </summary>
        public static ModelContext New
        {
            get
            {
                if (string.IsNullOrEmpty(DefaultConnectionString)) PrepareConnectionString();
                var result = new ModelContext(DefaultConnectionString);
#if DEBUG
                result.Database.Log = x => Debug.Write(x);
#endif
                result.Database.CommandTimeout = 300;
                return result;
            }
        }

        /// <summary>
        /// Возвращает контекст, созданный один раз (для неизменяемых справочников)
        /// </summary>
        private static ModelContext staticContext = null;

        public static ModelContext Static
        {
            get
            {
                return staticContext ?? (staticContext = New);
            }
        }

        #endregion

        #region connection management

        private static string ConnectionStringTemplate = "metadata=res://*/ChildCareModel.csdl|res://*/ChildCareModel.ssdl|res://*/ChildCareModel.msl;provider=System.Data.SqlClient;provider connection string=\"data source=@source@;initial catalog=ChildCare;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework\"";
        private static string DefaultConnectionString = "";

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

        #endregion
    }
}
