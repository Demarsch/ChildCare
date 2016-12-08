using Core.Data.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Services
{
    public interface IRecordTypesTree
    {
        int Id { get; set; }
        string Code { get; set; }
        string Name { get; set; }
        string Lvl { get; set; }
        int Level { get; set; }
        List<int> Childs { get; set; }
        List<int> Parents { get; set; }

        List<RecordTypesTree> GetAllChilds();
    }
}
