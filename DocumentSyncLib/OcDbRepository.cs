using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSyncLib
{
    public class OcDbRepository : Repository, IRepository
    {
        public void Create(
            string relativeFolder,
            string name,
            bool isFolder = false)
        {
            throw new NotImplementedException();
        }

        public void Delete(
            string path,
            bool isFolder = false)
        {
            throw new NotImplementedException();
        }

        public void CreateFolder(
                                int userId,
                                int groupId,
                                int parentFolderId,
                                string name,
                                DateTime createdDate)
        {
            
        }

    }
}
