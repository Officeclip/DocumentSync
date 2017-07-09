using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSync
{
    public class FolderRepository : IRepository
    {
        public List<Storage> storageList;
        private string folderPath;

        // Read the folder content and load it in the StorageList
        public FolderRepository(string topFolderPath)
        {
            folderPath = topFolderPath;
        }

        /// <summary>
        /// create a folder or file in the folderPath and then add it to the storageList
        /// if lastUpdated is DateTime.MinValue, then first create the folder and then
        /// put the lastUpdated time in the storageList
        /// </summary>
        /// <param name="relativeFolderPath"></param>
        /// <param name="name"></param>
        /// <param name="isFolder"></param>
        public void Create(
                    string relativeFolderPath, 
                    string name, 
                    bool isFolder = false)
        {
            string directoryPath = $@"{folderPath}\{relativeFolderPath}"
                                                .TrimEnd(Path.DirectorySeparatorChar);
            string path = $@"{directoryPath}\{name}";
            if (isFolder)
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                using (StreamWriter w = File.AppendText(path)) {
                    w.WriteLine("Test"); // adding some characters in the file
                }
            }

        }

        // delete the file or folder and do the same in the storageList
        public bool Delete(
                    string relativeFolderPath,
                    string name,
                    bool isFolder = false)
        {
            string directoryPath = $@"{folderPath}\{relativeFolderPath}"
                                                .TrimEnd(Path.DirectorySeparatorChar);
            string path = $@"{directoryPath}\{name}";
            if (isFolder)
            {
                Directory.Delete(relativeFolderPath);
            }
            else
            {
                Console.WriteLine("Repository Not found");
            }

        return true;
        }

        // return true if this storage match the repository (means it is present)
        public bool isMatch(Storage storage)
        {
            throw new NotImplementedException();
        }

        // rename the file or folder
        public void Move(string oldPath, string newPath, bool isFolder = false)
        {
            throw new NotImplementedException();
        }

        // convert the storageList to JSON
        public string ToJSON()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Storage storage in storageList)
            {
                stringBuilder.AppendLine(storage.ToString());
            }
            return stringBuilder.ToString();
        }

        public List<Storage> PopulateStorageList(string relativeFolderPath)
        {
            storageList = new List<Storage>();
            string rootFolderPath = $@"{folderPath}\{relativeFolderPath}";
            if (Directory.Exists(rootFolderPath))
            {
                DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(rootFolderPath);

                PopulateFilesInsideFolder(directoryInfo);

                foreach (DirectoryInfo innerFolderInfo in directoryInfo.GetDirectories())
                {
                    Storage storage = new Storage
                    {
                        IsFolder = true,
                        Name = innerFolderInfo.Name,
                        LastUpdated = innerFolderInfo.LastWriteTime,
                        Action = Constants.Action.None
                    };
                    storageList.Add(storage);
                }

            }
            return storageList;
        }

        private void PopulateFilesInsideFolder(DirectoryInfo directoryInfo)
        {
            IEnumerable<System.IO.FileInfo> allFiles = directoryInfo.GetFiles("*.*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (FileInfo fileInfo in allFiles)
            {
                var storage = new Storage
                {
                    IsFolder = true,
                    Name = fileInfo.Name,
                    LastUpdated = fileInfo.LastWriteTime,
                    Action = Constants.Action.None,
                    Size = fileInfo.Length
                };
                storageList.Add(storage);
            }
        }
    }
}
