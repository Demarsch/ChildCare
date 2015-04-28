using System;
using System.Linq;
using DataLib;
using NUnit.Framework;
using TestCore;

namespace Registry.Test.Services
{
    [TestFixture]
    class ScheduleServiceTest
    {
        [Test]
        public void GetRoomsOpenTime_ReturnsOverridenTime()
        {
            var date = new DateTime(2015, 1, 15);
            var context = new TestDataContext();
            context.AddData(new[]
            {
                new ScheduleItem
                {
                    RoomId = 1,
                    RecordTypeId = 1,
                    BeginDate = new DateTime(2015, 1, 1),
                    EndDate = new DateTime(2015, 1, 31),
                    DayOfWeek = (int)date.DayOfWeek,
                    StartTime = TimeSpan.FromHours(8.0),
                    EndTime = TimeSpan.FromHours(17.0)
                },
                new ScheduleItem
                {
                    RoomId = 1,
                    RecordTypeId = 1,
                    BeginDate = date,
                    EndDate = date,
                    DayOfWeek = (int)date.DayOfWeek,
                    StartTime = TimeSpan.FromHours(12.0),
                    EndTime = TimeSpan.FromHours(14.0)
                },
                new ScheduleItem
                {
                    RoomId = 1,
                    RecordTypeId = 1,
                    BeginDate = date,
                    EndDate = date,
                    DayOfWeek = (int)date.DayOfWeek,
                    StartTime = TimeSpan.FromHours(15.0),
                    EndTime = TimeSpan.FromHours(17.0)
                }
            });
            var contextProvider = new TestDataContextProvider();
            contextProvider.SetDataContext(context);
            var service = new ScheduleService(contextProvider);
            var result = service.GetRoomsWorkingTime(date);
            Assert.AreEqual(2, result.Count);
            result = result.OrderBy(x => x.StartTime).ToArray();
            Assert.IsTrue(result.First().StartTime == TimeSpan.FromHours(12.0) && result.First().EndTime == TimeSpan.FromHours(14.0));
            Assert.IsTrue(result.Last().StartTime == TimeSpan.FromHours(15.0) && result.Last().EndTime == TimeSpan.FromHours(17.0));
        }

        [Test]
        public void GetRoomsWorkingTime_ReturnsMultipleResults()
        {
            var date = new DateTime(2015, 1, 15);
            var context = new TestDataContext();
            context.AddData(new[]
            {
                new ScheduleItem
                {
                    RoomId = 1,
                    RecordTypeId = 1,
                    BeginDate = new DateTime(2015, 1, 1),
                    EndDate = new DateTime(2015, 1, 31),
                    DayOfWeek = (int)date.DayOfWeek,
                    StartTime = TimeSpan.FromHours(8.0),
                    EndTime = TimeSpan.FromHours(17.0)
                },
                new ScheduleItem
                {
                    RoomId = 2,
                    RecordTypeId = 2,
                    BeginDate = date,
                    EndDate = date,
                    DayOfWeek = (int)date.DayOfWeek,
                    StartTime = TimeSpan.FromHours(12.0),
                    EndTime = TimeSpan.FromHours(14.0)
                },
                new ScheduleItem
                {
                    RoomId = 1,
                    RecordTypeId = 1,
                    BeginDate = new DateTime(2015, 2, 1),
                    EndDate = new DateTime(2015, 2, 28),
                    DayOfWeek = (int)date.DayOfWeek,
                    StartTime = TimeSpan.FromHours(15.0),
                    EndTime = TimeSpan.FromHours(17.0)
                },
                 new ScheduleItem
                {
                    RoomId = 1,
                    RecordTypeId = 1,
                    BeginDate = new DateTime(2015, 1, 1),
                    EndDate = new DateTime(2015, 1, 31),
                    DayOfWeek = (int)date.DayOfWeek + 1,
                    StartTime = TimeSpan.FromHours(15.0),
                    EndTime = TimeSpan.FromHours(17.0)
                }
            });
            var contextProvider = new TestDataContextProvider();
            contextProvider.SetDataContext(context);
            var service = new ScheduleService(contextProvider);
            var result = service.GetRoomsWorkingTime(date);
            Assert.AreEqual(2, result.Count);
        }
    }
}
