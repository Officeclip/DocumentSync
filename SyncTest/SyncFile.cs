using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using DocumentSyncLib;
using System.IO;
using System.Diagnostics;

namespace SyncTest
{
    [TestFixture]
    [Category("SyncFile")]
    public class SyncFile
    {
        [SetUp]
        public void Initialize()
        {
            using (var p = Process.Start($@"{Utils.GetSetupDir()}\ResetFolder.bat"))
            {
                p.WaitForExit();
                p.Close();
            }
        }

        [TearDown]
        public void Cleanup()
        {

        }

        [Test]
        public void ShowDifferenceCreateFile()
        {
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Create("Folder1", "XXX.txt", false);
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, "Folder1");
            Storage storage = output[0];
            Assert.AreEqual(false, storage.IsFolder, "Incorrect type");
            Assert.AreEqual("XXX.txt", storage.Name, "Incorrect Name");
            Assert.AreEqual(Constants.Action.Create, storage.Action, "Incorrect Action");
        }

        [Test]
        public void ShowDifferencecreateFolder()
        {
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Create("Folder1", "XXX", true);
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, "Folder1");
            Storage storage = output[0];
            Assert.AreEqual(true, storage.IsFolder, "Incorrect type");
            Assert.AreEqual("XXX", storage.Name, "Incorrect Name");
            Assert.AreEqual(Constants.Action.Create, storage.Action, "Incorrect Action");
        }

        [Test]
        public void ShowDifferenceDeleteFile()
        {
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Delete("File1.txt", false);
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, "");
            Storage storage = output[0];
            Assert.AreEqual(false, storage.IsFolder, "Incorrect type");
            Assert.AreEqual("File1.txt", storage.Name, "Incorrect Name");
            Assert.AreEqual(Constants.Action.Delete, storage.Action, "Incorrect Action");
        }

        [Test]
        public void ShowDifferenceDeleteFolder()
        {
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Delete(@"Folder1\Folder2", true);
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, "Folder1");
            Storage storage = output[0];
            Assert.AreEqual(true, storage.IsFolder, "Incorrect type");
            Assert.AreEqual("Folder2", storage.Name, "Incorrect Name");
            Assert.AreEqual(Constants.Action.Delete, storage.Action, "Incorrect Action");
        }

        [Test]
        public void ApplyDifferenceCreateFile()
        {
            string fileToAdd = "Bhanu.txt";
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            FolderRepository uploadRepository = new FolderRepository(
                                        $@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Create("Folder1", fileToAdd, false);
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, "Folder1");
            Storage storage = output[0];
            dbRepository.ApplyChanges(
                                "Folder1",
                                output);
            List<Storage> dbStorageList = dbRepository.PopulateStorageList(
                                                            "Folder1");
            bool isFound = false;
            foreach (Storage myStorage in dbStorageList)
            {
                if (myStorage.Name == fileToAdd)
                {
                    Assert.AreEqual(false, myStorage.IsFolder, "Incorrect type");
                    isFound = true;
                }
            }
            Assert.IsTrue(isFound, $"{fileToAdd} cannot be found in the db");
        }

        [Test]
        public void ApplyDifferenceCreateFolder()
        {
            string folderToAdd = "Nagesh";
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            FolderRepository uploadRepository = new FolderRepository(
                                        $@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Create("Folder1", folderToAdd, true);
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, "Folder1");
            Storage storage = output[0];
            dbRepository.ApplyChanges(
                                "Folder1",
                                output);
            List<Storage> dbStorageList = dbRepository.PopulateStorageList(
                                                            "Folder1");
            bool isFound = false;
            foreach (Storage myStorage in dbStorageList)
            {
                if (myStorage.Name == folderToAdd)
                {                    
                    isFound = true;
                    break;
                }
            }
            if (!isFound)
            {
                Assert.Fail($"{folderToAdd} was supposed to be present but not found");
            }
        }

        [Test]
        public void ApplyDifferenceDeleteFile()
        {
            string fileToDelete = "File2.txt";
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            FolderRepository uploadRepository = new FolderRepository(
                                        $@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Delete(@"Folder1\File2.txt", false);
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, "Folder1");
            Storage storage = output[0];
            dbRepository.ApplyChanges(
                                "Folder1",
                                output);
            List<Storage> dbStorageList = dbRepository.PopulateStorageList(
                                                            "Folder1");
            foreach (Storage myStorage in dbStorageList)
            {
                if (myStorage.Name == fileToDelete)
                {
                    Assert.Fail($"The file {fileToDelete} should be deleted but it is still present");
                }
            }
        }

        [Test]
        public void ApplyDifferenceDeleteFolder()
        {
            string folderToDelete = "Folder2";
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            FolderRepository uploadRepository = new FolderRepository(
                                        $@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Delete($@"Folder1\{folderToDelete}", true);
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, "Folder1");
            Storage storage = output[0];
            dbRepository.ApplyChanges(
                                "Folder1",
                                output);
            List<Storage> dbStorageList = dbRepository.PopulateStorageList(
                                                            "Folder1");
            foreach (Storage myStorage in dbStorageList)
            {
                if (myStorage.Name == folderToDelete)
                {
                    Assert.Fail($"{folderToDelete} should not show up as it should be deleted");
                }
            }
        }

        [Test]
        public void ApplyDifferenceCreateFileTopFolder()
        {
            string fileToAdd = "Bhanu.txt";
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            FolderRepository uploadRepository = new FolderRepository(
                                        $@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Create("", fileToAdd, false);
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, "");
            Storage storage = output[0];
            dbRepository.ApplyChanges(
                                "",
                                output);
            List<Storage> dbStorageList = dbRepository.PopulateStorageList(
                                                            "");
            bool isFound = false;
            foreach (Storage myStorage in dbStorageList)
            {
                if (myStorage.Name == fileToAdd)
                {
                    Assert.AreEqual(false, myStorage.IsFolder, "Incorrect type");
                    isFound = true;
                }
            }
            Assert.IsTrue(isFound, $"{fileToAdd} cannot be found in the db");
        }

        [Test]
        public void ApplyDifferenceCreateFolderTopFolder()
        {
            string folderToAdd = "Nagesh";
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            FolderRepository uploadRepository = new FolderRepository(
                                        $@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Create("", folderToAdd, true);
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, "");
            Storage storage = output[0];
            dbRepository.ApplyChanges(
                                "",
                                output);
            List<Storage> dbStorageList = dbRepository.PopulateStorageList(
                                                            "");
            bool isFound = false;
            foreach (Storage myStorage in dbStorageList)
            {
                if (myStorage.Name == folderToAdd)
                {
                    isFound = true;
                    break;
                }
            }
            if (!isFound)
            {
                Assert.Fail($"{folderToAdd} was supposed to be present but not found");
            }
        }

        [Test]
        public void ApplyDifferenceDeleteFileTopFolder()
        {
            //ApplyDifferenceCreateFileTopFolder();
            string fileToDelete = "File1.txt";
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            FolderRepository uploadRepository = new FolderRepository(
                                        $@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Delete(fileToDelete, false);
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, "");
            Storage storage = output[0];
            dbRepository.ApplyChanges(
                                "",
                                output);
            List<Storage> dbStorageList = dbRepository.PopulateStorageList(
                                                            "");
            foreach (Storage myStorage in dbStorageList)
            {
                if (myStorage.Name == fileToDelete)
                {
                    Assert.Fail($"The file {fileToDelete} should be deleted but it is still present");
                }
            }
        }

        [Test]
        public void ApplyDifferenceDeleteFolderTopFolder()
        {
            ApplyDifferenceCreateFolderTopFolder();
            string folderToDelete = "Nagesh";
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            FolderRepository uploadRepository = new FolderRepository(
                                        $@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Delete($@"{folderToDelete}", true);
            List<Storage> output = Utils.Compare(dbRepository, uploadRepository, "");
            Storage storage = output[0];
            dbRepository.ApplyChanges(
                                "",
                                output);
            List<Storage> dbStorageList = dbRepository.PopulateStorageList(
                                                            "");
            foreach (Storage myStorage in dbStorageList)
            {
                if (myStorage.Name == folderToDelete)
                {
                    Assert.Fail($"{folderToDelete} should not show up as it should be deleted");
                }
            }
        }

        [Test]
        public void CreateFile()
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            Stream fileStream = File.OpenRead($@"{Utils.GetSetupDir()}\UploadFolder\file1.txt");
            DateTime createdTime = uploadRepository.CreateFile("Folder1", fileStream, "Dutta.txt");
            FolderRepository dbRepository = new FolderRepository($@"{Utils.GetSetupDir()}\DbFolder");
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "Folder1");
        }
    }
}
