using Core.Data;
using Core.Data.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyclinicModule.Services
{
    public interface IPolyclinicService
    {
        IDisposableQueryable<Room> GetPolyclinicRooms();

        IDisposableQueryable<Assignment> GetAssignments(DateTime selectedDate, int selectedRoomId);

        IDisposableQueryable<Record> GetRecords(DateTime selectedDate, int selectedRoomId);

        IDisposableQueryable<Assignment> GetAssignmentById(int id);

        IDisposableQueryable<Record> GetRecordById(int id);
    }
}
