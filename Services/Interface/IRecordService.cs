using System.Collections.Generic;
using DataLib;

namespace Core
{
    public interface IRecordService
    {
        RecordType GetRecordTypeById(int id);
        ICollection<RecordType> GetRecordTypesByOptions(string[] options);
        ICollection<RecordType> GetRecordTypesByOptions(string options);
        ICollection<RecordTypeRole> GetRecordTypeRolesByOptions(string[] options);
        ICollection<RecordTypeRole> GetRecordTypeRolesByOptions(string options);
        ICollection<RecordType> GetAllRecordTypes();
        ICollection<RecordType> GetRecordTypesByName(string name);        
            
    }
}
