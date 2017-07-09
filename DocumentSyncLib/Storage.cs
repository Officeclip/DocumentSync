using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentSyncLib
{
    public class Storage
    {
        public bool IsFolder { get; set; }
        public DateTime LastUpdated { get; set; }
        public decimal? Size { get; set; } // null if it is folder
        public string Name { get; set; }
        public Constants.Action Action { get; set; } // None, Add, Update, Delete
        public override string ToString()
        {
            return $"{Name}-{IsFolder}-{LastUpdated}-{Size}-{Action}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Storage storage = obj as Storage;
            if (storage.IsFolder)
            {
                return (storage.Name == Name);
            }
            return (
                (storage.Name == Name) &&
                ((storage.LastUpdated - LastUpdated).Seconds <= 2));
        }

        public bool IsModified(Storage storage)
        {
            if (storage.IsFolder)
            {
                return false;
            }
            return (
                (storage.IsFolder == IsFolder) &&
                (storage.Name == Name) &&
                ((storage.LastUpdated - LastUpdated).Seconds != 0));
        }

        public bool IsPresent(List<Storage> storageList)
        {
            foreach (Storage storage in storageList)
            {
                if (Equals(storage))
                {
                    return true;
                }
            }
            return false;
        }

        public Storage FindModified(List<Storage> storageList)
        {
            foreach (Storage storage in storageList)
            {
                if (IsModified(storage))
                {
                    return storage;
                }
            }
            return null;
        }
    }

    public class Constants
    {
        public enum Action
        {
            None = 0,
            Create = 1,
            Move = 2,
            Update = 3,
            Delete = 4
        }
    }
}
