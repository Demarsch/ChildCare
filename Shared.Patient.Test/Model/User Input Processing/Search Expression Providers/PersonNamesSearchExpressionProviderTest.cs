using System.Linq;
using NUnit.Framework;
using Shared.Patient.Model;

namespace Shared.Patient.Test
{
    [TestFixture]
    public class PersonNamesSearchExpressionProviderTest
    {
        [Test]
        public void GetNames_SeparatedWords()
        {
            var target = new PersonNamesSearchExpressionProvider().GetNames("Ivanov Ivan Ivanovich");
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual("Ivanov", target.ElementAt(0));
            Assert.AreEqual("Ivan", target.ElementAt(1));
            Assert.AreEqual("Ivanovich", target.ElementAt(2));
        }

        [Test]
        public void GetNames_IgnoresDates()
        {
            var target = new PersonNamesSearchExpressionProvider().GetNames("ivan 18-10-2015");
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual("ivan", target.First());
        }
    }
}
