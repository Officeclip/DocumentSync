using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSyncLib
{
    public class Repository
    {
        public bool Exist(string relativeFolderPath, Storage storage)
        {
            List<Storage> storageList = PopulateStorageList(relativeFolderPath);
            return storage.IsPresent(storageList);
        }

        public virtual List<Storage> PopulateStorageList(string relativeFolderPath)
        {
            throw new Exception("Derived class method should be called");
        }

    }
}
