using System;
using System.Collections.Generic;
using EzDbSchema.Core.Enums;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using Moq;
using EzDbSchema.Core.Extentions;

namespace EzDbCodeGen.Tests.EFCore
{
    /// <summary>
    /// Mock database schema for testing
    /// </summary>
    public static class MockDatabaseSchema
    {
        private static readonly object _lock = new object();
        private static IDatabase _mockDatabase;
        private static IDatabase _mockDatabaseWithRelationships;
        private static IDatabase _mockDatabaseWithNullableProperties;
        
        /// <summary>
        /// Creates a mock database with basic Customer and Order entities
        /// </summary>
        /// <returns>A mock database</returns>
        public static IDatabase CreateMockDatabase()
        {
            if (_mockDatabase != null)
                return _mockDatabase;
                
            lock (_lock)
            {
                if (_mockDatabase != null)
                    return _mockDatabase;
                
                // Create a mock database
                var database = new Mock<IDatabase>();
                var entities = new Dictionary<string, IEntity>();
                
                // Create Customer entity
                var customerEntity = new Mock<IEntity>();
                var customerProperties = new PropertyDictionary();
                
                // Add Customer properties
                var customerIdProperty = new Mock<IProperty>();
                customerIdProperty.Setup(p => p.PropertyName).Returns("CustomerId");
                customerIdProperty.Setup(p => p.IsPrimaryKey).Returns(true);
                customerIdProperty.Setup(p => p.IsNullable).Returns(false);
                customerIdProperty.Setup(p => p.DataType).Returns("int");
                customerProperties.Add("CustomerId", customerIdProperty.Object);
                
                var customerNameProperty = new Mock<IProperty>();
                customerNameProperty.Setup(p => p.PropertyName).Returns("CustomerName");
                customerNameProperty.Setup(p => p.IsPrimaryKey).Returns(false);
                customerNameProperty.Setup(p => p.IsNullable).Returns(false);
                customerNameProperty.Setup(p => p.DataType).Returns("string");
                customerProperties.Add("CustomerName", customerNameProperty.Object);
                
                // Setup Customer entity
                customerEntity.Setup(e => e.TableName).Returns("Customer");
                customerEntity.Setup(e => e.Properties).Returns(customerProperties);
                customerEntity.Setup(e => e.DatabaseSchema).Returns("dbo");
                
                // Create Order entity
                var orderEntity = new Mock<IEntity>();
                var orderProperties = new PropertyDictionary();
                
                // Add Order properties
                var orderIdProperty = new Mock<IProperty>();
                orderIdProperty.Setup(p => p.PropertyName).Returns("OrderId");
                orderIdProperty.Setup(p => p.IsPrimaryKey).Returns(true);
                orderIdProperty.Setup(p => p.IsNullable).Returns(false);
                orderIdProperty.Setup(p => p.DataType).Returns("int");
                orderProperties.Add("OrderId", orderIdProperty.Object);
                
                var orderDateProperty = new Mock<IProperty>();
                orderDateProperty.Setup(p => p.PropertyName).Returns("OrderDate");
                orderDateProperty.Setup(p => p.IsPrimaryKey).Returns(false);
                orderDateProperty.Setup(p => p.IsNullable).Returns(false);
                orderDateProperty.Setup(p => p.DataType).Returns("DateTime");
                orderProperties.Add("OrderDate", orderDateProperty.Object);
                
                var customerIdFkProperty = new Mock<IProperty>();
                customerIdFkProperty.Setup(p => p.PropertyName).Returns("CustomerId");
                customerIdFkProperty.Setup(p => p.IsPrimaryKey).Returns(false);
                customerIdFkProperty.Setup(p => p.IsNullable).Returns(false);
                customerIdFkProperty.Setup(p => p.DataType).Returns("int");
                
                // Add a custom attribute to indicate this property is a foreign key
                var customAttributes = new CustomAttributes();
                customAttributes.Add("IsForeignKey", true);
                customerIdFkProperty.Setup(p => p.CustomAttributes).Returns(customAttributes);
                
                orderProperties.Add("CustomerId", customerIdFkProperty.Object);
                
                // Setup Order entity
                orderEntity.Setup(e => e.TableName).Returns("Order");
                orderEntity.Setup(e => e.Properties).Returns(orderProperties);
                orderEntity.Setup(e => e.DatabaseSchema).Returns("dbo");
                
                // Create relationships
                var customerToOrderRelationship = new Mock<IRelationship>();
                customerToOrderRelationship.Setup(r => r.FromTableName).Returns("Customer");
                customerToOrderRelationship.Setup(r => r.ToTableName).Returns("Order");
                customerToOrderRelationship.Setup(r => r.RelationshipType).Returns(RelationshipMultiplicityType.OneToMany.ToString());
                customerToOrderRelationship.Setup(r => r.MultiplicityType).Returns(RelationshipMultiplicityType.OneToMany);
                customerToOrderRelationship.Setup(r => r.FromEntity).Returns(customerEntity.Object);
                customerToOrderRelationship.Setup(r => r.ToEntity).Returns(orderEntity.Object);
                customerToOrderRelationship.Setup(r => r.FromProperty).Returns(customerIdProperty.Object);
                customerToOrderRelationship.Setup(r => r.ToProperty).Returns(customerIdFkProperty.Object);
                
                var orderToCustomerRelationship = new Mock<IRelationship>();
                orderToCustomerRelationship.Setup(r => r.FromTableName).Returns("Order");
                orderToCustomerRelationship.Setup(r => r.ToTableName).Returns("Customer");
                orderToCustomerRelationship.Setup(r => r.RelationshipType).Returns(RelationshipMultiplicityType.ManyToOne.ToString());
                orderToCustomerRelationship.Setup(r => r.MultiplicityType).Returns(RelationshipMultiplicityType.ManyToOne);
                orderToCustomerRelationship.Setup(r => r.FromEntity).Returns(orderEntity.Object);
                orderToCustomerRelationship.Setup(r => r.ToEntity).Returns(customerEntity.Object);
                orderToCustomerRelationship.Setup(r => r.FromProperty).Returns(customerIdFkProperty.Object);
                orderToCustomerRelationship.Setup(r => r.ToProperty).Returns(customerIdProperty.Object);
                
                var customerRelationships = new RelationshipReferenceList();
                customerRelationships.Add(customerToOrderRelationship.Object);
                customerEntity.Setup(e => e.Relationships).Returns(customerRelationships);
                
                var orderRelationships = new RelationshipReferenceList();
                orderRelationships.Add(orderToCustomerRelationship.Object);
                orderEntity.Setup(e => e.Relationships).Returns(orderRelationships);
                
                // Add entities to database
                entities.Add("Customer", customerEntity.Object);
                entities.Add("Order", orderEntity.Object);
                
                // Setup the Entities property
                var entityDictionary = new EntityDictionary();
                foreach (var entity in entities)
                {
                    entityDictionary.Add(entity.Key, entity.Value);
                }
                database.Setup(db => db.Entities).Returns(entityDictionary);
                database.Setup(db => db.Count).Returns(entities.Count);
                
                _mockDatabase = database.Object;
                return _mockDatabase;
            }
        }
        
        /// <summary>
        /// Creates a mock database with nullable properties
        /// </summary>
        /// <returns>A mock database with nullable properties</returns>
        public static IDatabase CreateMockDatabaseWithNullableProperties()
        {
            if (_mockDatabaseWithNullableProperties != null)
                return _mockDatabaseWithNullableProperties;
                
            lock (_lock)
            {
                if (_mockDatabaseWithNullableProperties != null)
                    return _mockDatabaseWithNullableProperties;
                
                var database = new Mock<IDatabase>();
                var entities = new Dictionary<string, IEntity>();
                
                // Create Customer entity
                var customerEntity = new Mock<IEntity>();
                var customerProperties = new PropertyDictionary();
                
                // Add Customer properties
                var customerIdProperty = new Mock<IProperty>();
                customerIdProperty.Setup(p => p.PropertyName).Returns("CustomerId");
                customerIdProperty.Setup(p => p.IsPrimaryKey).Returns(true);
                customerIdProperty.Setup(p => p.IsNullable).Returns(false);
                customerIdProperty.Setup(p => p.DataType).Returns("int");
                customerProperties.Add("CustomerId", customerIdProperty.Object);
                
                var customerNameProperty = new Mock<IProperty>();
                customerNameProperty.Setup(p => p.PropertyName).Returns("CustomerName");
                customerNameProperty.Setup(p => p.IsPrimaryKey).Returns(false);
                customerNameProperty.Setup(p => p.IsNullable).Returns(false);
                customerNameProperty.Setup(p => p.DataType).Returns("string");
                customerProperties.Add("CustomerName", customerNameProperty.Object);
                
                var customerEmailProperty = new Mock<IProperty>();
                customerEmailProperty.Setup(p => p.PropertyName).Returns("CustomerEmail");
                customerEmailProperty.Setup(p => p.IsPrimaryKey).Returns(false);
                customerEmailProperty.Setup(p => p.IsNullable).Returns(true); // Make this nullable
                customerEmailProperty.Setup(p => p.DataType).Returns("string");
                customerProperties.Add("CustomerEmail", customerEmailProperty.Object);
                
                // Setup Customer entity
                customerEntity.Setup(e => e.TableName).Returns("Customer");
                customerEntity.Setup(e => e.Properties).Returns(customerProperties);
                customerEntity.Setup(e => e.DatabaseSchema).Returns("dbo");
                customerEntity.Setup(e => e.Relationships).Returns(new RelationshipReferenceList());
                
                // Add entities to database
                entities.Add("Customer", customerEntity.Object);
                
                // Setup the Entities property
                var entityDictionary = new EntityDictionary();
                foreach (var entity in entities)
                {
                    entityDictionary.Add(entity.Key, entity.Value);
                }
                database.Setup(db => db.Entities).Returns(entityDictionary);
                database.Setup(db => db.Count).Returns(entities.Count);
                
                _mockDatabaseWithNullableProperties = database.Object;
                return _mockDatabaseWithNullableProperties;
            }
        }
        
        /// <summary>
        /// Creates a mock database with casing variations in entity names
        /// </summary>
        /// <returns>A mock database with casing variations</returns>
        public static IDatabase CreateMockDatabaseWithCasingVariations()
        {
            // Create a mock database
            var database = new Mock<IDatabase>();
            var entities = new Dictionary<string, IEntity>();
            
            // Create customer entity (lowercase)
            var customerEntity = new Mock<IEntity>();
            var customerProperties = new PropertyDictionary();
            
            // Add properties to customer entity
            var customerIdProperty = new Mock<IProperty>();
            customerIdProperty.Setup(p => p.PropertyName).Returns("customerId");
            customerIdProperty.Setup(p => p.IsPrimaryKey).Returns(true);
            customerIdProperty.Setup(p => p.DataType).Returns("int");
            customerProperties.Add("customerId", customerIdProperty.Object);
            
            var customerNameProperty = new Mock<IProperty>();
            customerNameProperty.Setup(p => p.PropertyName).Returns("customerName");
            customerNameProperty.Setup(p => p.IsPrimaryKey).Returns(false);
            customerNameProperty.Setup(p => p.DataType).Returns("string");
            customerProperties.Add("customerName", customerNameProperty.Object);
            
            // Setup customer entity
            customerEntity.Setup(e => e.TableName).Returns("customer");
            customerEntity.Setup(e => e.Properties).Returns(customerProperties);
            customerEntity.Setup(e => e.DatabaseSchema).Returns("dbo");
            customerEntity.Setup(e => e.Relationships).Returns(new RelationshipReferenceList());
            
            // Create PRODUCT entity (uppercase)
            var productEntity = new Mock<IEntity>();
            var productProperties = new PropertyDictionary();
            
            // Add properties to PRODUCT entity
            var productIdProperty = new Mock<IProperty>();
            productIdProperty.Setup(p => p.PropertyName).Returns("PRODUCT_ID");
            productIdProperty.Setup(p => p.IsPrimaryKey).Returns(true);
            productIdProperty.Setup(p => p.DataType).Returns("int");
            productProperties.Add("PRODUCT_ID", productIdProperty.Object);
            
            var productNameProperty = new Mock<IProperty>();
            productNameProperty.Setup(p => p.PropertyName).Returns("PRODUCT_NAME");
            productNameProperty.Setup(p => p.IsPrimaryKey).Returns(false);
            productNameProperty.Setup(p => p.DataType).Returns("string");
            productProperties.Add("PRODUCT_NAME", productNameProperty.Object);
            
            // Setup PRODUCT entity
            productEntity.Setup(e => e.TableName).Returns("PRODUCT");
            productEntity.Setup(e => e.Properties).Returns(productProperties);
            productEntity.Setup(e => e.DatabaseSchema).Returns("dbo");
            productEntity.Setup(e => e.Relationships).Returns(new RelationshipReferenceList());
            
            // Add entities to database
            entities.Add("customer", customerEntity.Object);
            entities.Add("PRODUCT", productEntity.Object);
            
            // Setup the Entities property
            var entityDictionary = new EntityDictionary();
            foreach (var entity in entities)
            {
                entityDictionary.Add(entity.Key, entity.Value);
            }
            database.Setup(db => db.Entities).Returns(entityDictionary);
            
            return database.Object;
        }
        
        /// <summary>
        /// Creates a mock database with relationships
        /// </summary>
        /// <returns>A mock database with relationships</returns>
        public static IDatabase CreateMockDatabaseWithRelationships()
        {
            if (_mockDatabaseWithRelationships != null)
                return _mockDatabaseWithRelationships;
                
            lock (_lock)
            {
                if (_mockDatabaseWithRelationships != null)
                    return _mockDatabaseWithRelationships;
                
                // Create a mock database
                var database = new Mock<IDatabase>();
                var entities = new Dictionary<string, IEntity>();
                
                // Create Customer entity
                var customerEntity = new Mock<IEntity>();
                var customerProperties = new PropertyDictionary();
                
                // Add properties to Customer entity
                var customerIdProperty = new Mock<IProperty>();
                customerIdProperty.Setup(p => p.PropertyName).Returns("CustomerId");
                customerIdProperty.Setup(p => p.IsPrimaryKey).Returns(true);
                customerIdProperty.Setup(p => p.DataType).Returns("int");
                customerProperties.Add("CustomerId", customerIdProperty.Object);
                
                var customerNameProperty = new Mock<IProperty>();
                customerNameProperty.Setup(p => p.PropertyName).Returns("CustomerName");
                customerNameProperty.Setup(p => p.IsPrimaryKey).Returns(false);
                customerNameProperty.Setup(p => p.DataType).Returns("string");
                customerProperties.Add("CustomerName", customerNameProperty.Object);
                
                var customerEmailProperty = new Mock<IProperty>();
                customerEmailProperty.Setup(p => p.PropertyName).Returns("CustomerEmail");
                customerEmailProperty.Setup(p => p.IsPrimaryKey).Returns(false);
                customerEmailProperty.Setup(p => p.DataType).Returns("string");
                customerProperties.Add("CustomerEmail", customerEmailProperty.Object);
                
                // Setup Customer entity
                customerEntity.Setup(e => e.TableName).Returns("Customer");
                customerEntity.Setup(e => e.Properties).Returns(customerProperties);
                customerEntity.Setup(e => e.DatabaseSchema).Returns("dbo");
                
                // Setup relationships for Customer
                var customerRelationships = new RelationshipReferenceList();
                customerEntity.Setup(e => e.Relationships).Returns(customerRelationships);
                
                // Create Order entity
                var orderEntity = new Mock<IEntity>();
                var orderProperties = new PropertyDictionary();
                
                // Add properties to Order entity
                var orderIdProperty = new Mock<IProperty>();
                orderIdProperty.Setup(p => p.PropertyName).Returns("OrderId");
                orderIdProperty.Setup(p => p.IsPrimaryKey).Returns(true);
                orderIdProperty.Setup(p => p.DataType).Returns("int");
                orderProperties.Add("OrderId", orderIdProperty.Object);
                
                var orderDateProperty = new Mock<IProperty>();
                orderDateProperty.Setup(p => p.PropertyName).Returns("OrderDate");
                orderDateProperty.Setup(p => p.IsPrimaryKey).Returns(false);
                orderDateProperty.Setup(p => p.DataType).Returns("DateTime");
                orderProperties.Add("OrderDate", orderDateProperty.Object);
                
                var customerIdFkProperty = new Mock<IProperty>();
                customerIdFkProperty.Setup(p => p.PropertyName).Returns("CustomerId");
                customerIdFkProperty.Setup(p => p.IsPrimaryKey).Returns(false);
                customerIdFkProperty.Setup(p => p.DataType).Returns("int");
                
                // Add a custom attribute to indicate this property is a foreign key
                var customAttributes = new CustomAttributes();
                customAttributes.Add("IsForeignKey", true);
                customerIdFkProperty.Setup(p => p.CustomAttributes).Returns(customAttributes);
                
                orderProperties.Add("CustomerId", customerIdFkProperty.Object);
                
                // Setup Order entity
                orderEntity.Setup(e => e.TableName).Returns("Order");
                orderEntity.Setup(e => e.Properties).Returns(orderProperties);
                orderEntity.Setup(e => e.DatabaseSchema).Returns("dbo");
                
                // Create relationships
                var customerToOrderRelationship = new Mock<IRelationship>();
                customerToOrderRelationship.Setup(r => r.FromTableName).Returns("Customer");
                customerToOrderRelationship.Setup(r => r.ToTableName).Returns("Order");
                customerToOrderRelationship.Setup(r => r.RelationshipType).Returns(RelationshipMultiplicityType.OneToMany.ToString());
                customerToOrderRelationship.Setup(r => r.MultiplicityType).Returns(RelationshipMultiplicityType.OneToMany);
                customerToOrderRelationship.Setup(r => r.FromEntity).Returns(customerEntity.Object);
                customerToOrderRelationship.Setup(r => r.ToEntity).Returns(orderEntity.Object);
                customerToOrderRelationship.Setup(r => r.FromProperty).Returns(customerIdProperty.Object);
                customerToOrderRelationship.Setup(r => r.ToProperty).Returns(customerIdFkProperty.Object);
                
                var orderToCustomerRelationship = new Mock<IRelationship>();
                orderToCustomerRelationship.Setup(r => r.FromTableName).Returns("Order");
                orderToCustomerRelationship.Setup(r => r.ToTableName).Returns("Customer");
                orderToCustomerRelationship.Setup(r => r.RelationshipType).Returns(RelationshipMultiplicityType.ManyToOne.ToString());
                orderToCustomerRelationship.Setup(r => r.MultiplicityType).Returns(RelationshipMultiplicityType.ManyToOne);
                orderToCustomerRelationship.Setup(r => r.FromEntity).Returns(orderEntity.Object);
                orderToCustomerRelationship.Setup(r => r.ToEntity).Returns(customerEntity.Object);
                orderToCustomerRelationship.Setup(r => r.FromProperty).Returns(customerIdFkProperty.Object);
                orderToCustomerRelationship.Setup(r => r.ToProperty).Returns(customerIdProperty.Object);
                
                var customerRelationshipsList = new RelationshipReferenceList();
                customerRelationshipsList.Add(customerToOrderRelationship.Object);
                customerEntity.Setup(e => e.Relationships).Returns(customerRelationshipsList);
                
                var orderRelationshipsList = new RelationshipReferenceList();
                orderRelationshipsList.Add(orderToCustomerRelationship.Object);
                orderEntity.Setup(e => e.Relationships).Returns(orderRelationshipsList);
                
                // Add entities to database
                entities.Add("Customer", customerEntity.Object);
                entities.Add("Order", orderEntity.Object);
                
                // Setup the Entities property
                var entityDictionary = new EntityDictionary();
                foreach (var entity in entities)
                {
                    entityDictionary.Add(entity.Key, entity.Value);
                }
                database.Setup(db => db.Entities).Returns(entityDictionary);
                
                _mockDatabaseWithRelationships = database.Object;
                return _mockDatabaseWithRelationships;
            }
        }
    }
}
