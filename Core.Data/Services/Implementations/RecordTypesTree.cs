using Core.Data.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Core.Data.Services
{
    public class RecordTypesTree : IRecordTypesTree
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Lvl { get; set; }
        public int Level { get; set; }
        public List<int> Childs { get; set; }
        public List<int> Parents { get; set; }

        private static IDbContextProvider contextProvider;

        public RecordTypesTree(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            RecordTypesTree.contextProvider = contextProvider;
        }


        private static IList<RecordTypesTree> allRecordTypes = null;
        public static IList<RecordTypesTree> AllRecordTypes
        {
            get
            {
                if (allRecordTypes == null)
                {
                    RecordTypesTree rtt = new RecordTypesTree(contextProvider);
                    allRecordTypes = rtt.GetAllChilds();
                }
                return allRecordTypes;
            }
        }

        private List<RecordTypesTree> GetRecordTypesTreeChilds(RecordTypesTreeQueryItem rtype, RecordTypesTreeQueryItem[] alltypes, int numlevel = 0, string txtlevel = "", string inclevel = "     ")
        {
            List<RecordTypesTree> list = new List<RecordTypesTree>();
            list.Add(new RecordTypesTree(contextProvider)
            {
                Id = rtype.Id,
                ParentId = rtype.ParentId,
                Lvl = txtlevel,
                Level = numlevel,
                Name = rtype.Name,
                Code = rtype.Code,
                Childs = new List<int>(),
                Parents = new List<int>()
            });
            list.First().Childs.Add(rtype.Id);
            list.First().Parents.Add(rtype.Id);
            var curRType = rtype;
            while (curRType != null && curRType.ParentId.HasValue)
            {
                list.First().Parents.Add(curRType.ParentId.Value);
                curRType = alltypes.FirstOrDefault(x => x.Id == curRType.ParentId);
            }
            foreach (RecordTypesTreeQueryItem t in alltypes.Where(x => x.ParentId == rtype.Id).OrderBy(x => x.Priority).ThenBy(x => x.Name))
            {
                List<RecordTypesTree> childs = GetRecordTypesTreeChilds(t, alltypes, numlevel + 1, txtlevel + inclevel, inclevel);
                list.First().Childs.AddRange(childs.First().Childs);
                list.AddRange(childs);
            }
            return list;
        }

        public List<RecordTypesTree> GetAllChilds()
        {
            if (allrectypes == null) allrectypes = GetAllRecordTypesTreeChilds();
            return allrectypes;
        }

        private List<RecordTypesTree> allrectypes = null;

        private List<RecordTypesTree> GetAllRecordTypesTreeChilds()
        {
            using (var context = contextProvider.CreateNewContext())
            {
                var alltypes = context.Set<RecordType>().Where(x => x.IsRecord).Select(x => new RecordTypesTreeQueryItem() { Id = x.Id, ParentId = x.ParentId, Code = x.Code, Name = (x.ShortName != "" ? x.ShortName : x.Name), Priority = x.Priority }).ToArray();
                return alltypes.Where(x => !x.ParentId.HasValue).OrderBy(x => x.Priority).ThenBy(x => x.Name).SelectMany(x => GetRecordTypesTreeChilds(x, alltypes)).ToList();
            }
        }
    }
}
