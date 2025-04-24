using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Schema;
using EzDbCodeGen.Core.V2.Tests.Mocks;
using EzDbCodeGen.Core.V2.Tests.TestCompat;
using EzDbSchema.Core.V2.Interfaces;
using EzDbSchema.Core.V2.Models;
using Moq;
using Xunit;

namespace EzDbCodeGen.Core.V2.Tests.Schema
{
    public class SchemaDiffTests
    {
        [Fact]
        public void Compare_WithIdenticalSchemas_ShouldReturnNoDifferences()
        {
            // Arrange
            var schema1 = CreateSampleDatabase("TestDb");
            var schema2 = CreateSampleDatabase("TestDb");
            
            var schemaDiff = TestCompatibility.CreateSchemaDiff();
            
            // Act
            var result = schemaDiff.Compare(schema1, schema2);
            
            // Assert
            result.HasChanges.Should().BeFalse();
            result.AddedEntities.Should().BeEmpty();
            result.RemovedEntities.Should().BeEmpty();
            result.ChangedEntities().Should().BeEmpty();
        }
        
        [Fact]
        public void Compare_WithAddedEntity_ShouldDetectAddedEntity()
        {
            // Arrange
            var schema1 = CreateSampleDatabase("TestDb");
            var schema2 = CreateSampleDatabase("TestDb");
            
            // Add a new entity to schema2
            var newEntity = new Entity("NewTable", "dbo", "NewTable", EntityType.Table);
            schema2.AddEntity(newEntity);
            
            var schemaDiff = TestCompatibility.CreateSchemaDiff();
            
            // Act
            var result = schemaDiff.Compare(schema1, schema2);
            
            // Assert
            result.HasChanges.Should().BeTrue();
            result.AddedEntities.Should().HaveCount(1);
            result.AddedEntities.First().Name.Should().Be("NewTable");
            result.RemovedEntities.Should().BeEmpty();
            result.ChangedEntities().Should().BeEmpty();
        }
        
        [Fact]
        public void Compare_WithRemovedEntity_ShouldDetectRemovedEntity()
        {
            // Arrange
            var schema1 = CreateSampleDatabase("TestDb");
            var schema2 = CreateSampleDatabase("TestDb");
            
            // Create a copy of schema1 without one entity
            var entityToRemove = schema1.Entities.Values.First();
            schema2 = new Database(schema2.Name, schema2.DefaultSchema);
            foreach (var entity in schema1.Entities.Values.Where(e => e.Name != entityToRemove.Name))
            {
                schema2.AddEntity(entity);
            }
            
            var schemaDiff = TestCompatibility.CreateSchemaDiff();
            
            // Act
            var result = schemaDiff.Compare(schema1, schema2);
            
            // Assert
            result.HasChanges.Should().BeTrue();
            result.AddedEntities.Should().BeEmpty();
            result.RemovedEntities.Should().HaveCount(1);
            result.RemovedEntities.First().Name.Should().Be(entityToRemove.Name);
            result.ChangedEntities().Should().BeEmpty();
        }
        
        [Fact]
        public void Compare_WithChangedEntity_ShouldDetectChangedEntity()
        {
            // Arrange
            var schema1 = CreateSampleDatabase("TestDb");
            var schema2 = CreateSampleDatabase("TestDb");
            
            // Modify an entity in schema2
            var entityToModify = schema2.Entities.Values.First();
            var newProperty = new Property(
                "NewColumn",
                "NewProperty",
                "varchar",
                "string",
                entityToModify,
                10);
            entityToModify.AddProperty(newProperty);
            
            var schemaDiff = TestCompatibility.CreateSchemaDiff();
            
            // Act
            var result = schemaDiff.Compare(schema1, schema2);
            
            // Assert
            result.HasChanges.Should().BeTrue();
            result.AddedEntities.Should().BeEmpty();
            result.RemovedEntities.Should().BeEmpty();
            result.ChangedEntities().Should().HaveCount(1);
            
            var changedEntity = result.ChangedEntities().First();
            changedEntity.Entity.Name.Should().Be(entityToModify.Name);
            changedEntity.AddedProperties.Should().HaveCount(1);
            changedEntity.AddedProperties.First().Name.Should().Be("NewColumn");
            changedEntity.RemovedProperties.Should().BeEmpty();
            changedEntity.ChangedProperties.Should().BeEmpty();
        }
        
        [Fact]
        public void Compare_WithRemovedProperty_ShouldDetectRemovedProperty()
        {
            // Arrange
            var schema1 = CreateSampleDatabase("TestDb");
            var schema2 = CreateSampleDatabase("TestDb");
            
            // Remove a property from an entity in schema2
            var entityToModify = schema2.Entities.Values.First();
            var propertyToRemove = entityToModify.Properties.Values.First();
            
            // Create a new entity with all properties except the one to remove
            var modifiedEntity = new Entity(
                entityToModify.TableName,
                entityToModify.Schema,
                entityToModify.Alias,
                entityToModify.EntityType);
                
            foreach (var prop in entityToModify.Properties.Values.Where(p => p.Name != propertyToRemove.Name))
            {
                modifiedEntity.AddProperty(prop);
            }
            
            // Replace the entity in schema2
            schema2 = new Database(schema2.Name, schema2.DefaultSchema);
            foreach (var entity in schema1.Entities.Values.Where(e => e.Name != entityToModify.Name))
            {
                schema2.AddEntity(entity);
            }
            schema2.AddEntity(modifiedEntity);
            
            var schemaDiff = TestCompatibility.CreateSchemaDiff();
            
            // Act
            var result = schemaDiff.Compare(schema1, schema2);
            
            // Assert
            result.HasChanges.Should().BeTrue();
            result.AddedEntities.Should().BeEmpty();
            result.RemovedEntities.Should().BeEmpty();
            result.ChangedEntities().Should().HaveCount(1);
            
            var changedEntity = result.ChangedEntities().First();
            changedEntity.Entity.Name.Should().Be(entityToModify.Name);
            changedEntity.AddedProperties.Should().BeEmpty();
            changedEntity.RemovedProperties.Should().HaveCount(1);
            changedEntity.RemovedProperties.First().Name.Should().Be(propertyToRemove.Name);
            changedEntity.ChangedProperties.Should().BeEmpty();
        }
        
        [Fact]
        public void Compare_WithChangedProperty_ShouldDetectChangedProperty()
        {
            // Arrange
            var schema1 = CreateSampleDatabase("TestDb");
            var schema2 = CreateSampleDatabase("TestDb");
            
            // Modify a property in an entity in schema2
            var entityToModify = schema2.Entities.Values.First();
            var propertyToModify = entityToModify.Properties.Values.First();
            
            // Create a modified version of the property
            var modifiedProperty = new Property(
                propertyToModify.ColumnName,
                propertyToModify.PropertyName,
                propertyToModify.DataType,
                propertyToModify.ClrType,
                entityToModify,
                propertyToModify.OrdinalPosition);
                
            // Change some property
            modifiedProperty.MaxLength = (propertyToModify.MaxLength ?? 0) + 100;
            
            // Replace the property in the entity
            entityToModify.Properties.Remove(propertyToModify.Name);
            entityToModify.AddProperty(modifiedProperty);
            
            var schemaDiff = TestCompatibility.CreateSchemaDiff();
            
            // Act
            var result = schemaDiff.Compare(schema1, schema2);
            
            // Assert
            result.HasChanges.Should().BeTrue();
            result.AddedEntities.Should().BeEmpty();
            result.RemovedEntities.Should().BeEmpty();
            result.ChangedEntities().Should().HaveCount(1);
            
            var changedEntity = result.ChangedEntities().First();
            changedEntity.Entity.Name.Should().Be(entityToModify.Name);
            changedEntity.AddedProperties.Should().BeEmpty();
            changedEntity.RemovedProperties.Should().BeEmpty();
            changedEntity.ChangedProperties.Should().HaveCount(1);
            changedEntity.ChangedProperties.First().NewProperty.Name.Should().Be(propertyToModify.Name);
        }
        
        [Fact]
        public void Compare_WithChangedRelationships_ShouldDetectChangedRelationships()
        {
            // Arrange
            var schema1 = CreateSampleDatabaseWithRelationships("TestDb");
            var schema2 = CreateSampleDatabaseWithRelationships("TestDb");
            
            // Add a new relationship to schema2
            var parentEntity = schema2.Entities.Values.First();
            var childEntity = schema2.Entities.Values.Last();
            
            var newRelationship = new Relationship(
                "FK_New_Relationship",
                parentEntity,
                childEntity);
                
            var parentProperty = parentEntity.Properties.Values.First(p => p.IsPrimaryKey);
            var childProperty = childEntity.Properties.Values.First(p => !p.IsPrimaryKey);
            
            newRelationship.AddPropertyPair(parentProperty, childProperty);
            parentEntity.AddChildRelationship(newRelationship);
            childEntity.AddParentRelationship(newRelationship);
            
            var schemaDiff = TestCompatibility.CreateSchemaDiff();
            
            // Act
            var result = schemaDiff.Compare(schema1, schema2);
            
            // Assert
            result.HasChanges.Should().BeTrue();
            result.AddedRelationships.Should().HaveCount(1);
            result.AddedRelationships.First().Name.Should().Be("FK_New_Relationship");
            result.RemovedRelationships.Should().BeEmpty();
            result.ChangedRelationships.Should().BeEmpty();
        }
        
        [Fact]
        public void Compare_WithRemovedRelationship_ShouldDetectRemovedRelationship()
        {
            // Arrange
            var schema1 = CreateSampleDatabaseWithRelationships("TestDb");
            var schema2 = CreateSampleDatabaseWithoutRelationships("TestDb");
            
            var schemaDiff = TestCompatibility.CreateSchemaDiff();
            
            // Act
            var result = schemaDiff.Compare(schema1, schema2);
            
            // Assert
            result.HasChanges.Should().BeTrue();
            result.AddedRelationships.Should().BeEmpty();
            result.RemovedRelationships.Should().NotBeEmpty();
        }
        
        [Fact]
        public void Compare_WithNullSourceSchema_ShouldThrowArgumentNullException()
        {
            // Arrange
            var schema = CreateSampleDatabase("TestDb");
            var schemaDiff = TestCompatibility.CreateSchemaDiff();
            
            // Act & Assert
            Action act = () => schemaDiff.Compare(null!, schema);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("sourceSchema");
        }
        
        [Fact]
        public void Compare_WithNullTargetSchema_ShouldThrowArgumentNullException()
        {
            // Arrange
            var schema = CreateSampleDatabase("TestDb");
            var schemaDiff = TestCompatibility.CreateSchemaDiff();
            
            // Act & Assert
            Action act = () => schemaDiff.Compare(schema, null!);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("targetSchema");
        }
        
        // Helper methods to create test databases
        private IDatabase CreateSampleDatabase(string name)
        {
            var database = new Database(name, "dbo");
            
            // Create first entity
            var entity1 = new Entity("Customer", "dbo", "Customer", EntityType.Table);
            var property1_1 = new Property("CustomerId", "Id", "int", "int", entity1, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var property1_2 = new Property("CustomerName", "Name", "varchar", "string", entity1, 2)
            {
                MaxLength = 100
            };
            entity1.AddProperty(property1_1);
            entity1.AddProperty(property1_2);
            
            // Create second entity
            var entity2 = new Entity("Order", "dbo", "Order", EntityType.Table);
            var property2_1 = new Property("OrderId", "Id", "int", "int", entity2, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var property2_2 = new Property("OrderDate", "Date", "datetime", "DateTime", entity2, 2);
            entity2.AddProperty(property2_1);
            entity2.AddProperty(property2_2);
            
            database.AddEntity(entity1);
            database.AddEntity(entity2);
            
            return database;
        }

        private IDatabase CreateSampleDatabaseWithRelationships(string name)
        {
            var database = CreateSampleDatabase(name);
            
            // Get the entities
            var customerEntity = database.GetEntity("Customer");
            var orderEntity = database.GetEntity("Order");
            
            // Add CustomerId to Order
            var customerIdProperty = new Property("CustomerId", "CustomerId", "int", "int", orderEntity!, 3)
            {
                IsForeignKey = true
            };
            orderEntity!.AddProperty(customerIdProperty);
            
            // Create relationship
            var relationship = new Relationship(
                "FK_Order_Customer",
                customerEntity!,
                orderEntity);
                
            relationship.AddPropertyPair(
                customerEntity!.GetProperty("CustomerId")!,
                customerIdProperty);
                
            customerEntity.AddChildRelationship(relationship);
            orderEntity.AddParentRelationship(relationship);
            
            return database;
        }
        
        private IDatabase CreateSampleDatabaseWithoutRelationships(string name)
        {
            var database = CreateSampleDatabase(name);
            
            // Get the order entity and add CustomerId but without relationship
            var orderEntity = database.GetEntity("Order");
            var customerIdProperty = new Property("CustomerId", "CustomerId", "int", "int", orderEntity!, 3);
            orderEntity!.AddProperty(customerIdProperty);
            
            return database;
        }
    }
}
