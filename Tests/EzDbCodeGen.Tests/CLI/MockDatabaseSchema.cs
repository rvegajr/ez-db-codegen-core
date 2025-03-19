using System;
using System.Collections.Generic;
using System.Linq;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using Moq;

namespace EzDbCodeGen.Tests.CLI
{
    /// <summary>
    /// A mock database schema for testing
    /// </summary>
    public class MockDatabaseSchema
    {
        private readonly IDatabase _database;

        /// <summary>
        /// Creates a new instance of the MockDatabaseSchema class
        /// </summary>
        public MockDatabaseSchema()
        {
            _database = InitializeMockDatabase();
        }

        /// <summary>
        /// Gets the underlying database
        /// </summary>
        public IDatabase Database => _database;

        /// <summary>
        /// Creates a mock database for testing
        /// </summary>
        /// <returns>A mock IDatabase instance</returns>
        public static IDatabase CreateMockDatabase()
        {
            var mockDbSchema = new MockDatabaseSchema();
            return mockDbSchema.Database;
        }

        private IDatabase InitializeMockDatabase()
        {
            // Create mock entities
            var customerEntity = CreateCustomerEntity();
            var orderEntity = CreateOrderEntity();

            // Setup relationships
            SetupRelationships(customerEntity, orderEntity);

            // Create a dictionary of entities
            var entities = new Dictionary<string, IEntity>
            {
                { "Customer", customerEntity },
                { "Order", orderEntity }
            };

            // Create mock entity dictionary
            var mockEntityDict = new Mock<IEntityDictionary>();
            
            // Setup basic properties
            mockEntityDict.Setup(e => e.DatabaseObjectName).Returns("TestEntities");
            mockEntityDict.Setup(e => e._id).Returns(1);
            mockEntityDict.Setup(e => e.IsEnabled).Returns(true);
            mockEntityDict.Setup(e => e.CustomAttributes).Returns(Mock.Of<ICustomAttributes>());
            
            // Setup dictionary methods
            mockEntityDict.Setup(e => e.ContainsKey(It.IsAny<string>()))
                .Returns<string>(key => entities.ContainsKey(key));
            
            mockEntityDict.Setup(e => e[It.IsAny<string>()])
                .Returns<string>(key => entities.ContainsKey(key) ? entities[key] : null);
            
            mockEntityDict.Setup(e => e.Keys).Returns(entities.Keys);
            mockEntityDict.Setup(e => e.Values).Returns(entities.Values);
            mockEntityDict.Setup(e => e.Count).Returns(entities.Count);
            mockEntityDict.Setup(e => e.IsReadOnly).Returns(false);
            mockEntityDict.Setup(e => e.GetEnumerator()).Returns(entities.GetEnumerator());
            
            // Setup TryGetValue
            foreach (var key in entities.Keys)
            {
                var entity = entities[key];
                mockEntityDict.Setup(e => e.TryGetValue(key, out It.Ref<IEntity>.IsAny))
                    .Callback(new OutRefAction<string, IEntity>((string k, out IEntity v) => v = entity))
                    .Returns(true);
            }
            
            // Create mock database
            var mockDb = new Mock<IDatabase>();
            
            // Setup basic properties
            mockDb.Setup(db => db.DatabaseObjectName).Returns("TestDatabase");
            mockDb.Setup(db => db._id).Returns(1);
            mockDb.Setup(db => db.IsEnabled).Returns(true);
            mockDb.Setup(db => db.CustomAttributes).Returns(Mock.Of<ICustomAttributes>());
            
            // Setup database properties
            mockDb.Setup(db => db.Name).Returns("TestDatabase");
            mockDb.Setup(db => db.DefaultSchema).Returns("dbo");
            mockDb.Setup(db => db.ShowWarnings).Returns(false);
            mockDb.Setup(db => db.AutoAddPrimaryKeys).Returns(false);
            mockDb.Setup(db => db.Entities).Returns(mockEntityDict.Object);
            mockDb.Setup(db => db.LastUpdates).Returns(Mock.Of<IDatabaseObjectUpdates>());
            
            // Setup dictionary methods
            mockDb.Setup(db => db.ContainsKey(It.IsAny<string>()))
                .Returns<string>(key => entities.ContainsKey(key));
            
            mockDb.Setup(db => db[It.IsAny<string>()])
                .Returns<string>(key => entities.ContainsKey(key) ? entities[key] : null);
            
            mockDb.Setup(db => db.Keys).Returns(Mock.Of<IEntityNameList>());
            mockDb.Setup(db => db.Values).Returns(entities.Values.AsEnumerable());
            mockDb.Setup(db => db.Count).Returns(entities.Count);
            mockDb.Setup(db => db.GetEnumerator()).Returns(entities.GetEnumerator());
            
            // Setup TryGetValue
            foreach (var key in entities.Keys)
            {
                var entity = entities[key];
                mockDb.Setup(db => db.TryGetValue(key, out It.Ref<IEntity>.IsAny))
                    .Callback(new OutRefAction<string, IEntity>((string k, out IEntity v) => v = entity))
                    .Returns(true);
            }
            
            // Setup other methods
            mockDb.Setup(db => db.ContainsValue(It.IsAny<IEntity>()))
                .Returns<IEntity>(entity => entities.Values.Contains(entity));
            
            mockDb.Setup(db => db.Render(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockDb.Object);
            
            mockDb.Setup(db => db.AsJson()).Returns("{}");

            return mockDb.Object;
        }

        private static IEntity CreateCustomerEntity()
        {
            var mockEntity = new Mock<IEntity>();
            var properties = CreateMockPropertyDictionary();

            // Create properties
            var idProperty = CreateProperty("CustomerId", "int", true, false, false);
            var nameProperty = CreateProperty("Name", "nvarchar", false, false, false, 100);
            var emailProperty = CreateProperty("Email", "nvarchar", false, false, true, 255);

            // Add properties to dictionary
            properties.Add("CustomerId", idProperty);
            properties.Add("Name", nameProperty);
            properties.Add("Email", emailProperty);

            // Setup entity
            mockEntity.Setup(e => e.TableName).Returns("Customer");
            mockEntity.Setup(e => e.DatabaseSchema).Returns("dbo");
            mockEntity.Setup(e => e.Properties).Returns(properties);
            mockEntity.Setup(e => e.DatabaseObjectName).Returns("Customer");
            mockEntity.Setup(e => e._id).Returns(1);
            mockEntity.Setup(e => e.IsEnabled).Returns(true);
            mockEntity.Setup(e => e.CustomAttributes).Returns(Mock.Of<ICustomAttributes>());

            return mockEntity.Object;
        }

        private static IEntity CreateOrderEntity()
        {
            var mockEntity = new Mock<IEntity>();
            var properties = CreateMockPropertyDictionary();

            // Create properties
            var idProperty = CreateProperty("OrderId", "int", true, false, false);
            var customerIdProperty = CreateProperty("CustomerId", "int", false, true, false);
            var dateProperty = CreateProperty("OrderDate", "datetime", false, false, false);
            var totalProperty = CreateProperty("Total", "decimal", false, false, false, 0, 2, 10);

            // Add properties to dictionary
            properties.Add("OrderId", idProperty);
            properties.Add("CustomerId", customerIdProperty);
            properties.Add("OrderDate", dateProperty);
            properties.Add("Total", totalProperty);

            // Setup entity
            mockEntity.Setup(e => e.TableName).Returns("Order");
            mockEntity.Setup(e => e.DatabaseSchema).Returns("dbo");
            mockEntity.Setup(e => e.Properties).Returns(properties);
            mockEntity.Setup(e => e.DatabaseObjectName).Returns("Order");
            mockEntity.Setup(e => e._id).Returns(1);
            mockEntity.Setup(e => e.IsEnabled).Returns(true);
            mockEntity.Setup(e => e.CustomAttributes).Returns(Mock.Of<ICustomAttributes>());

            return mockEntity.Object;
        }

        private static IProperty CreateProperty(
            string name,
            string dataType,
            bool isPrimaryKey,
            bool isForeignKey,
            bool isNullable,
            int maxLength = 0,
            int scale = 0,
            int precision = 0)
        {
            var mockProperty = new Mock<IProperty>();

            mockProperty.Setup(p => p.PropertyName).Returns(name);
            mockProperty.Setup(p => p.ColumnName).Returns(name);
            mockProperty.Setup(p => p.DataType).Returns(dataType);
            mockProperty.Setup(p => p.IsPrimaryKey).Returns(isPrimaryKey);
            
            // Setup foreign key relationship if needed
            var mockRelationshipList = new Mock<IRelationshipList>();
            mockProperty.Setup(p => p.RelatedTo).Returns(mockRelationshipList.Object);
            
            mockProperty.Setup(p => p.IsNullable).Returns(isNullable);
            mockProperty.Setup(p => p.MaxLength).Returns(maxLength);
            mockProperty.Setup(p => p.Scale).Returns(scale);
            mockProperty.Setup(p => p.Precision).Returns(precision);
            mockProperty.Setup(p => p.DatabaseObjectName).Returns(name);
            mockProperty.Setup(p => p._id).Returns(1);
            mockProperty.Setup(p => p.IsEnabled).Returns(true);
            mockProperty.Setup(p => p.CustomAttributes).Returns(Mock.Of<ICustomAttributes>());

            return mockProperty.Object;
        }

        private static IPropertyDictionary CreateMockPropertyDictionary()
        {
            var mockPropertyDict = new Mock<IPropertyDictionary>();
            var properties = new Dictionary<string, IProperty>();

            // Setup dictionary methods
            mockPropertyDict.Setup(pd => pd.ContainsKey(It.IsAny<string>()))
                .Returns<string>(key => properties.ContainsKey(key));

            mockPropertyDict.Setup(pd => pd.Add(It.IsAny<string>(), It.IsAny<IProperty>()))
                .Callback<string, IProperty>((key, value) => properties.Add(key, value));

            mockPropertyDict.Setup(pd => pd[It.IsAny<string>()])
                .Returns<string>(key => properties.ContainsKey(key) ? properties[key] : null);

            mockPropertyDict.Setup(pd => pd.GetEnumerator())
                .Returns(() => properties.GetEnumerator());
                
            mockPropertyDict.Setup(pd => pd.DatabaseObjectName).Returns("Properties");
            mockPropertyDict.Setup(pd => pd._id).Returns(1);
            mockPropertyDict.Setup(pd => pd.IsEnabled).Returns(true);
            mockPropertyDict.Setup(pd => pd.CustomAttributes).Returns(Mock.Of<ICustomAttributes>());

            return mockPropertyDict.Object;
        }

        private static void SetupRelationships(IEntity customerEntity, IEntity orderEntity)
        {
            // In a real implementation, we would set up the relationships between entities
        }
        
        // Helper delegate for out parameters in callbacks
        public delegate void OutRefAction<T1, T2>(T1 arg1, out T2 arg2);
    }
}
