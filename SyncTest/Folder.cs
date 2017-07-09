using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using DocumentSyncLib;

namespace SyncTest
{
    [TestFixture]
    [Category("Folder")]
    public class Folder
    {
        [SetUp]
        public void Initialize()
        {
            System.Diagnostics.Process.Start($@"{Utils.GetSetupDir()}\ResetFolder.bat");
        }

        [TearDown]
        public void Cleanup()
        {

        }
        
        [Test]
        public void PopulateStorage()
        {
            FolderRepository folderRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            List<Storage> storageList = 
                    folderRepository.PopulateStorageList("Folder1");
            Assert.AreEqual(2, storageList.Count, "There should only be two elements in the list");
        }

        [Test]
        public void CreateFolderTest()
        {
            FolderRepository folderRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            folderRepository.CreateFolder("Folder1", "Bhanu");

            Storage input = new Storage
            {
                Name = "Bhanu",
                IsFolder = true
            };
            Assert.IsTrue(folderRepository.Exist("Folder1", input), "Folder not in the list");

            //List<Storage> storageList = folderRepository.PopulateStorageList("Folder1");
            //bool isPresent = false;
            //foreach (Storage storage in storageList)
            //{
            //    if ((storage.IsFolder) && (storage.Name == "Dutta"))
            //    {
            //        isPresent = true;
            //        break;
            //    }
            //}
            //Assert.IsTrue(isPresent, "Dutta is not in the list");
        }

        [Test]
        public void CreateFileTest()
        {
            FolderRepository folderRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            Stream fileStream = File.OpenRead($@"{Utils.GetSetupDir()}\UploadFolder\file1.txt");
            folderRepository.CreateFile("Folder1", fileStream, "Bhanu.txt");

              Storage input = new Storage
            {
                Name = "Bhanu.txt",
                IsFolder = false
            };
            Assert.IsTrue(folderRepository.Exist("Folder1", input), "File not in the list");
        }

        [Test]
        public void DeleteFolder()
        {
            FolderRepository folderRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            //folderRepository.Create("Folder1", "Bhanu", true);
            List<Storage> storageList = 
                    folderRepository.PopulateStorageList("Folder1");
            folderRepository.Delete("Folder1", true);

            bool isPresent = true;
            foreach (Storage storage in storageList)
            {
                if((storage.IsFolder) && (storage.Name == "Folder1"))
                {
                    isPresent = false;
                    break;
                }
            }
            Assert.IsTrue(isPresent, "Folder1 is not in the list");

            //Storage input = new Storage
            //{
            //    Name = "Nagesh",
            //    IsFolder = true
            //};

            //Assert.IsTrue(folderRepository.Exist("Folder1", input), "Folder not in the list");
            //Assert.Inconclusive("Not Implemented Yet!");
        }

        [Test]
        public void DeleteFile()
        {
            FolderRepository folderRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            List<Storage> storageList =
                    folderRepository.PopulateStorageList("File1.txt");
            folderRepository.Delete("File1.txt", false);

            bool isPresent = true;
            foreach (Storage storage in storageList)
            {
                if ((storage.IsFolder) && (storage.Name == "File1.txt"))
                {
                    isPresent = false;
                    break;
                }
            }
            Assert.IsTrue(isPresent, "File1.txt is not in the list");
        }
    }
}
