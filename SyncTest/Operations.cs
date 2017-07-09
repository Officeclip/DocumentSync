using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using DocumentSyncLib;
using System.IO;
using System.Diagnostics;

namespace DocumentSyncLib
{
    [TestFixture]
    [Category("Operations")]
    public class Operations
    {
        [SetUp]
        public void Initialize()
        {
            using (var p = Process.Start($@"{Utils.GetSetupDir()}\ResetFolder.bat"))
            {
                p.WaitForExit();
                p.Close();
            }
            string sqlString = File.ReadAllText($@"{Utils.GetSetupDir()}\ResetFolder.sql");
            Utils.ExecuteNonQuery(sqlString, "Db");
        }

        [TearDown]
        public void Cleanup()
        {

        }

        [Test]
        public void CreateFileInner()   // Creates a New file inside the Folder1
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            Stream fileStream = File.OpenRead($@"{Utils.GetSetupDir()}\UploadFolder\file1.txt");
            DateTime createdTime = uploadRepository.CreateFile("Folder1", fileStream, "Dutta.txt");
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.CreateFile("Folder1", "Dutta.txt", createdTime);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "Folder1");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void CreateFileTopLevel()  // Creates New file at the Top Level
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            Stream fileStream = File.OpenRead($@"{Utils.GetSetupDir()}\UploadFolder\File1.txt");
            DateTime createdTime = uploadRepository.CreateFile("", fileStream, "Dutta.txt");
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.CreateFile("", "Dutta.txt", createdTime);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void CreateFolderInner()  // Creates New Folder inside the Folder1
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            DateTime createdTime = uploadRepository.CreateFolder("Folder1", "Dutta");
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.CreateFolder("Folder1", "Dutta", createdTime);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "Folder1");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void CreateFolderTopLevel() // Creates New Folder at the Top Level
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            DateTime createdTime = uploadRepository.CreateFolder("", "Dutta");
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.CreateFolder("", "Dutta", createdTime);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void DeleteFileTopLevel()  // Deletes existing file at the Top Level
        {            
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Delete("File1.txt", false);
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.Delete("File1.txt", false);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void DeleteFileInner()  // Deletes existing File inside the Folder1
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Delete(@"Folder1\File2.txt", false);
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.Delete(@"Folder1\File2.txt", false);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void DeleteNewFileTopLevel()  // Deletes newly created file at the Top Level
        {
            CreateFileTopLevel();
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Delete(@"Dutta.txt", false);
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.Delete(@"Dutta.txt", false);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void DeleteFolderTopLevel()  // Deletes Top Level folder (i.e Folder1) and its content
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Delete("Folder1", true);
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.Delete("Folder1", true);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void DeleteFolderInner()  // Delete's Folder2 and its content inside Folder1
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Delete(@"Folder1\Folder2", true);
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.Delete(@"Folder1\Folder2", true);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void DeleteNewFolderTopLevel()  // Delete newly created folder at the Top level
        {
            CreateFolderTopLevel();
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Delete(@"Dutta", true);
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.Delete(@"Dutta", true);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void MoveFileInner()      // Move file from Top Level to Folder2
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            DateTime createdDate = uploadRepository.MoveFile(
                                                        "",
                                                        "File1.txt",
                                                        @"Folder1\Folder2");
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.MoveFile(
                            "",
                            "File1.txt",
                            @"Folder1\Folder2",
                            createdDate);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");

            difference = Utils.Compare(dbRepository, uploadRepository, @"Folder1\Folder2");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void MoveFileOuter()   // Move file from Folder2 to Top level
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            DateTime createdDate = uploadRepository.MoveFile(
                                                        @"Folder1\Folder2",
                                                        "File3.txt",
                                                        "");
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.MoveFile(
                            @"Folder1\Folder2",
                            "File3.txt",
                            "",
                            createdDate);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");

            difference = Utils.Compare(dbRepository, uploadRepository, @"Folder1\Folder2");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void MoveNewFolderInner()   // Move Newly created Folder from Top Level to the Folder2 
        {
            CreateFolderTopLevel();
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            DateTime createdDate = uploadRepository.MoveFolder(
                                                         "Dutta",
                                                         @"Folder1\Folder2");
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.MoveFolder(
                            "Dutta",
                            @"Folder1\Folder2",
                            createdDate);

            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void MoveFolderOuter()   // Move Folder2 and its content to the Top Level 
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            DateTime createdDate = uploadRepository.MoveFolder(
                                                         @"Folder1\Folder2",
                                                         "");
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.MoveFolder(
                            @"Folder1\Folder2",
                            "",
                            createdDate);

            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void RenameFileInner()  // Rename Inner file(i.e File3) to Dutta.txt
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Rename(@"Folder1\Folder2", "File3.txt", "Dutta.txt", false);
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.Rename(@"Folder1\Folder2", "File3.txt", "Dutta.txt", false);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void RenameFileOuter()  // Rename Top Level file(i.e File1) to File2
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Rename("", "File1.txt", "File2.txt", false);
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.Rename("", "File1.txt", "File2.txt", false);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void RenameFolderInner()  // Rename Inside Folder(i.e Folder2) to Dutta.
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Rename(@"Folder1", "Folder2", "Dutta", true);
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.Rename(@"Folder1", "Folder2", "Dutta", true);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }


        [Test]
        public void RenameFolderOuter()  // Rename Top Level Folder(i.e Folder1) to Folder3
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            uploadRepository.Rename("", "Folder1", "Folder3", true);
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.Rename("", "Folder1", "Folder3", true);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void CopyFileInner() // Copy File From Top Level (i.e File1.txt) to Folder1
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            DateTime createdTime = uploadRepository.CopyFile("", "File1.txt", "Folder1");
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.CopyFile("", "File1.txt", "Folder1", createdTime);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void CopyFileOuter() // Copy File From inside Folder2 (i.e File3.txt) to Top Level Folder
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            DateTime createdTime = uploadRepository.CopyFile(@"Folder1\Folder2", "File3.txt", "");
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.CopyFile(@"Folder1\Folder2", "File3.txt", "", createdTime);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void CopyFolderInner()  // Copy Top Level Folder (i.e Folder1) and its content to Newly Created Folder(i.e Dutta)
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            DbRepository dbRepository = new DbRepository("DbFolder");

            DateTime createdTime = uploadRepository.CreateFolder("", "Dutta");
            dbRepository.CreateFolder("", "Dutta", createdTime);

            createdTime = uploadRepository.CopyFolder("Folder1", "Dutta");
            dbRepository.CopyFolder(@"Folder1", @"Dutta", createdTime);

            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "Dutta");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void CopyNewTopFolderInner() // Copy Newly created Top Folder (i.e Dutta) to Inner Folder(i.e Folder2)
        {
            CreateFolderTopLevel();
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            DateTime createdTime = uploadRepository.CopyFolder("Dutta", @"Folder1\Folder2");
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.CopyFolder("Dutta", @"Folder1\Folder2", createdTime);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }

        [Test]
        public void CopyFolderOuter()  // Copy Inside Folder (i.e Folder2) and its content to Top Level 
        {
            FolderRepository uploadRepository = new FolderRepository($@"{Utils.GetSetupDir()}\UploadFolder");
            DateTime createdTime = uploadRepository.CopyFolder(@"Folder1\Folder2", "");
            DbRepository dbRepository = new DbRepository("DbFolder");
            dbRepository.CopyFolder(@"Folder1\Folder2", "", createdTime);
            List<Storage> difference = Utils.Compare(dbRepository, uploadRepository, "");
            Assert.AreEqual(0, difference.Count, "There should be no difference between repositories");
        }
    }
}

