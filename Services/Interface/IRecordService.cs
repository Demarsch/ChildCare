using System.Collections.Generic;
using DataLib;
using System;

namespace Core
{
    public interface IRecordService
    {
        RecordType GetRecordTypeById(int id);
        ICollection<RecordType> GetAllRecordTypes();
        ICollection<RecordType> GetRecordTypesByName(string name);        
            
    }
}
