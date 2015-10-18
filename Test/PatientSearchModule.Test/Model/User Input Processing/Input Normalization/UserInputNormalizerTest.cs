using System;
using NUnit.Framework;
using PatientSearchModule.Model;

namespace PatientSearchModule.Test
{
    [TestFixture]
    public class UserInputNormalizerTest
    {
        [Test]
        public void NormalizeUserInput_DoesNothing()
        {
            var text = "ivanov ivan ivanovich 01-01-2015";
            var result = new UserInputNormalizer().NormalizeUserInput(text);
            Assert.AreEqual(text, result);
        }

        [Test]
        public void NormalizeUserInput_TrimsText()
        {
            var result = new UserInputNormalizer().NormalizeUserInput(" ivan ");
            Assert.AreEqual("ivan", result);
        }

        [Test]
        public void NormalizeUserInput_ReplacesNewLinesWithSpaces()
        {
            var result = new UserInputNormalizer().NormalizeUserInput("ivan" + Environment.NewLine + "ivan" );
            Assert.AreEqual("ivan ivan", result);
        }

        [Test]
        public void NormalizeUserInput_ReplacesTabsWithSpaces()
        {
            var result = new UserInputNormalizer().NormalizeUserInput("ivan\tivan");
            Assert.AreEqual("ivan ivan", result);
        }

        [Test]
        public void NormalizeUserInput_ShrinksMultipleSpacesIntoOne()
        {
            var result = new UserInputNormalizer().NormalizeUserInput("ivan   ivan");
            Assert.AreEqual("ivan ivan", result);
        }
    }
}
