using NUnit.Framework;

namespace Core.Data.Test
{
    [TestFixture]
    public class PersonTest
    {
        [Test]
        public void DelimitizeSnils_NullSnilsGoesUnknown()
        {
            Assert.AreEqual(Person.UnknownSnils, Person.DelimitizeSnils(null));
        }

        [Test]
        public void DelimitizeSnils_IncompleteSnilsRemainsTheSame()
        {
            var incompleteSnils = "123";
            Assert.AreEqual(incompleteSnils, Person.DelimitizeSnils(incompleteSnils));
        }

        [Test]
        public void DelimitizeSnils_CompleteSnilsBecomesDelimitized()
        {
            Assert.AreEqual("123-456-789 01", Person.DelimitizeSnils("12345678901"));
        }
    }
}
