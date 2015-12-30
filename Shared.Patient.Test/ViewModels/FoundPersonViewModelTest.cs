using Core.Data;
using NUnit.Framework;
using Shared.Patient.ViewModels;

namespace Shared.Patient.Test
{
    [TestFixture]
    public class FoundPersonViewModelTest
    {
        [Test]
        public void FullName_CurrentOnly()
        {
            var target = new FoundPersonViewModel
            {
                CurrentName = new PersonName { LastName = "Last", FirstName = "First", MiddleName = "Middle" }
            };
            Assert.AreEqual("Last First Middle", target.FullName);
        }

        [Test]
        public void FullName_HasCurrentAndPrevious()
        {
            var target = new FoundPersonViewModel
            {
                CurrentName = new PersonName { LastName = "Last", FirstName = "First", MiddleName = "Middle" },
                PreviousName = new PersonName { LastName = "Previous" }
            };
            Assert.AreEqual("Last(Previous) First Middle", target.FullName);
        }

        [Test]
        public void FullName_HasPreviousOnly()
        {
            var target = new FoundPersonViewModel
            {
                CurrentName = new PersonName { LastName = "Last", MiddleName = "Middle" },
                PreviousName = new PersonName { FirstName = "First" }
            };
            Assert.AreEqual("Last First Middle", target.FullName);
        }

        [Test]
        public void FullName_NoCurrentNoPrevious()
        {
            var target = new FoundPersonViewModel();
            Assert.AreEqual(PersonName.UnknownLastName + " " + PersonName.UnknownFirstName, target.FullName);
        }
    }
}
