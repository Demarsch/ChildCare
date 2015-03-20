using System;
using Core;
using NUnit.Framework;
using TestCore;

namespace MainLib.Test
{
    [TestFixture]
    class PatientServiceTest
    {
        [Test]
        public void ParseUserInput_SingleNameIsParsed()
        {
            var patientService = new PatientService(new TestDataContextProvider());
            var parsedUserInput = patientService.ParseUserInput("word");
            Assert.AreEqual(1, parsedUserInput.Names.Count, "A single name should be the result of a test");
            Assert.AreEqual("word", parsedUserInput.Names[0], "A single name should be the same as in the user input");
        }
        
        [Test]
        public void ParseUserInput_MultipleWordsAreParsed()
        {
            var patientService = new PatientService(new TestDataContextProvider());
            var parsedUserInput = patientService.ParseUserInput("word  Something");
            Assert.AreEqual(2, parsedUserInput.Names.Count, "Multimple words should be extracted");
            Assert.AreEqual("word", parsedUserInput.Names[0], "All words must be the same as in the original string");
            Assert.AreEqual("Something", parsedUserInput.Names[1], "All words must be the same as in the original string");
        }

        [Test]
        public void ParseUserInput_DateIsParsedWithDotDelimiter()
        {
            var patientService = new PatientService(new TestDataContextProvider());
            var parsedUserInput = patientService.ParseUserInput("1.02.2003");
            Assert.AreEqual(new DateTime(2003, 2, 1), parsedUserInput.Date, "Date in short format should be parsed with dot delimiter");
        }

        [Test]
        public void ParseUserInput_DateIsParsedWithSlashDelimiter()
        {
            var patientService = new PatientService(new TestDataContextProvider());
            var parsedUserInput = patientService.ParseUserInput("1/02/2003");
            Assert.AreEqual(new DateTime(2003, 2, 1), parsedUserInput.Date, "Date in short format should be parsed with slash delimiter");
        }

        [Test]
        public void ParseUserInput_MedicalNumberIsParsed()
        {
            var patientService = new PatientService(new TestDataContextProvider());
            var parsedUserInput = patientService.ParseUserInput("1234567890123");
            Assert.AreEqual("1234567890123", parsedUserInput.Number, "Medical number should be parsed");
        }

        [Test]
        public void ParseUserInput_SnilsWithDelimitersIsParsed()
        {
            var patientService = new PatientService(new TestDataContextProvider());
            var parsedUserInput = patientService.ParseUserInput("123 456 789-01");
            Assert.AreEqual("123 456 789-01", parsedUserInput.Number, "Snils with delimiters should be parsed");
        }

        [Test]
        public void ParseUserInput_SnilsWithoutDelimitersIsParsedAndDelimitized()
        {
            var patientService = new PatientService(new TestDataContextProvider());
            var parsedUserInput = patientService.ParseUserInput("12345678901");
            Assert.AreEqual("123 456 789-01", parsedUserInput.Number, "Snils without delimiters should be parsed and delimitized");
        }


    }
}
