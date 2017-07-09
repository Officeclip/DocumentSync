using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace DocumentSyncLib
{
    /// <summary>
    /// The folder table will be dmgroupfolders and the document table will be dmmasters
    /// </summary>
    public class DbRepository : Repository, IRepository
    {
        private string folderPath;
        private const string baseKey = "Db";

        // Read the folder content and load it in the StorageList
        public DbRepository(string topFolderPath)
        {
            folderPath = topFolderPath;
        }

        public void Create(string relativeFolder, string name, bool isFolder = false)
        {
            string parentPath = $@"{folderPath}/{relativeFolder}".TrimEnd('/');

            int? parentId = GetFolderIdFromRelativeFolder(relativeFolder, isFolder);

            var parentIdString = (parentId == null) ? "NULL" : parentId.ToString();
            string query = (isFolder) ?
                    $@"
                        insert into dmgroupfolders
                            (name, parent_id, parent_path) 
                        VALUES
                            ('{name}', {parentIdString}, '{parentPath}')" :
                    $@"insert into dmmasters(name, folder_id) VALUES ('{name}', {parentId})";
            Utils.ExecuteNonQuery(query, baseKey);
        }

        public void CreateFolder(
                                string relativeFolder,
                                string name,
                                DateTime createdDate)
        {
            string parentPath = $@"{folderPath}/{relativeFolder}".TrimEnd('/');

            int? parentId = GetFolderIdFromRelativeFolder(relativeFolder, true);
            var parentIdString = (parentId == null) ? "NULL" : parentId.ToString();
            string lastUpdated = createdDate.ToString("yyyy-MM-dd HH:mm:ss tt");
            string query =
                    $@"
                        insert into dmgroupfolders
                            (name, parent_id, parent_path, last_updated) 
                        VALUES
                            ('{name}', {parentIdString}, '{parentPath}', '{lastUpdated}')";
            Utils.ExecuteNonQuery(query, baseKey);
        }

        public void CreateFile(
                                string relativeFolder,
                                string name,
                                DateTime createdDate)
        {
            int? folderId = GetFolderIdFromRelativeFolder(relativeFolder, false);
            var folderIdString = (folderId == null) ? "NULL" : folderId.ToString();
            string lastUpdated = createdDate.ToString("yyyy-MM-dd HH:mm:ss tt");
            string query =
                    $@"insert into dmmasters
                            (name, folder_id, last_updated) 
                    VALUES 
                            ('{name}', {folderIdString}, '{lastUpdated}')";
            Utils.ExecuteNonQuery(query, baseKey);
        }

        public void MoveFile(
                        string sourceFolder,
                        string sourceFileName,
                        string destinationFolder,
                        DateTime createdDate)
        {
            MoveOrCopyFile(
                        sourceFolder,
                        sourceFileName,
                        destinationFolder,
                        createdDate,
                        true);
        }

        public void CopyFile(
                        string sourceFolder,
                        string sourceFileName,
                        string destinationFolder,
                        DateTime createdDate)
        {
            MoveOrCopyFile(
                        sourceFolder,
                        sourceFileName,
                        destinationFolder,
                        createdDate,
                        false);
        }

        private void MoveOrCopyFile(
                        string sourceFolder,
                        string sourceFileName,
                        string destinationFolder,
                        DateTime createdDate,
                        bool isMove)
        {
            if (sourceFolder.Trim() == destinationFolder.Trim())
            {
                throw new Exception("Cannot move to the same folder");
            }
            var destinationFolderId = GetFolderIdFromRelativeFolder(destinationFolder);
            if (destinationFolderId == null)
            {
                throw new Exception("Destination Folder is not present");
            }

            var sourceFolderId = GetFolderIdFromRelativeFolder(sourceFolder);
            if (sourceFolderId == null)
            {
                throw new Exception("Source Folder is not present");
            }

            string findDocumentIdQuery =
                    $@"SELECT TOP 1 document_id FROM dmmasters WHERE 
                    name = '{sourceFileName}' AND folder_id = {sourceFolderId}";

            DataSet sourceIdSet = Utils.SqlDataAdapter(findDocumentIdQuery, baseKey);
            int? documentId = (Convert.IsDBNull(sourceIdSet.Tables[0].Rows[0][0]))
                                            ? (int?)null
                                            : (int)sourceIdSet.Tables[0].Rows[0][0];
            if (documentId == null)
            {
                throw new Exception("Source File is not present");
            }

            string lastUpdated = createdDate.ToString("yyyy-MM-dd HH:mm:ss tt");
            string updateOrInsertQuery;
            if (isMove)
            {
                updateOrInsertQuery = $@"
                                    UPDATE dmmasters SET 
                                    folder_id = {destinationFolderId},
                                    last_updated = '{lastUpdated}'
                                    WHERE document_id = {documentId}";
            }
            else
            {
                updateOrInsertQuery = $@"
                                    INSERT INTO dmmasters 
                                    (name, folder_id, last_updated)
                                    VALUES
                                    ('{sourceFileName}', {destinationFolderId}, '{lastUpdated}')";
            }
            Utils.ExecuteNonQuery(updateOrInsertQuery, baseKey);
        }

        public void MoveFolder(
                string sourceFolder,
                string destinationFolder,
                DateTime lastUpdateTime)
        {
            MoveOrCopyFolder(
                    sourceFolder,
                    destinationFolder,
                    lastUpdateTime,
                    true);
        }

        public void CopyFolder(
                string sourceFolder,
                string destinationFolder,
                DateTime lastUpdateTime)
        {
            MoveOrCopyFolder(
                    sourceFolder,
                    destinationFolder,
                    lastUpdateTime,
                    false);
        }

        public void MoveOrCopyFolder(
                        string sourceFolder,
                        string destinationFolder,
                        DateTime lastUpdateTime,
                        bool isMove)
        {
            var destinationFolderId = GetFolderIdFromRelativeFolder(destinationFolder);
            if (destinationFolderId == null)
            {
                throw new Exception("Destination Folder is not present");
            }

            var sourceFolderId = GetFolderIdFromRelativeFolder(sourceFolder);
            if (sourceFolderId == null)
            {
                throw new Exception("Source Folder is not present");
            }

            // Get the destination folder name that will be put into parent path
            if (isMove)
            {
                var folderQuery =
                $@"SELECT name, parent_path FROM dmgroupfolders WHERE folder_id = {destinationFolderId}";
                DataSet folderSet = Utils.SqlDataAdapter(folderQuery, baseKey);
                var folderName = (string)folderSet.Tables[0].Rows[0][0];
                var parent_path = (string)folderSet.Tables[0].Rows[0][1];

                var newParentPath = (parent_path == string.Empty) ? "" : @"{parent_path}/";
                newParentPath += folderName;

                string lastUpdated = lastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss tt");

                string updateOrInsertQuery = $@"UPDATE dmgroupfolders SET 
                                    parent_id = {destinationFolderId},
                                    last_updated = '{lastUpdated}',
                                    parent_path = '{newParentPath}'
                                    WHERE folder_id = {sourceFolderId}";
                Utils.ExecuteNonQuery(updateOrInsertQuery, baseKey);
            }
            else
            {
                Hashtable hashTable = InsertFoldersForCopy((int)sourceFolderId, (int)destinationFolderId);
                InsertDocumentsForCopy((int)sourceFolderId, hashTable);
            }
        }

        public void Rename(string parentPath, string oldName, string newName, bool isFolder = false)
        {
            int? folderId = GetFolderIdFromRelativeFolder(parentPath);
            if (isFolder)
            {
                string query = $@"
                        SELECT count(*) FROM dmgroupfolders WHERE
                        parent_id = {folderId} AND
                        name = '{newName}'";
                DataSet errorCount = Utils.SqlDataAdapter(query, baseKey);
                if ((int)errorCount.Tables[0].Rows[0][0] > 0)
                {
                    throw new Exception("The destination name already exists");
                }
                query = $@"
                        UPDATE dmgroupfolders SET
                        name = '{newName}' WHERE
                        parent_id = {folderId} AND
                        name = '{oldName}'";
                Utils.ExecuteNonQuery(query, baseKey);
            }
            else
            {
                string query = $@"
                        SELECT count(*) FROM dmmasters WHERE
                        folder_id = '{folderId}' AND
                        name = '{newName}'";
                DataSet errorCount = Utils.SqlDataAdapter(query, baseKey);
                if ((int)errorCount.Tables[0].Rows[0][0] > 0)
                {
                    throw new Exception("The destination name already exists");
                }
                query = $@"
                        UPDATE dmmasters SET
                        name = '{newName}' WHERE
                        folder_id = '{folderId}' AND
                        name = '{oldName}'";
                Utils.ExecuteNonQuery(query, baseKey);
            }
        }

        public void Delete(string relativePath, bool isFolder = false)
        {
            string fullPath = $@"{folderPath}\{relativePath}".TrimEnd('\\');
            string[] fullPathArray = fullPath.Split('\\');

            if (isFolder)
            {
                int? folderId = GetFolderIdFromRelativeFolder(relativePath, isFolder);
                if (folderId == null)
                {
                    throw new Exception($"Could not get the folder to delete: {relativePath}");
                }

                var dataSet = GetChildFolders(
                                              folderId);
                StringBuilder stringBuilder = new StringBuilder();
                foreach (DataRow dataRow in dataSet.Tables[0].Rows)
                {
                    stringBuilder.Append($"{(int)dataRow["folder_id"]},");
                }
                stringBuilder.Append(folderId);
                // We now need to delete all the folders, files and subfolders...
                string sqlQuery = $@"                                
                                DELETE FROM dmmasters 
                                WHERE folder_id IN (
                                    SELECT folder_id FROM dmgroupfolders
                                    WHERE folder_id IN ({stringBuilder}))";
                Utils.ExecuteNonQuery(sqlQuery, baseKey);
                sqlQuery = $@"
                                DELETE FROM dmgroupfolders
                                WHERE folder_id IN ({stringBuilder})";
                Utils.ExecuteNonQuery(sqlQuery, baseKey);
            }
            else
            {
                string parentPath = string.Join("/", fullPathArray, 0, fullPathArray.Length - 2);
                string folderName = fullPathArray[fullPathArray.Length - 2];
                string documentname = fullPathArray[fullPathArray.Length - 1];

                string sqlQuery = $@"
                                DELETE FROM dmmasters 
                                WHERE folder_id IN (
                                    SELECT folder_id FROM dmgroupfolders
                                    WHERE parent_path = '{parentPath}'
                                    AND name = '{folderName}')
                                AND name = '{documentname}'";
                Utils.ExecuteNonQuery(sqlQuery, baseKey);
            }
        }

        /// <summary>
        /// Taken From https://www.codeproject.com/Articles/818694/SQL-queries-to-manage-hierarchical-or-parent-child
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        private static DataSet GetChildFolders(
            int? folderId)
        {
            string sqlQuery = @"
                        DECLARE @folderId BIGINT;
                        SET @folderId = {0};
                        WITH tblChild AS
                        (
                            SELECT *
                                FROM dmgroupfolders WHERE parent_id = @folderId
                            UNION ALL
                            SELECT dmgroupfolders.* FROM dmgroupfolders  
                            JOIN tblChild  ON dmgroupfolders.parent_id = tblChild.folder_id
                        )
                        SELECT * from tblChild
                        OPTION(MAXRECURSION 32767)
                        ";
            sqlQuery = string.Format(
                                     sqlQuery,
                                     folderId);
            DataSet dataSet = Utils.SqlDataAdapter(
                                                   sqlQuery, baseKey);
            return dataSet;
        }

        public override List<Storage> PopulateStorageList(string relativeFolder)
        {
            string folderName;
            string parentPath;
            if (relativeFolder == string.Empty)
            {
                folderName = folderPath;
                parentPath = string.Empty;
            }
            else
            {
                var pathparts = relativeFolder.Split(@"\".ToCharArray());
                folderName = pathparts[pathparts.Length - 1];
                var leftPart = string.Join(@"/", pathparts, 0, pathparts.Length - 1);
                parentPath = $@"{folderPath}/{leftPart}".TrimEnd('/');
            }

            string findParentIdQuery =
                    $"SELECT TOP 1 folder_id FROM dmgroupfolders WHERE name='{folderName}' AND parent_path = '{parentPath}'";
            DataSet parentIdSet = Utils.SqlDataAdapter(findParentIdQuery, baseKey);

            int? parentId = (Convert.IsDBNull(parentIdSet.Tables[0].Rows[0][0]))
                                            ? (int?)null
                                            : (int)parentIdSet.Tables[0].Rows[0][0];

            string query =
                (parentId == null) ?
                "SELECT name, parent_path, last_updated FROM dmgroupfolders where parent_id is NULL" :
                $"SELECT name, parent_path, last_updated FROM dmgroupfolders where parent_id = {parentId}";
            DataSet folders = Utils.SqlDataAdapter(query, baseKey);
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
                DataSet files = Utils.SqlDataAdapter(query, baseKey);
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

        private int? GetFolderIdFromRelativeFolder(string relativeFolder)
        {
            string folderName;
            string parentPath;
            if (relativeFolder == string.Empty)
            {
                folderName = folderPath;
                parentPath = string.Empty;
            }
            else
            {
                var pathparts = relativeFolder.Split(@"\".ToCharArray());
                folderName = pathparts[pathparts.Length - 1];
                var leftPart = string.Join(@"/", pathparts, 0, pathparts.Length - 1);
                parentPath = $@"{folderPath}/{leftPart}".TrimEnd('/');
            }

            string query =
                    $@"SELECT TOP 1 
                        folder_id 
                       FROM 
                        dmgroupfolders 
                       WHERE 
                        name='{folderName}' AND parent_path = '{parentPath}'";
            DataSet parentIdSet = Utils.SqlDataAdapter(query, baseKey);

            return (Convert.IsDBNull(parentIdSet.Tables[0].Rows[0][0]))
                                            ? (int?)null
                                            : (int)parentIdSet.Tables[0].Rows[0][0];
        }

        private int? GetFolderIdFromRelativeFolder(string relativeFolder, bool isFolder)
        {
            int? folderId = GetFolderIdFromRelativeFolder(relativeFolder);
            if (
                (folderId == null) &&
                !isFolder)
            {
                throw new Exception($"Trying to add a document without the folder present");
            }
            return folderId;
        }

        /// <summary>
        /// Go through the storage list and apply the same changes here
        /// </summary>
        /// <param name="storageList"></param>
        public void ApplyChanges(
                        string relativeFolder,
                        List<Storage> storageList)
        {
            foreach (Storage storage in storageList)
            {
                switch (storage.Action)
                {
                    case Constants.Action.Create:
                        Create(
                            relativeFolder,
                            storage.Name,
                            storage.IsFolder);
                        break;
                    case Constants.Action.Delete:
                        var fullName = $"{relativeFolder}/{storage.Name}";
                        fullName = fullName.TrimStart('/');
                        if (storage.IsFolder)
                        {
                            Delete(fullName, true);
                        }
                        else
                        {
                            Delete(fullName, false);
                        }
                        break;
                }
            }
        }

        private string GetParentPathFromFolderId(int folderId)
        {
            string query =
                    $@"SELECT 
                        parent_path 
                       FROM 
                        dmgroupfolders 
                       WHERE 
                        folder_id = {folderId}";
            DataSet dataSet = Utils.SqlDataAdapter(query, baseKey);
            return (string)dataSet.Tables[0].Rows[0][0];
        }

        private DataTable GetFolderInfo(int folderId)
        {
            string query =
                    $@"SELECT 
                        * 
                       FROM 
                        dmgroupfolders 
                       WHERE 
                        folder_id = {folderId}";
            return Utils.SqlDataAdapter(query, baseKey).Tables[0];
        }

        private Hashtable InsertFoldersForCopy(int sourceFolderId, int destinationFolderId)
        {
            DataTable dataTable = GetChildFolders(sourceFolderId).Tables[0];
            Hashtable hashTable = new Hashtable();

            DataTable sourceDataTable = GetFolderInfo(sourceFolderId);
            DataTable destinationDataTable = GetFolderInfo(destinationFolderId);

            DataRow newRow = dataTable.NewRow();
            newRow["folder_id"] = sourceFolderId;
            newRow["parent_id"] = sourceDataTable.Rows[0]["parent_id"];
            newRow["parent_path"] = sourceDataTable.Rows[0]["parent_path"];
            dataTable.Rows.InsertAt(newRow, 0);

            // From: http://stackoverflow.com/a/8809409
            var regex = new Regex(Regex.Escape((string)sourceDataTable.Rows[0]["parent_path"]));
            var stringToBeReplaced = string.Format(
                                                    "{0}/{1}",
                                                    (string)destinationDataTable.Rows[0]["parent_path"],
                                                    (string)destinationDataTable.Rows[0]["name"]);


            foreach (DataRow dataRow in dataTable.Rows)
            {
                int folderId = (int)dataRow["folder_id"];
                string insertQuery = $@"
                            INSERT INTO dmgroupfolders
                                (name, parent_id, parent_path, last_updated)
                            SELECT
                                name, parent_id, parent_path, last_updated
                            FROM dmgroupfolders
                            WHERE folder_id = {folderId}";
                int uniqueId = Utils.ExecuteInsertQueryWithReturnId(insertQuery, baseKey);
                hashTable.Add(folderId, uniqueId);
                int parentId = (folderId == sourceFolderId) ? destinationFolderId : (int)hashTable[dataRow["parent_id"]];
                string parentPath = regex.Replace(
                                        (string)dataRow["parent_path"],
                                        stringToBeReplaced);

                string updateQuery = $@"
                            UPDATE dmgroupfolders SET
                                parent_id = {parentId},
                                parent_path = '{parentPath}'
                            WHERE
                                folder_id = {uniqueId}";
                Utils.ExecuteNonQuery(updateQuery, baseKey);
            }
            return hashTable;
        }

        private DataTable GetChildDocumentInfo(int folderId)
        {
            string parentPath = GetParentPathFromFolderId(folderId);
            string query =
                    $@"SELECT 
                        * 
                       FROM 
                        dmmasters 
                       WHERE 
                        folder_id IN (
                            SELECT folder_id FROM dmgroupfolders WHERE
                                parent_path LIKE '{parentPath}%')";
            return Utils.SqlDataAdapter(query, baseKey).Tables[0];
        }

        private void InsertDocumentsForCopy(int sourceFolderId, Hashtable hashTable)
        {
            DataTable dataTable = GetChildDocumentInfo(sourceFolderId);
            foreach (DataRow dataRow in dataTable.Rows)
            {
                int folderId = (int)dataRow["folder_id"];
                string name = (string)dataRow["name"];
                DateTime lastUpdated = (DateTime)dataRow["last_updated"];
                string insertQuery = $@"
                            INSERT INTO dmmasters
                                (name, folder_id, last_updated)
                            VALUES
                                ('{name}', {(int)hashTable[folderId]}, '{lastUpdated}')";
                Utils.SqlDataAdapter(insertQuery, baseKey);
            }
        }
    }
}
