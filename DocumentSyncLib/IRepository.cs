using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSyncLib
{
    public interface IRepository
    {
        void Create(string relativeFolder, string name, bool isFolder = false);
        //void Move(string oldPath, string newPath, bool isFolder = false);
        void Delete(string path, bool isFolder = false);
        //bool Exist(string relativeFolderPath, Storage storage);
        List<Storage> PopulateStorageList(string folder);
    }
}
