using Core;
using NUnit.Framework;
using TestCore;

namespace StuffLib.Test
{
    [TestFixture]
    public class DataContextCacheServiceTest
    {
        [Test]
        public void GetIdProperty_ReturnsPropertyNameFromIdPropertyAttribute()
        {
            var dataContextCacheService = new DataContextCacheService(new TestDataContextProvider());
            var property = dataContextCacheService.GetIdProperty<ClassWithIdAttribute>();
            Assert.IsNotNull(property);
            Assert.AreEqual("SomeId", property.Name);
        }

        [Test]
        public void GetIdProperty_ReturnsPropertyNameFromIdProperty()
        {
            var dataContextCacheService = new DataContextCacheService(new TestDataContextProvider());
            var property = dataContextCacheService.GetIdProperty<ClassWithIdProperty>();
            Assert.IsNotNull(property);
            Assert.AreEqual(DataContextCacheService.ExpectedIdPropertyName, property.Name);
        }

        [Test]
        public void GetIdProperty_ReturnsNullWhenThereAreNeitherIdAttributeNorIdProperty()
        {
            var dataContextCacheService = new DataContextCacheService(new TestDataContextProvider());
            var property = dataContextCacheService.GetIdProperty<ClassWithoutAnyId>();
            Assert.IsNull(property);
        }

        [Test]
        public void GetNameProperty_ReturnsPropertynameFromNamePropertyAttribute()
        {
            var dataContextCacheService = new DataContextCacheService(new TestDataContextProvider());
            var property = dataContextCacheService.GetNameProperty<ClassWithNameAttribute>();
            Assert.IsNotNull(property);
            Assert.AreEqual("SomeName", property.Name);
        }

        #region Test classes

        [IdProperty("SomeId")]
        class ClassWithIdAttribute
        {
            public int SomeId { get; set; }
        }
        
        class ClassWithIdProperty
        {
            public int Id { get; set; }
        }

        class ClassWithoutAnyId
        {
            public string SomeName { get; set; }
        }

        [NameProperty("SomeName")]
        class ClassWithNameAttribute
        {
            public string SomeName { get; set; }
        }

        #endregion
    }
}
