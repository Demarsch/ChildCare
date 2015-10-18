using System;
using System.Linq;
using Core.Expressions;
using NUnit.Framework;

namespace Core.Test
{
    [TestFixture]
    public class DateBasedSearchExpressionProviderTest
    {
        private class MockProvider : DateBasedSearchExpressionProvider<int>
        {
            public override SearchExpression<int> CreateSearchExpression(string searchPattern)
            {
                throw new System.NotImplementedException();
            }
        }

        [Test]
        public void GetDates_RetrievesShortDates()
        {
            var result = new MockProvider().GetDates("01-10-2015 2/10/2015 3.10.2015");
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(new DateTime(2015, 10, 1), result.ElementAt(0));
            Assert.AreEqual(new DateTime(2015, 10, 2), result.ElementAt(1));
            Assert.AreEqual(new DateTime(2015, 10, 3), result.ElementAt(2));
        }
    }
}
