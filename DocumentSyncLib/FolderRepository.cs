using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSyncLib
{
    public class FolderRepository : Repository, IRepository
    {
        public List<Storage> storageList;
        public string FolderPath { get; set; }

        // Read the folder content and load it in the StorageList
        public FolderRepository(string topFolderPath)
        {
            FolderPath = topFolderPath;
        }

        /// <summary>
        /// create a folder or file in the FolderPath and then add it to the storageList
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
            string directoryPath = $@"{FolderPath}\{relativeFolderPath}"
                                                .TrimEnd(Path.DirectorySeparatorChar)
                                                .Replace('/', '\\');
            string path = Path.Combine(directoryPath, name);
            if (isFolder)
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                File.WriteAllText(path, "Test");
            }
        }

        public DateTime CreateFolder(
                    string relativeFolderPath,
                    string name)
        {
            string directoryPath = $@"{FolderPath}\{relativeFolderPath}"
                                                .TrimEnd(Path.DirectorySeparatorChar)
                                                .Replace('/', '\\');
            string path = Path.Combine(directoryPath, name);
            if (Directory.Exists(path))
            {
                throw new Exception($"Folder {path} exists");
            }
            DirectoryInfo directoryInfo = Directory.CreateDirectory(path);

            return directoryInfo.LastWriteTime;
        }

        public DateTime CreateFile(
                    string relativeFolderPath,
                    Stream stream,
                    string name)
        {
            string directoryPath = $@"{FolderPath}\{relativeFolderPath}"
                                                .TrimEnd(Path.DirectorySeparatorChar)
                                                .Replace('/', '\\');
            string path = Path.Combine(directoryPath, name);
            if (Directory.Exists(path))
            {
                throw new Exception($"File {path} exists");
            }
            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
            fileStream.Dispose();
            return File.GetLastWriteTime(path);
        }

        // delete the file or folder and do the same in the storageList
        public void Delete(string relativePath, bool isFolder = false)
        {
            string path = $@"{FolderPath}\{relativePath}"
                                                .TrimEnd(Path.DirectorySeparatorChar)
                                                .Replace('/', '\\');
            if (isFolder)
            {
                Directory.Delete(path, true);
            }
            else
            {
                File.Delete(path);
            }
        }

        public void Rename(string relativeFolderPath, string oldName, string newName, bool isFolder = false)
        {
            string path = $@"{FolderPath}\{relativeFolderPath}"
                                                .TrimEnd(Path.DirectorySeparatorChar)
                                                .Replace('/', '\\');
            string oldPath = $@"{path}\{oldName}";
            string newPath = $@"{path}\{newName}";

            if (isFolder)
            {
                Directory.Move(oldPath, newPath);
            }
            else
            {
                File.Move(oldPath, newPath);
            }
        }

        public DateTime MoveFile(string oldFolderPath, string fileName, string newFolderPath)
        {
            return MoveOrCopyFile(
                            oldFolderPath,
                            fileName,
                            newFolderPath,
                            true);
        }

        public DateTime MoveFolder(string oldFolderPath, string newFolderPath)
        {
            return MoveOrCopyFolder(
                                oldFolderPath,
                                newFolderPath,
                                true);
        }

        public DateTime CopyFile(string oldFolderPath, string fileName, string newFolderPath)
        {
            return MoveOrCopyFile(
                            oldFolderPath,
                            fileName,
                            newFolderPath,
                            false);
        }

        private DateTime MoveOrCopyFile(string oldFolderPath, string fileName, string newFolderPath, bool isMove)
        {
            if (oldFolderPath.Trim() == newFolderPath.Trim())
            {
                throw new Exception("Cannot move to the same folder");
            }
            string sourceFolder = $@"{FolderPath}\{oldFolderPath}"
                                                .TrimEnd(Path.DirectorySeparatorChar)
                                                .Replace('/', '\\');
            var sourcePath = $@"{sourceFolder}\{fileName}";

            string destinationFolderPath = $@"{FolderPath}\{newFolderPath}"
                                                .TrimEnd(Path.DirectorySeparatorChar)
                                                .Replace('/', '\\');
            string destinationPath = $@"{destinationFolderPath}\{fileName}";

            if (!File.Exists(sourcePath))
            {
                throw new Exception($"Folder {sourcePath} does not exists");
            }
            if (File.Exists(destinationPath))
            {
                throw new Exception($"Folder {destinationPath} exists");
            }

            if (isMove) {
                File.Move(sourcePath, destinationPath);
            }
            else
            {
                File.Copy(sourcePath, destinationPath);
            }
            return File.GetLastWriteTime(destinationPath);
        }

        public DateTime CopyFolder(string oldFolderPath, string newFolderPath)
        {
            return MoveOrCopyFolder(
                                oldFolderPath,
                                newFolderPath,
                                false);
        }

        private DateTime MoveOrCopyFolder(string oldFolderPath, string newFolderPath, bool isMove)
        {
            string sourceFolderPath = $@"{FolderPath}\{oldFolderPath}"
                                                .TrimEnd(Path.DirectorySeparatorChar)
                                                .Replace('/', '\\');


            string destinationFolderPath = $@"{FolderPath}\{newFolderPath}"
                                                .TrimEnd(Path.DirectorySeparatorChar)
                                                .Replace('/', '\\');

            destinationFolderPath += $@"\{new DirectoryInfo(sourceFolderPath).Name}";

            if (!Directory.Exists(sourceFolderPath))
            {
                throw new Exception($"Folder {sourceFolderPath} does not exists");
            }
            if (Directory.Exists(destinationFolderPath))
            {
                throw new Exception($"Folder {destinationFolderPath} exists");
            }
            if (destinationFolderPath.IndexOf(sourceFolderPath) >= 0)
            {
                throw new Exception("The destination folder is subfolder of the source folder");
            }

            if (isMove)
            {
                Directory.Move(sourceFolderPath, destinationFolderPath);
            }
            else
            {
                // Got from http://stackoverflow.com/a/3822913
                //Create all of the directories
                Directory.CreateDirectory(destinationFolderPath);
                foreach (string dirPath in Directory.GetDirectories(sourceFolderPath, "*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(sourceFolderPath, destinationFolderPath));

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(sourceFolderPath, "*.*",
                    SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(sourceFolderPath, destinationFolderPath), true);
            }
            return Directory.GetLastWriteTime(destinationFolderPath);
        }

        /// convert the storageList to JSON
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

        public override List<Storage> PopulateStorageList(string relativeFolderPath)
        {
            storageList = new List<Storage>();
            string rootFolderPath = $@"{FolderPath}\{relativeFolderPath}"
                                                .TrimEnd(Path.DirectorySeparatorChar)
                                                .Replace('/', '\\');

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
                    IsFolder = false,
                    Name = fileInfo.Name,
                    LastUpdated = fileInfo.LastWriteTime,
                    Action = Constants.Action.None,
                    Size = fileInfo.Length
                };
                storageList.Add(storage);
            }
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
    }
}
