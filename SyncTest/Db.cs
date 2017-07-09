using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using DocumentSyncLib;
using System.IO;

namespace SyncTest
{
    [TestFixture]
    [Category("Db")]
    public class Db
    {
        [SetUp]
        public void Initialize()
        {
            string sqlString = File.ReadAllText($@"{Utils.GetSetupDir()}\ResetFolder.sql");
            Utils.ExecuteNonQuery(sqlString, "Db");
        }

        [TearDown]
        public void Cleanup()
        {

        }
        
        [Test]
        public void PopulateStorage()
        {
            DbRepository dbRepository = new DbRepository("DbFolder");
            List<Storage> storageList = 
                    dbRepository.PopulateStorageList("Folder1");
            Assert.AreEqual(2, storageList.Count, "There should only be two elements in the list");
        }

        [Test]
        public void CreateFolderTest()
        {
            DbRepository dBRepository = new DbRepository("DbFolder");
            DateTime createdDateTime = new DateTime(2017, 03, 27);
            dBRepository.CreateFolder("Folder1", "Dutta", createdDateTime);
            Storage input = new Storage
            {
                Name = "Dutta",
                IsFolder = true
            };

            Assert.IsTrue(dBRepository.Exist("Folder1", input), "Folder not in the list");
        }

        [Test]
        public void CreateFile()
        {
            DbRepository dBRepository = new DbRepository("DbFolder");
            DateTime createdDateTime = new DateTime(2017, 03, 27);
            dBRepository.CreateFile("Folder1", "Dutta.txt", createdDateTime);
            List<Storage> storageList =
                    dBRepository.PopulateStorageList("Folder1");

            bool isPresent = false;
            foreach (Storage storage in storageList)
            {
                if ((!storage.IsFolder) && (storage.Name == "Dutta.txt"))
                {
                    isPresent = true;
                    break;
                }
            }
            Assert.IsTrue(isPresent, "Dutta.txt is not in the list");
        }

        [Test]
        public void DeleteFolder()
        {
            DbRepository dBRepository = new DbRepository("DbFolder");
            List<Storage> storageList =
                    dBRepository.PopulateStorageList("");
            dBRepository.Delete("Folder1", true);

            storageList =
                    dBRepository.PopulateStorageList("");

            bool isPresent = false;
            foreach (Storage storage in storageList)
            {
                if ((storage.IsFolder) && (storage.Name == "Folder1"))
                {
                    isPresent = true;
                    break;
                }
            }
            Assert.IsTrue(isPresent, "Folder1 is not in the list");
        }

        [Test]
        public void DeleteFile()
        {
            DbRepository dBRepository = new DbRepository("DbFolder");
            List<Storage> storageList =
                   dBRepository.PopulateStorageList("");
            dBRepository.Delete("File1.txt", false);

            bool isPresent = false;
            foreach (Storage storage in storageList)
            {
                if ((!storage.IsFolder) && (storage.Name == "File1.txt"))
                {
                    isPresent = true;
                    break;
                }
            }
            Assert.IsTrue(isPresent, "File1.txt is not in the list");
        }
    }
}
