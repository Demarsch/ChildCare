using System;
using System.Linq;
using Core;
using DataLib;
using NUnit.Framework;
using TestCore;

namespace Registry.Test.Services
{
    [TestFixture]
    class ScheduleServiceTest
    {
        #region GetRoomsWorkingTime

        [Test]
        public void GetRoomsWorkingTime_ReturnsOverridenTime()
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

        #endregion

        #region GetAvailableTimeIntervals

        [Test]
        public void GetAvailableTimeIntervals_SplitsIntervalIntoEvenPieces()
        {
            var scheduleService = new ScheduleService(new TestDataContextProvider());
            var result = scheduleService.GetAvailableTimeIntervals(new[] { new TimeInterval(TimeSpan.FromHours(8.0), TimeSpan.FromHours(9.0)) }, null, 30, 30).ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.IsTrue(result[0].StartTime == TimeSpan.FromHours(8.0) && result[0].EndTime == TimeSpan.FromHours(8.5));
            Assert.IsTrue(result[1].StartTime == TimeSpan.FromHours(8.5) && result[1].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void GetAvailableTimeIntervals_ReturnsFullIntervalAndPartialInterval()
        {
            var scheduleService = new ScheduleService(new TestDataContextProvider());
            var result = scheduleService.GetAvailableTimeIntervals(new[] { new TimeInterval(TimeSpan.FromHours(8.0), TimeSpan.FromHours(9.0)) }, null, 25, 10).ToArray();
            Assert.AreEqual(3, result.Length);
            Assert.IsTrue(result[0].StartTime == new TimeSpan(8, 0, 0) && result[0].EndTime == new TimeSpan(8, 25, 0));
            Assert.IsTrue(result[1].StartTime == new TimeSpan(8, 25, 0) && result[1].EndTime == new TimeSpan(8, 50, 0));
            Assert.IsTrue(result[2].StartTime == new TimeSpan(8, 50, 0) && result[2].EndTime == new TimeSpan(9, 0, 0));
        }

        [Test]
        public void GetAvailableTimeIntervals_DecreaseFullIntervalsToBeAbleToAccomodateGap()
        {
            var scheduleService = new ScheduleService(new TestDataContextProvider());
            var result = scheduleService.GetAvailableTimeIntervals(new[] { new TimeInterval(TimeSpan.FromHours(8.0), TimeSpan.FromHours(9.0)) }, null, 25, 15).ToArray();
            Assert.AreEqual(3, result.Length);
            Assert.IsTrue(result[0].StartTime == new TimeSpan(8, 0, 0) && result[0].EndTime == new TimeSpan(8, 25, 0));
            Assert.IsTrue(result[1].StartTime == new TimeSpan(8, 25, 0) && result[1].EndTime == new TimeSpan(8, 45, 0));
            Assert.IsTrue(result[2].StartTime == new TimeSpan(8, 45, 0) && result[2].EndTime == new TimeSpan(9, 0, 0));
        }

        [Test]
        public void GetAvailableTimeIntervals_ReturnsNothingWhenNotEnoughFreeTime()
        {
            var scheduleService = new ScheduleService(new TestDataContextProvider());
            var result = scheduleService.GetAvailableTimeIntervals(new[] { new TimeInterval(TimeSpan.FromHours(8.0), TimeSpan.FromHours(9.0)) }, null, 120, 120).ToArray();
            Assert.AreEqual(0, result.Length);
        }

        #endregion
    }
}
