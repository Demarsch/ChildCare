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
            var context = new TestDataContext();
            var date = new DateTime(2015, 1, 15);
            var generalScheduleDay = new ScheduleDay
            {
                Id = 1,
                DayOfWeek = (int)date.DayOfWeek,
                BeginDate = new DateTime(2015, 1, 1),
                EndDate = new DateTime(2015, 1, 31),
                RecordTypeId = 1,
                RoomId = 1
            };
            var specificScheduleDay = new ScheduleDay
            {
                Id = 2,
                DayOfWeek = (int)date.DayOfWeek,
                BeginDate = date,
                EndDate = date,
                RecordTypeId = 1,
                RoomId = 1
            };
            var generalScheduleItem = new ScheduleDayItem
            {
                Id = 1,
                ScheduleDayId = generalScheduleDay.Id,
                ScheduleDay = generalScheduleDay,
                StartTime = TimeSpan.FromHours(8.0),
                EndTime = TimeSpan.FromHours(17.0)
            };
            var specificScheduleItem = new ScheduleDayItem
            {
                Id = 2,
                ScheduleDayId = specificScheduleDay.Id,
                ScheduleDay = specificScheduleDay,
                StartTime = TimeSpan.FromHours(13.0),
                EndTime = TimeSpan.FromHours(14.0)
            };
            generalScheduleDay.ScheduleDayItems = new[] { generalScheduleItem };
            specificScheduleDay.ScheduleDayItems = new[] { specificScheduleItem };
            context.AddData(new[] { generalScheduleDay, specificScheduleDay });
            context.AddData(new[] { generalScheduleItem, specificScheduleItem });
            var contextProvider = new TestDataContextProvider();
            contextProvider.SetDataContext(context);
            var target = new ScheduleService(contextProvider);
            var result = target.GetRoomsWorkingTime(date);
            var resultItem = result.GetRoomAndRecordTypeWorkingTime(1, 1).Single();
            Assert.IsTrue(resultItem.StartTime == specificScheduleItem.StartTime && resultItem.EndTime == specificScheduleItem.EndTime);
        }
    }
}
