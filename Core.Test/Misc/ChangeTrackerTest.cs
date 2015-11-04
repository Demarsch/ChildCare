using System.Collections.Generic;
using Core.Misc;
using NUnit.Framework;

namespace Core.Test.Misc
{
    [TestFixture]
    public class ChangeTrackerTest
    {
        private int X { get; set; }

        [Test]
        public void Track_DoesNothingWhenTrackerIsDisabled()
        {
            var target = new ChangeTracker();
            target.Track(0, 1, "X");
            Assert.IsFalse(target.HasChanges);
        }

        [Test]
        public void Track_TracksChangesWhenEnabled()
        {
            var target = new ChangeTracker { IsEnabled = true };
            target.Track(0, 1, "X");
            Assert.IsTrue(target.HasChanges);
            Assert.IsTrue(target.PropertyHasChanges(() => X));
        }

        [Test]
        public void Track_UsesRegisteredComparer()
        {
            var target = new ChangeTracker { IsEnabled = true };
            var comparer = new CustomComparer();
            target.RegisterComparer(() => X, comparer);
            target.Track(0, 1, "X");
            target.Track(1, 2, "X");
            Assert.IsTrue(comparer.IsEqualsInvoked);
        }

        [Test]
        public void Track_UntracksChangesWhenOriginalValueIsSet()
        {
            var target = new ChangeTracker { IsEnabled = true };
            target.Track(0, 1, "X");
            target.Track(1, 0, "X");
            Assert.IsFalse(target.HasChanges);
            Assert.IsFalse(target.PropertyHasChanges(() => X));
        }

        [Test]
        public void Untrack_DoesNothingIfPropertyHasNoChanges()
        {
            var target = new ChangeTracker { IsEnabled = true };
            int value = -1;
            target.Track(0, 1, "y");
            target.Untrack(ref value, () => X);
            Assert.AreEqual(-1, value);
        }

        [Test]
        public void Untrack_SetsOriginalValueIfPropertyHasChanges()
        {
            var target = new ChangeTracker { IsEnabled = true };
            int originalValueSource = 0;
            target.Track(originalValueSource, 1, "X");
            originalValueSource = 1;
            target.Untrack(ref originalValueSource, () => X);
            Assert.AreEqual(0, originalValueSource);
            Assert.IsFalse(target.HasChanges);
            Assert.IsFalse(target.PropertyHasChanges(() => X));
        }

        [Test]
        public void IsEnabled_DisablingClearAllChanges()
        {
            var target = new ChangeTracker { IsEnabled = true };
            target.Track(0, 1, "X");
            target.IsEnabled = false;
            Assert.IsFalse(target.HasChanges);
        }

        private class CustomComparer : IEqualityComparer<int>
        {
            public bool IsEqualsInvoked { get; private set; }

            public bool Equals(int x, int y)
            {
                IsEqualsInvoked = true;
                return x == y;
            }

            public int GetHashCode(int obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
