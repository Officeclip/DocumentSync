using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSync
{
    public interface IRepository
    {
        void Create(string relativeFolder, string name, bool isFolder = false);
        void Move(string oldPath, string newPath, bool isFolder = false);
        bool Delete(string relativeFolder, string name, bool isFolder = false);
        bool isMatch(Storage storage);
        List<Storage> PopulateStorageList(string folder);
    }
}
