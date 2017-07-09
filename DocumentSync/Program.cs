using DocumentSyncLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSync
{
    public class Program
    {
        static string connectionString;
        //= System.Configuration.ConfigurationManager.ConnectionStrings["Test"].ConnectionString;
        static void Main(string[] args)
        {
            connectionString = Utils.GetConnectionString("Db");
            Initialize();
            Test100();
        }

        public static void Initialize()
        {
            using (var p = Process.Start($@"{Utils.GetSetupDir()}\ResetFolder.bat"))
            {
                p.WaitForExit();
                p.Close();
            }
            //string sqlString = File.ReadAllText($@"{Utils.GetSetupDir()}\ResetFolder.sql");
            //Utils.ExecuteNonQuery(sqlString, "Db");
        }

        static void Test91()
        {
            //FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            //DateTime updatedDate = uploadRepository.MoveFile("File1.txt", @"Folder1");
        }

        static void Test92()
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            DateTime updatedDate = uploadRepository.MoveFolder(@"Folder1\Folder2", "");
        }

        static void Test81()
        {
            FolderRepository uploadRepository = new FolderRepository("UploadFolder");
            DateTime createdTime = uploadRepository.CreateFolder("Folder1", "Dutta");
            DbRepository dbRepository = new DbRepository(
                                                    "DbFolder");
            dbRepository.CreateFolder("Folder1", "Dutta", createdTime);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "Folder1");
        }

        static void Test82()
        {
            FolderRepository uploadRepository = new FolderRepository("UploadFolder");
            Stream fileStream = File.OpenRead(@"UploadFolder\file1.txt");
            DateTime createdTime = uploadRepository.CreateFile("Folder1", fileStream, "Dutta.txt");
            DbRepository dbRepository = new DbRepository(
                                                    "DbFolder");
            dbRepository.CreateFile("Folder1", "Dutta.txt", createdTime);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "Folder1");
        }

        static void Test71()
        {
            DbRepository dbRepository = new DbRepository(
                                                    "DbFolder");
            dbRepository.Delete("Folder1/file2.txt", false);
        }

        static void Test7()
        {
            DbRepository dbRepository = new DbRepository(
                                                    "DbFolder");
            dbRepository.Delete("Folder1", true);
        }

        static void Test61()
        {
            FolderRepository folderRepository = new FolderRepository("DbFolder");
            folderRepository.Delete(@"Folder1\file2.txt", false);
        }

        static void Test6()
        {
            FolderRepository folderRepository = new FolderRepository("DbFolder");
            folderRepository.Delete("Folder1", true);
        }

        static void Test5()
        {
            DbRepository dbRepository = new DbRepository(
                                                    "DbFolder");
            List<Storage> storageList = dbRepository.PopulateStorageList(string.Empty);
        }

        static void Test4()
        {
            DbRepository dbRepository = new DbRepository(
                                                    "DbFolder");
            dbRepository.Create("Folder1", "Folder11", true);
        }

        static void Test3()
        {
            DbRepository dbRepository = new DbRepository(
                                                    "DbFolder");
            dbRepository.Create("", "XXX", true);
        }

        static void Test12()
        {
            FolderRepository uploadRepository = new FolderRepository("UploadFolder");
            uploadRepository.Create(string.Empty, "XXX", true);
        }

        static void Test11()
        {
            FolderRepository uploadRepository = new FolderRepository("UploadFolder");
            uploadRepository.Create(string.Empty, "XXX");
        }

        static void Test1()
        {
            FolderRepository dbRepository = new FolderRepository("DbFolder");
            FolderRepository uploadRepository = new FolderRepository("UploadFolder");
            uploadRepository.Create(string.Empty, "XXX");
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, string.Empty);
        }

        /// <summary>
        /// Recursive sync of two folders
        /// </summary>
        static void Test100()
        {
            FolderRepository dbRepository = new FolderRepository("DbFolder");
            FolderRepository uploadRepository = new FolderRepository("UploadFolder");
            // We will make changes to the upload repository and sync with the DbRepository
            // At the top level
            uploadRepository.Create("", "Top.new.file.txt");
            uploadRepository.Create("", "Top.new.folder", true);
            // At the folder1 level
            uploadRepository.Create("Folder1", "Folder1.new.file.txt");
            uploadRepository.Create("Folder1", "Folder1.new.folder", true);
            // At the folder2 level
            uploadRepository.Delete(@"Folder1\Folder2", true);
            // At the Folder1.new.folder level
            uploadRepository.Create("Top.new.folder", "File.inside.folder1.txt");
            //Now find the difference between the two repository
            var root = new DirectoryInfo(uploadRepository.FolderPath);
            var directories = root.GetDirectories("*.*", System.IO.SearchOption.AllDirectories);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, string.Empty);
            dbRepository.ApplyChanges(string.Empty, difference);
            foreach (var directoryInfo in directories)
            {
                string folder = directoryInfo.FullName.Substring(root.FullName.Length + 1);
                difference = Utils.Compare(dbRepository, uploadRepository, folder);
                dbRepository.ApplyChanges(folder, difference);
            }
        }
    }
}
