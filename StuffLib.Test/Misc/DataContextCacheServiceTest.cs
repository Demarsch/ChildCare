using System;
using Core;
using NUnit.Framework;
using TestCore;

namespace StuffLib.Test
{
    [TestFixture]
    public class DataContextCacheServiceTest
    {
        #region GetIdProperty

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

        #endregion

        #region GetNameProperty
        [Test]
        public void GetNameProperty_ReturnsPropertyNameFromNamePropertyAttribute()
        {
            var dataContextCacheService = new DataContextCacheService(new TestDataContextProvider());
            var property = dataContextCacheService.GetNameProperty<ClassWithNameAttribute>();
            Assert.IsNotNull(property);
            Assert.AreEqual("SomeName", property.Name);
        }

        [Test]
        public void GetIdProperty_ReturnsPropertyNameFromPrimaryNameProperty()
        {
            var dataContextCacheService = new DataContextCacheService(new TestDataContextProvider());
            var property = dataContextCacheService.GetNameProperty<ClassWithPrimaryNameProperty>();
            Assert.IsNotNull(property);
            Assert.AreEqual(DataContextCacheService.ExpectedPrimaryNamePropertyName, property.Name);
        }

        [Test]
        public void GetIdProperty_ReturnsPropertyNameFromSecondaryNameProperty()
        {
            var dataContextCacheService = new DataContextCacheService(new TestDataContextProvider());
            var property = dataContextCacheService.GetNameProperty<ClassWithSecondaryNameProperty>();
            Assert.IsNotNull(property);
            Assert.AreEqual(DataContextCacheService.ExpectedSecondaryNamePropertyName, property.Name);
        }

        [Test]
        public void GetIdProperty_ReturnsNullWhenThereAreNeitherNameAttributeNorNameProperties()
        {
            var dataContextCacheService = new DataContextCacheService(new TestDataContextProvider());
            var property = dataContextCacheService.GetNameProperty<ClassWithoutAnyName>();
            Assert.IsNull(property);
        }

        #endregion

        #region GetIdSelectorFunction

        [Test]
        public void GetIdSelectorFunction_ThrowsExceptionWhenClassHasNoIdProperty()
        {
            var dataContextCacheService = new DataContextCacheService(new TestDataContextProvider());
            Assert.Throws<ArgumentException>(() => dataContextCacheService.GetIdSelectorFunction<ClassWithoutAnyId>());
        }

        [Test]
        public void GetIdSelectorFunction_ReturnsValidFunctionForClassWithIdProperty()
        {
            var dataContextCacheService = new DataContextCacheService(new TestDataContextProvider());
            var selectorFunction = dataContextCacheService.GetIdSelectorFunction<ClassWithIdAttribute>();
            var testObject = new ClassWithIdAttribute {SomeId = 999};
            Assert.AreEqual(999, selectorFunction(testObject));
        }

        #endregion

        #region GetNameSelectorFunction

        [Test]
        public void GetNameSelectorFunction_ThrowsExceptionWhenClassHasNoNameProperty()
        {
            var dataContextCacheService = new DataContextCacheService(new TestDataContextProvider());
            Assert.Throws<ArgumentException>(() => dataContextCacheService.GetNameSelectorFunction<ClassWithoutAnyName>());
        }

        [Test]
        public void GetNameSelectorFunction_ReturnsValidFunctionForClassWithNameProperty()
        {
            var dataContextCacheService = new DataContextCacheService(new TestDataContextProvider());
            var selectorFunction = dataContextCacheService.GetNameSelectorFunction<ClassWithNameAttribute>();
            var testObject = new ClassWithNameAttribute { SomeName = "Some name" };
            Assert.AreEqual("Some name", selectorFunction(testObject));
        }
        #endregion

        #region GetItemById

        [Test]
        public void GetItemById_ReturnsNullWhereThereIsNoItemWithSpecifiedId()
        {
            var testDataContextProvider = new TestDataContextProvider();
            var testDataContext = new TestDataContext();
            var testObject = new ClassWithBothNameAndId {Id = 1, Name = "Some name"};
            testDataContext.AddData(new[] { testObject });
            testDataContextProvider.SetDataContext(testDataContext);
            var dataContextCacheService = new DataContextCacheService(testDataContextProvider);
            var result = dataContextCacheService.GetItemById<ClassWithBothNameAndId>(0);
            Assert.IsNull(result);
        }

        [Test]
        public void GetItemById_ReturnsValidObjectById()
        {
            var testDataContextProvider = new TestDataContextProvider();
            var testDataContext = new TestDataContext();
            var testObject = new ClassWithBothNameAndId { Id = 1, Name = "Some name" };
            testDataContext.AddData(new[] { testObject });
            testDataContextProvider.SetDataContext(testDataContext);
            var dataContextCacheService = new DataContextCacheService(testDataContextProvider);
            var result = dataContextCacheService.GetItemById<ClassWithBothNameAndId>(1);
            Assert.AreSame(testObject, result);
        }

        [Test]
        public void GetItemById_ThrowsExceptionWhenMoreThanOneItemWithTheSameIdExist()
        {
            var testDataContextProvider = new TestDataContextProvider();
            var testDataContext = new TestDataContext();
            testDataContext.AddData(new[] { new ClassWithIdAttribute { SomeId = 1 }, new ClassWithIdAttribute { SomeId = 1 } });
            testDataContextProvider.SetDataContext(testDataContext);
            var dataContextCacheService = new DataContextCacheService(testDataContextProvider);
            Assert.Throws<ArgumentException>(() => dataContextCacheService.GetItemById<ClassWithIdAttribute>(1));
        }

        #endregion

        #region GetItemById

        [Test]
        public void GetItemByName_ReturnsNullWhereThereIsNoItemWithSpecifiedName()
        {
            var testDataContextProvider = new TestDataContextProvider();
            var testDataContext = new TestDataContext();
            var testObject = new ClassWithBothNameAndId { Id = 1, Name = "Some name" };
            testDataContext.AddData(new[] { testObject });
            testDataContextProvider.SetDataContext(testDataContext);
            var dataContextCacheService = new DataContextCacheService(testDataContextProvider);
            var result = dataContextCacheService.GetItemByName<ClassWithBothNameAndId>("Other name");
            Assert.IsNull(result);
        }

        [Test]
        public void GetItemByName_ReturnsValidObjectByName()
        {
            var testDataContextProvider = new TestDataContextProvider();
            var testDataContext = new TestDataContext();
            var testObject = new ClassWithBothNameAndId { Id = 1, Name = "Some name" };
            testDataContext.AddData(new[] { testObject });
            testDataContextProvider.SetDataContext(testDataContext);
            var dataContextCacheService = new DataContextCacheService(testDataContextProvider);
            var result = dataContextCacheService.GetItemByName<ClassWithBothNameAndId>("Some name");
            Assert.AreSame(testObject, result);
        }

        [Test]
        public void GetItemByName_ThrowsExceptionWhenMoreThanOneItemWithTheSameNameExist()
        {
            var testDataContextProvider = new TestDataContextProvider();
            var testDataContext = new TestDataContext();
            testDataContext.AddData(new[] { new ClassWithNameAttribute { SomeName = "Some name" }, new ClassWithNameAttribute { SomeName = "Some name" } });
            testDataContextProvider.SetDataContext(testDataContext);
            var dataContextCacheService = new DataContextCacheService(testDataContextProvider);
            Assert.Throws<ArgumentException>(() => dataContextCacheService.GetItemByName<ClassWithNameAttribute>("Some name"));
        }

        #endregion

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

        class ClassWithPrimaryNameProperty
        {
            public string Name { get; set; }
        }

        class ClassWithSecondaryNameProperty
        {
            public string ShortName { get; set; }
        }

        class ClassWithoutAnyName
        {
            public int Id { get; set; }
        }

        class ClassWithBothNameAndId
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        #endregion
    }
}
