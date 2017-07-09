using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSyncLib
{
    public class Utils
    {
        /// <summary>
        /// Compares two repositories
        /// </summary>
        /// <param name="dbRepository"></param>
        /// <param name="uploadRepository"></param>
        /// <param name="folder"></param>
        /// <returns>The list that should be added to the dbRepository to
        /// make it equal to the uploadRepository</returns>
        public static List<Storage> Compare(
                                        IRepository dbRepository,
                                        IRepository uploadRepository,
                                        string folder)
        {
            List<Storage> dbStorage = dbRepository.PopulateStorageList(folder);
            List<Storage> uploadStorage = uploadRepository.PopulateStorageList(folder);

            List<Storage> differenceList = new List<Storage>();
            foreach (Storage storageDb in dbStorage)
            {
                if (!storageDb.IsPresent(uploadStorage))
                {
                    storageDb.Action = Constants.Action.Delete;
                    differenceList.Add(storageDb);
                    continue;
                }
                Storage modifiedStorage = storageDb.FindModified(uploadStorage);
                if (modifiedStorage != null)
                {
                    modifiedStorage.Action = Constants.Action.Update;
                    differenceList.Add(modifiedStorage);
                }
            }
            foreach (Storage storageUpload in uploadStorage)
            {
                if (!storageUpload.IsPresent(dbStorage))
                {
                    {
                        storageUpload.Action = Constants.Action.Create;
                        differenceList.Add(storageUpload);
                        continue;
                    }
                }
            }
            return differenceList;
        }

        public static void ExecuteNonQuery(string query, string baseKey)
        {
            string connectionString = GetConnectionString(baseKey);

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    cn.Close();
                }
            }
        }

        public static DataSet SqlDataAdapter(
                                    string query,
                                    string baseKey)
        {
            DataSet returnValue;
            string connectionString = GetConnectionString(baseKey);
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, connectionString);

                returnValue = new DataSet();
                adapter.Fill(returnValue, "Output");
            }
            return returnValue;
        }

        public static int ExecuteInsertQueryWithReturnId(
                                                    string query,
                                                    string baseKey)
        {
            query += $";SELECT CAST(scope_identity() AS int)";
            string connectionString = GetConnectionString(baseKey);
            int uniqueId = 0;
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    cn.Open();
                    uniqueId = (int)cmd.ExecuteScalar();
                    cn.Close();
                }
            }
            return uniqueId;
        }

        public static string GetConnectionString(string baseKey)
        {
            string connString = ConfigurationManager.AppSettings[GetConfigKey(baseKey)];
            return connString;
        }

        private static string GetConfigKey(string baseKey)
        {
            if (Dns.GetHostName().StartsWith("skd-laptop") || 
                Dns.GetHostName().StartsWith("Bharath_PC") || 
                Dns.GetHostName().StartsWith("DESKTOP-K1V2OOO") ||
                Dns.GetHostName().StartsWith("DESKTOP-0K7VTCD"))
             {
                return $"{baseKey}.{Dns.GetHostName()}";
            }

            throw new Exception("Connection String could not be determined, put appropriate db key in apps.config");
        }

        public static string GetSetupDir()
        {
            System.Reflection.Assembly Asm = System.Reflection.Assembly.GetExecutingAssembly();
            return Path.GetDirectoryName(Asm.Location);
        }


        public static void ExecuteStorageCommands(IRepository repository, List<Storage> storageList)
        {

        }
    }
}
