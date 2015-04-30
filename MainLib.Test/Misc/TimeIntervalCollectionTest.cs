using System;
using System.Linq;
using Core;
using NUnit.Framework;

namespace MainLib.Test
{
    [TestFixture]
    class TimeIntervalCollectionTest
    {
        #region GetTimeIntervalRelation

        [Test]
        public void GetTimeIntervalRelation_ReturnsBefore()
        {
            var first = new TimeInterval(8.0, 9.0);
            var second = new TimeInterval(10.0, 11.0);
            Assert.AreEqual(TimeIntervalRelation.Before, TimeIntervalCollection.GetTimeIntervalRelation(first, second));
        }
        [Test]
        public void GetTimeIntervalRelation_ReturnsAfter()
        {
            var first = new TimeInterval(12.0, 13.0);
            var second = new TimeInterval(10.0, 11.0);
            Assert.AreEqual(TimeIntervalRelation.After, TimeIntervalCollection.GetTimeIntervalRelation(first, second));
        }
        [Test]
        public void GetTimeIntervalRelation_ReturnsStartInside()
        {
            var first = new TimeInterval(8.0, 10.0);
            var second = new TimeInterval(7.0, 9.0);
            Assert.AreEqual(TimeIntervalRelation.StartInside, TimeIntervalCollection.GetTimeIntervalRelation(first, second));
        }
        [Test]
        public void GetTimeIntervalRelation_ReturnsEndInside()
        {
            var first = new TimeInterval(8.0, 10.0);
            var second = new TimeInterval(9.0, 11.0);
            Assert.AreEqual(TimeIntervalRelation.EndInside, TimeIntervalCollection.GetTimeIntervalRelation(first, second));
        }
        [Test]
        public void GetTimeIntervalRelation_ReturnsStartTouching()
        {
            var first = new TimeInterval(8.0, 9.0);
            var second = new TimeInterval(7.0, 8.0);
            Assert.AreEqual(TimeIntervalRelation.StartTouching, TimeIntervalCollection.GetTimeIntervalRelation(first, second));
        }
        [Test]
        public void GetTimeIntervalRelation_ReturnsEndTouching()
        {
            var first = new TimeInterval(8.0, 9.0);
            var second = new TimeInterval(9.0, 11.0);
            Assert.AreEqual(TimeIntervalRelation.EndTouching, TimeIntervalCollection.GetTimeIntervalRelation(first, second));
        }
        [Test]
        public void GetTimeIntervalRelation_ReturnsContains()
        {
            var first = new TimeInterval(8.0, 11.0);
            var second = new TimeInterval(10.0, 11.0);
            Assert.AreEqual(TimeIntervalRelation.Contains, TimeIntervalCollection.GetTimeIntervalRelation(first, second));
        }
        [Test]
        public void GetTimeIntervalRelation_ReturnsIsContained()
        {
            var first = new TimeInterval(8.0, 9.0);
            var second = new TimeInterval(7.0, 11.0);
            Assert.AreEqual(TimeIntervalRelation.IsContained, TimeIntervalCollection.GetTimeIntervalRelation(first, second));
        }
        [Test]
        public void GetTimeIntervalRelation_ReturnsSame()
        {
            var first = new TimeInterval(8.0, 9.0);
            var second = new TimeInterval(8.0, 9.0);
            Assert.AreEqual(TimeIntervalRelation.Same, TimeIntervalCollection.GetTimeIntervalRelation(first, second));
        }

        #endregion

        #region AddInterval

        [Test]
        public void AddInterval_AddTheVeryFirstIntervalUnchanged()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(8.0, 9.0));
            var intervals = collection.ToArray();
            Assert.AreEqual(1, intervals.Length);
            Assert.IsTrue(intervals[0].StartTime == TimeSpan.FromHours(8.0) && intervals[0].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void AddInterval_AddsNewIntervalWhenLiesBeforeTheFirstOne()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(8.0, 9.0));
            collection.AddInterval(new TimeInterval(6.0, 7.0));
            var intervals = collection.ToArray();
            Assert.AreEqual(2, intervals.Length);
            Assert.IsTrue(intervals[0].StartTime == TimeSpan.FromHours(6.0) && intervals[0].EndTime == TimeSpan.FromHours(7.0));
            Assert.IsTrue(intervals[1].StartTime == TimeSpan.FromHours(8.0) && intervals[1].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void AddInterval_AddsNewIntervalWhenLiesAfterTheLastOne()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(6.0, 7.0));
            collection.AddInterval(new TimeInterval(8.0, 9.0));
            var intervals = collection.ToArray();
            Assert.AreEqual(2, intervals.Length);
            Assert.IsTrue(intervals[0].StartTime == TimeSpan.FromHours(6.0) && intervals[0].EndTime == TimeSpan.FromHours(7.0));
            Assert.IsTrue(intervals[1].StartTime == TimeSpan.FromHours(8.0) && intervals[1].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void AddInterval_ExtendsExistingIntervalWhenTouchesItsStart()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(8.0, 9.0));
            collection.AddInterval(new TimeInterval(6.0, 8.0));
            var intervals = collection.ToArray();
            Assert.AreEqual(1, intervals.Length);
            Assert.IsTrue(intervals[0].StartTime == TimeSpan.FromHours(6.0) && intervals[0].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void AddInterval_ExtendsExistingIntervalWhenTouchesItsEnd()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(6.0, 8.0));
            collection.AddInterval(new TimeInterval(8.0, 9.0));
            var intervals = collection.ToArray();
            Assert.AreEqual(1, intervals.Length);
            Assert.IsTrue(intervals[0].StartTime == TimeSpan.FromHours(6.0) && intervals[0].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void AddInterval_ExtendsExistingIntervalWhenStartsInsideIt()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(6.0, 8.0));
            collection.AddInterval(new TimeInterval(7.0, 9.0));
            var intervals = collection.ToArray();
            Assert.AreEqual(1, intervals.Length);
            Assert.IsTrue(intervals[0].StartTime == TimeSpan.FromHours(6.0) && intervals[0].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void AddInterval_ExtendsExistingIntervalWhenEndsInsideIt()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(7.0, 9.0));
            collection.AddInterval(new TimeInterval(6.0, 9.0));
            var intervals = collection.ToArray();
            Assert.AreEqual(1, intervals.Length);
            Assert.IsTrue(intervals[0].StartTime == TimeSpan.FromHours(6.0) && intervals[0].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void AddInterval_ExtendsExistingIntervalWhenContainsIt()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(7.0, 8.0));
            collection.AddInterval(new TimeInterval(6.0, 9.0));
            var intervals = collection.ToArray();
            Assert.AreEqual(1, intervals.Length);
            Assert.IsTrue(intervals[0].StartTime == TimeSpan.FromHours(6.0) && intervals[0].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void AddInterval_UnitesTwoIntervalsWhenTouchesThemBoth()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(6.0, 7.0));
            collection.AddInterval(new TimeInterval(8.0, 9.0));
            collection.AddInterval(new TimeInterval(7.0, 8.0));
            var intervals = collection.ToArray();
            Assert.AreEqual(1, intervals.Length);
            Assert.IsTrue(intervals[0].StartTime == TimeSpan.FromHours(6.0) && intervals[0].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void AddInterval_UnitesTwoIntervalsWhenStartsInsideFirstOneAndEndsInsideSecondOne()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(6.0, 7.0));
            collection.AddInterval(new TimeInterval(8.0, 9.0));
            collection.AddInterval(new TimeInterval(6.5, 8.5));
            var intervals = collection.ToArray();
            Assert.AreEqual(1, intervals.Length);
            Assert.IsTrue(intervals[0].StartTime == TimeSpan.FromHours(6.0) && intervals[0].EndTime == TimeSpan.FromHours(9.0));
        }

        #endregion

        #region RemoveInterval

        [Test]
        public void RemoveInterval_DoesNothingIfRemovesFromEmptyList()
        {
            var collection = new TimeIntervalCollection();
            collection.RemoveInterval(new TimeInterval(8.0, 9.0));
            Assert.AreEqual(0, collection.Count());
        }

        [Test]
        public void RemoveInterval_RemoveTheSameInterval()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(8.0, 9.0));
            collection.RemoveInterval(new TimeInterval(8.0, 9.0));
            Assert.AreEqual(0, collection.Count());
        }

        [Test]
        public void RemoveInterval_DoesNothingWhenTouchesStartOfExistingInterval()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(8.0, 9.0));
            collection.RemoveInterval(new TimeInterval(7.0, 8.0));
            var result = collection.ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result[0].StartTime == TimeSpan.FromHours(8.0) && result[0].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void RemoveInterval_DoesNothingWhenTouchesEndOfExistingInterval()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(8.0, 9.0));
            collection.RemoveInterval(new TimeInterval(9.0, 10.0));
            var result = collection.ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result[0].StartTime == TimeSpan.FromHours(8.0) && result[0].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void RemoveInterval_RemovesLeftPartOfIntervalThatStartsInsideTheTargetOne()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(8.0, 10.0));
            collection.RemoveInterval(new TimeInterval(7.0, 9.0));
            var result = collection.ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result[0].StartTime == TimeSpan.FromHours(9.0) && result[0].EndTime == TimeSpan.FromHours(10.0));
        }

        [Test]
        public void RemoveInterval_RemovesRightPartOfIntervalThatEndsInsideTheTargetOne()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(8.0, 10.0));
            collection.RemoveInterval(new TimeInterval(9.0, 11.0));
            var result = collection.ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result[0].StartTime == TimeSpan.FromHours(8.0) && result[0].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void RemoveInterval_RemovesLeftPartOfIntervalThatContainsTheTargetOneAndHasSameStartTime()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(8.0, 10.0));
            collection.RemoveInterval(new TimeInterval(8.0, 9.0));
            var result = collection.ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result[0].StartTime == TimeSpan.FromHours(9.0) && result[0].EndTime == TimeSpan.FromHours(10.0));
        }

        [Test]
        public void RemoveInterval_RemovesRightPartOfIntervalThatContainsTheTargetOneAndHasSameEndTime()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(8.0, 10.0));
            collection.RemoveInterval(new TimeInterval(9.0, 10.0));
            var result = collection.ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result[0].StartTime == TimeSpan.FromHours(8.0) && result[0].EndTime == TimeSpan.FromHours(9.0));
        }

        [Test]
        public void RemoveInterval_SplitIntervalIntoTwoWhenLiesInsideIt()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(8.0, 11.0));
            collection.RemoveInterval(new TimeInterval(9.0, 10.0));
            var result = collection.ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.IsTrue(result[0].StartTime == TimeSpan.FromHours(8.0) && result[0].EndTime == TimeSpan.FromHours(9.0));
            Assert.IsTrue(result[1].StartTime == TimeSpan.FromHours(10.0) && result[1].EndTime == TimeSpan.FromHours(11.0));
        }

        [Test]
        public void RemoveInterval_RemovesIntervalsThatLiesInsideTheTargetOne()
        {
            var collection = new TimeIntervalCollection();
            collection.AddInterval(new TimeInterval(8.0, 9.0));
            collection.AddInterval(new TimeInterval(10.0, 11.0));
            collection.RemoveInterval(new TimeInterval(7.0, 12.0));
            Assert.AreEqual(0, collection.Count());
        }

        #endregion
    }
}
