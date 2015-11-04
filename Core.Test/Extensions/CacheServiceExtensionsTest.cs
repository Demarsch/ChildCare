using System;
using Core.Extensions;
using Core.Services;
using Moq;
using NUnit.Framework;

namespace Core.Test.Extensions
{
    [TestFixture]
    public class CacheServiceExtensionsTest
    {
        [Test]
        public void AutoWire_ThrowsExceptionWhenThereIsNoIdProperty()
        {
            var cacheService = new Mock<ICacheService>().Object;
            var invalidClass = new InvalidClassToAutoWire();
            Assert.Throws<InvalidOperationException>(() => cacheService.AutoWire(invalidClass, x => x.CachedObject));
        }

        [Test]
        public void AutoWire_SetsNullWhenCacheHasNoObject()
        {
            var mock = new Mock<ICacheService>();
            mock.Setup(x => x.GetItemById<CachedObject>(It.IsAny<int>())).Returns((CachedObject)null);
            var cacheService = mock.Object;
            var classToAutowire = new ValidClassToAutoWire { CachedObject = new CachedObject() };
            cacheService.AutoWire(classToAutowire, x => x.CachedObject);
            Assert.IsNull(classToAutowire.CachedObject);
        }

        [Test]
        public void AutoWire_SetsObjectFromCache()
        {
            var mock = new Mock<ICacheService>();
            var cachedObject = new CachedObject { Id = 1, Name = "One" };
            mock.Setup(x => x.GetItemById<CachedObject>(1)).Returns(cachedObject);
            var cacheService = mock.Object;
            var classToAutowire = new ValidClassToAutoWire { CachedObjectId = 1 };
            cacheService.AutoWire(classToAutowire, x => x.CachedObject);
            Assert.AreSame(cachedObject, classToAutowire.CachedObject);
        }

        private class InvalidClassToAutoWire
        {
            public CachedObject CachedObject { get; set; }
        }

        private class ValidClassToAutoWire
        {
            public int CachedObjectId { get; set; }

            public CachedObject CachedObject { get; set; }
        }

        private class CachedObject
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
