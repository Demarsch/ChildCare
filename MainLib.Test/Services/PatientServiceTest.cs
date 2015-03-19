using Core;
using NUnit.Framework;
using TestCore;

namespace MainLib.Test
{
    [TestFixture]
    class PatientServiceTest
    {
        [Test]
        public void ParseUserInputSingleName()
        {
            var patientService = new PatientService(new TestDataContextProvider());
            var parsedUserInput = patientService.ParseUserInput("word");
            Assert.AreEqual(1, parsedUserInput.Names.Count, "A single name should be the result of a test");
            Assert.AreEqual("word", parsedUserInput.Names[0], "A single name should be the same as in the user input");
        }
    }
}
