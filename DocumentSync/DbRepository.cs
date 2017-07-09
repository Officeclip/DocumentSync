using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSync
{
    /// <summary>
    /// The folder table will be dmgroupfolders and the document table will be dmmasters
    /// </summary>
    public class DbRepository : IRepository
    {
        public void Create(string relativeFolder, string name, bool isFolder = false)
        {
            int? parentId = GetParentId(relativeFolder, isFolder);
            var parentIdString = (parentId == null) ? "NULL" : parentId.ToString();
            string query = (isFolder) ?
                    $"insert into dmgroupfolders(name, parent, path) VALUES('{name}', {parentIdString}, '{relativeFolder}')" :
                    $"insert into dmmasters(name, folder_id) VALUES ('{name}', {parentId})";
            Utils.ExecuteNonQuery(query);
        }

        public void Move(string oldPath, string newPath, bool isFolder = false)
        {
            throw new NotImplementedException();
        }
        public bool Delete(string relativeFolder, string name, bool isFolder = false)
        {
            throw new NotImplementedException();
        }
        public bool isMatch(Storage storage)
        {
            throw new NotImplementedException();
        }
        public List<Storage> PopulateStorageList(string folder)
        {
            int? parentId = GetParentId(folder, true);
            var parentIdString = (parentId == null) ? "NULL" : parentId.ToString();
            string query =
                (parentId == null) ?
                "SELECT name, parent, last_updated FROM dmgroupfolders where parent is NULL" :
                $"SELECT name, parent, last_updated FROM dmgroupfolders where parent = {parentId}";
            DataSet folders = Utils.SqlDataAdapter(query);
            List<Storage> storageList = new List<Storage>();
            foreach (DataRow dataRow in folders.Tables[0].Rows)
            {
                Storage storage = new Storage
                {
                    IsFolder = true,
                    LastUpdated = Convert.ToDateTime(dataRow["last_updated"]),
                    Name = (string)dataRow["name"]
                };
                storageList.Add(storage);
            }
            if (parentId != null)
            {
                query = $"SELECT name, last_updated FROM dmmasters WHERE folder_id = {parentId}";
                DataSet files = Utils.SqlDataAdapter(query);
                foreach (DataRow dataRow in files.Tables[0].Rows)
                {
                    Storage storage = new Storage
                    {
                        IsFolder = false,
                        LastUpdated = Convert.ToDateTime(dataRow["last_updated"]),
                        Name = (string)dataRow["name"]
                    };
                    storageList.Add(storage);
                }
            }
            return storageList;
        }

        private int? GetParentId(string relativePath, bool isFolder)
        {
            int? parentId = null;
            string query = $@"select TOP 1 parent FROM dmgroupfolders where path = '{relativePath}'";

            DataSet output = Utils.SqlDataAdapter(query);
            if (output.Tables[0].Rows.Count > 0)
            {
                var parentIdObject = output.Tables[0].Rows[0][0];
                parentId = (Convert.IsDBNull(parentIdObject)) ?
                                    null :
                                    (int?)Convert.ToInt32(output.Tables[0].Rows[0][0]);
            }
            if (parentId == null)
            {
                if (!isFolder)
                {
                    throw new Exception($"Trying to add a document without the folder present");
                }
                else if (!string.IsNullOrEmpty(relativePath))
                {
                    throw new Exception($"Folder cannot be added as the {relativePath} is not present");
                }
            }
            return parentId;
        }
    }
}
