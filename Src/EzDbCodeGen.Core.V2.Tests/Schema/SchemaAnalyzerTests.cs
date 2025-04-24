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
    public class SchemaAnalyzerTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithNoOptions()
        {
            // Arrange & Act
            var analyzer = TestCompatibility.CreateSchemaAnalyzer();

            // Assert
            analyzer.Should().NotBeNull();
        }

        [Fact]
        public void AnalyzeSchema_WithNullSchema_ShouldThrowArgumentNullException()
        {
            // Arrange
            var analyzer = TestCompatibility.CreateSchemaAnalyzer();

            // Act & Assert
            Action act = () => analyzer.AnalyzeSchema(null!);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("schema");
        }

        [Fact]
        public void AnalyzeSchema_ShouldIdentifyStandardRelationships()
        {
            // Arrange
            var analyzer = TestCompatibility.CreateSchemaAnalyzer();
            var database = CreateDatabaseWithStandardRelationships();

            // Act
            var result = analyzer.AnalyzeSchema(database);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(database); // Should return the same object

            // Check that relationships were identified
            var customerEntity = database.GetEntity("Customer");
            var orderEntity = database.GetEntity("Order");
            
            customerEntity!.ChildRelationships.Should().HaveCount(1);
            orderEntity!.ParentRelationships.Should().HaveCount(1);
            
            var relationship = customerEntity.ChildRelationships.First();
            relationship.ParentEntity.Should().BeSameAs(customerEntity);
            relationship.ChildEntity.Should().BeSameAs(orderEntity);
            relationship.IsOneToMany.Should().BeTrue();
            relationship.IsOneToOne.Should().BeFalse();
        }

        [Fact]
        public void AnalyzeSchema_ShouldIdentifyOneToOneRelationships()
        {
            // Arrange
            var analyzer = TestCompatibility.CreateSchemaAnalyzer();
            var database = CreateDatabaseWithOneToOneRelationship();

            // Act
            var result = analyzer.AnalyzeSchema(database);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(database); // Should return the same object

            // Check that the one-to-one relationship was identified
            var customerEntity = database.GetEntity("Customer");
            var profileEntity = database.GetEntity("CustomerProfile");
            
            customerEntity!.ChildRelationships.Should().HaveCount(1);
            profileEntity!.ParentRelationships.Should().HaveCount(1);
            
            var relationship = customerEntity.ChildRelationships.First();
            relationship.ParentEntity.Should().BeSameAs(customerEntity);
            relationship.ChildEntity.Should().BeSameAs(profileEntity);
            relationship.IsOneToOne.Should().BeTrue();
            relationship.IsOneToMany.Should().BeFalse();
        }

        [Fact]
        public void AnalyzeSchema_ShouldIdentifyManyToManyRelationships()
        {
            // Arrange
            var analyzer = TestCompatibility.CreateSchemaAnalyzer();
            var database = CreateDatabaseWithManyToManyRelationship();

            // Act
            var result = analyzer.AnalyzeSchema(database);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(database); // Should return the same object

            // Check that the many-to-many relationship was identified through the join table
            var studentEntity = database.GetEntity("Student");
            var courseEntity = database.GetEntity("Course");
            var enrollmentEntity = database.GetEntity("StudentCourse");
            
            studentEntity!.ChildRelationships.Count.Should().BeGreaterThan(0);
            courseEntity!.ChildRelationships.Count.Should().BeGreaterThan(0);
            enrollmentEntity!.ParentRelationships.Should().HaveCount(2);
            
            // Check relationship between Student and StudentCourse
            var studentToEnrollment = studentEntity.ChildRelationships
                .FirstOrDefault(r => r.ChildEntity.Name == "StudentCourse");
            studentToEnrollment.Should().NotBeNull();
            studentToEnrollment!.IsOneToMany.Should().BeTrue();
            
            // Check relationship between Course and StudentCourse
            var courseToEnrollment = courseEntity.ChildRelationships
                .FirstOrDefault(r => r.ChildEntity.Name == "StudentCourse");
            courseToEnrollment.Should().NotBeNull();
            courseToEnrollment!.IsOneToMany.Should().BeTrue();
        }

        [Fact]
        public void AnalyzeSchema_ShouldIdentifySelfReferencingRelationships()
        {
            // Arrange
            var analyzer = TestCompatibility.CreateSchemaAnalyzer();
            var database = CreateDatabaseWithSelfReferencingRelationship();

            // Act
            var result = analyzer.AnalyzeSchema(database);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(database); // Should return the same object

            // Check that the self-referencing relationship was identified
            var employeeEntity = database.GetEntity("Employee");
            
            employeeEntity!.ChildRelationships.Should().HaveCount(1);
            employeeEntity!.ParentRelationships.Should().HaveCount(1);
            
            var relationship = employeeEntity.ChildRelationships.First();
            relationship.ParentEntity.Should().BeSameAs(employeeEntity);
            relationship.ChildEntity.Should().BeSameAs(employeeEntity);
            relationship.IsSelfReferencing.Should().BeTrue();
        }

        [Fact]
        public void DetectRelationshipTypes_ShouldClassifyRelationshipsCorrectly()
        {
            // Arrange
            var analyzer = TestCompatibility.CreateSchemaAnalyzer();
            var database = CreateComplexDatabase();
            
            // Act
            var result = analyzer.AnalyzeSchema(database);
            
            // Assert
            var customerEntity = database.GetEntity("Customer");
            var orderEntity = database.GetEntity("Order");
            var profileEntity = database.GetEntity("CustomerProfile");
            
            // Check Customer->Order (one-to-many)
            var customerToOrder = customerEntity!.ChildRelationships
                .FirstOrDefault(r => r.ChildEntity.Name == "Order");
            customerToOrder.Should().NotBeNull();
            customerToOrder!.IsOneToMany.Should().BeTrue();
            customerToOrder!.IsOneToOne.Should().BeFalse();
            
            // Check Customer->CustomerProfile (one-to-one)
            var customerToProfile = customerEntity!.ChildRelationships
                .FirstOrDefault(r => r.ChildEntity.Name == "CustomerProfile");
            customerToProfile.Should().NotBeNull();
            customerToProfile!.IsOneToOne.Should().BeTrue();
            customerToProfile!.IsOneToMany.Should().BeFalse();
        }

        [Fact]
        public void DetectRelationshipTypes_ShouldHandle_NoRelationships()
        {
            // Arrange
            var analyzer = TestCompatibility.CreateSchemaAnalyzer();
            var database = CreateDatabaseWithNoRelationships();
            
            // Act
            var result = analyzer.AnalyzeSchema(database);
            
            // Assert
            var customerEntity = database.GetEntity("Customer");
            var orderEntity = database.GetEntity("Order");
            
            // Verify no relationships were created
            customerEntity!.ChildRelationships.Should().BeEmpty();
            customerEntity!.ParentRelationships.Should().BeEmpty();
            orderEntity!.ChildRelationships.Should().BeEmpty();
            orderEntity!.ParentRelationships.Should().BeEmpty();
        }

        [Fact]
        public void SetNavigationPropertyNames_ShouldGenerateAppropriateNames()
        {
            // Arrange
            var analyzer = TestCompatibility.CreateSchemaAnalyzer();
            var database = CreateDatabaseWithStandardRelationships();
            
            // Act
            var result = analyzer.AnalyzeSchema(database);
            
            // Assert
            var customerEntity = database.GetEntity("Customer");
            var orderEntity = database.GetEntity("Order");
            
            var relationship = customerEntity!.ChildRelationships.First();
            
            // For one-to-many, parent should have plural child name
            relationship.NavigationPropertyName.Should().Be("Orders");
            
            // Child should have singular parent name
            relationship.InverseNavigationPropertyName.Should().Be("Customer");
        }

        [Fact]
        public void SetNavigationPropertyNames_ShouldHandleOneToOneRelationshipsCorrectly()
        {
            // Arrange
            var analyzer = TestCompatibility.CreateSchemaAnalyzer();
            var database = CreateDatabaseWithOneToOneRelationship();

            // Act
            var result = analyzer.AnalyzeSchema(database);

            // Assert
            var customerEntity = database.GetEntity("Customer");
            var profileEntity = database.GetEntity("CustomerProfile");
            
            var relationship = customerEntity!.ChildRelationships.First();
            
            // For one-to-one, parent should have singular child name
            relationship.NavigationPropertyName.Should().Be("CustomerProfile");
            
            // Child should have singular parent name
            relationship.InverseNavigationPropertyName.Should().Be("Customer");
        }

        // Helper methods to create test databases

        private IDatabase CreateDatabaseWithStandardRelationships()
        {
            var database = new Database("TestDb", "dbo");
            
            // Create Customer entity
            var customerEntity = new Entity("Customer", "dbo", "Customer", EntityType.Table);
            var customerIdProperty = new Property("CustomerId", "Id", "int", "int", customerEntity, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var customerNameProperty = new Property("CustomerName", "Name", "varchar", "string", customerEntity, 2)
            {
                MaxLength = 100
            };
            customerEntity.AddProperty(customerIdProperty);
            customerEntity.AddProperty(customerNameProperty);
            
            // Create Order entity
            var orderEntity = new Entity("Order", "dbo", "Order", EntityType.Table);
            var orderIdProperty = new Property("OrderId", "Id", "int", "int", orderEntity, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var orderDateProperty = new Property("OrderDate", "Date", "datetime", "DateTime", orderEntity, 2);
            var customerIdFkProperty = new Property("CustomerId", "CustomerId", "int", "int", orderEntity, 3)
            {
                IsForeignKey = true
            };
            orderEntity.AddProperty(orderIdProperty);
            orderEntity.AddProperty(orderDateProperty);
            orderEntity.AddProperty(customerIdFkProperty);
            
            database.AddEntity(customerEntity);
            database.AddEntity(orderEntity);
            
            return database;
        }

        private IDatabase CreateDatabaseWithOneToOneRelationship()
        {
            var database = new Database("TestDb", "dbo");
            
            // Create Customer entity
            var customerEntity = new Entity("Customer", "dbo", "Customer", EntityType.Table);
            var customerIdProperty = new Property("CustomerId", "Id", "int", "int", customerEntity, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var customerNameProperty = new Property("CustomerName", "Name", "varchar", "string", customerEntity, 2)
            {
                MaxLength = 100
            };
            customerEntity.AddProperty(customerIdProperty);
            customerEntity.AddProperty(customerNameProperty);
            
            // Create CustomerProfile entity with one-to-one relationship
            var profileEntity = new Entity("CustomerProfile", "dbo", "CustomerProfile", EntityType.Table);
            var profileIdProperty = new Property("ProfileId", "Id", "int", "int", profileEntity, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var customerIdFkProperty = new Property("CustomerId", "CustomerId", "int", "int", profileEntity, 2)
            {
                IsPrimaryKey = true, // Part of composite key for one-to-one
                IsForeignKey = true
            };
            var profileDataProperty = new Property("ProfileData", "Data", "varchar", "string", profileEntity, 3)
            {
                MaxLength = 500
            };
            profileEntity.AddProperty(profileIdProperty);
            profileEntity.AddProperty(customerIdFkProperty);
            profileEntity.AddProperty(profileDataProperty);
            
            database.AddEntity(customerEntity);
            database.AddEntity(profileEntity);
            
            return database;
        }

        private IDatabase CreateDatabaseWithManyToManyRelationship()
        {
            var database = new Database("TestDb", "dbo");
            
            // Create Student entity
            var studentEntity = new Entity("Student", "dbo", "Student", EntityType.Table);
            var studentIdProperty = new Property("StudentId", "Id", "int", "int", studentEntity, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var studentNameProperty = new Property("StudentName", "Name", "varchar", "string", studentEntity, 2)
            {
                MaxLength = 100
            };
            studentEntity.AddProperty(studentIdProperty);
            studentEntity.AddProperty(studentNameProperty);
            
            // Create Course entity
            var courseEntity = new Entity("Course", "dbo", "Course", EntityType.Table);
            var courseIdProperty = new Property("CourseId", "Id", "int", "int", courseEntity, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var courseNameProperty = new Property("CourseName", "Name", "varchar", "string", courseEntity, 2)
            {
                MaxLength = 100
            };
            courseEntity.AddProperty(courseIdProperty);
            courseEntity.AddProperty(courseNameProperty);
            
            // Create StudentCourse join entity
            var enrollmentEntity = new Entity("StudentCourse", "dbo", "StudentCourse", EntityType.Table);
            var studentIdFkProperty = new Property("StudentId", "StudentId", "int", "int", enrollmentEntity, 1)
            {
                IsPrimaryKey = true, // Part of composite key
                IsForeignKey = true
            };
            var courseIdFkProperty = new Property("CourseId", "CourseId", "int", "int", enrollmentEntity, 2)
            {
                IsPrimaryKey = true, // Part of composite key
                IsForeignKey = true
            };
            var enrollmentDateProperty = new Property("EnrollmentDate", "EnrollmentDate", "datetime", "DateTime", enrollmentEntity, 3);
            enrollmentEntity.AddProperty(studentIdFkProperty);
            enrollmentEntity.AddProperty(courseIdFkProperty);
            enrollmentEntity.AddProperty(enrollmentDateProperty);
            
            database.AddEntity(studentEntity);
            database.AddEntity(courseEntity);
            database.AddEntity(enrollmentEntity);
            
            return database;
        }

        private IDatabase CreateDatabaseWithSelfReferencingRelationship()
        {
            var database = new Database("TestDb", "dbo");
            
            // Create Employee entity with self-referencing relationship
            var employeeEntity = new Entity("Employee", "dbo", "Employee", EntityType.Table);
            var employeeIdProperty = new Property("EmployeeId", "Id", "int", "int", employeeEntity, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var employeeNameProperty = new Property("EmployeeName", "Name", "varchar", "string", employeeEntity, 2)
            {
                MaxLength = 100
            };
            var managerIdProperty = new Property("ManagerId", "ManagerId", "int", "int", employeeEntity, 3)
            {
                IsForeignKey = true,
                IsNullable = true
            };
            employeeEntity.AddProperty(employeeIdProperty);
            employeeEntity.AddProperty(employeeNameProperty);
            employeeEntity.AddProperty(managerIdProperty);
            
            database.AddEntity(employeeEntity);
            
            return database;
        }

        private IDatabase CreateDatabaseWithNoRelationships()
        {
            var database = new Database("TestDb", "dbo");
            
            // Create Customer entity
            var customerEntity = new Entity("Customer", "dbo", "Customer", EntityType.Table);
            var customerIdProperty = new Property("CustomerId", "Id", "int", "int", customerEntity, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var customerNameProperty = new Property("CustomerName", "Name", "varchar", "string", customerEntity, 2)
            {
                MaxLength = 100
            };
            customerEntity.AddProperty(customerIdProperty);
            customerEntity.AddProperty(customerNameProperty);
            
            // Create Order entity without foreign key
            var orderEntity = new Entity("Order", "dbo", "Order", EntityType.Table);
            var orderIdProperty = new Property("OrderId", "Id", "int", "int", orderEntity, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var orderDateProperty = new Property("OrderDate", "Date", "datetime", "DateTime", orderEntity, 2);
            orderEntity.AddProperty(orderIdProperty);
            orderEntity.AddProperty(orderDateProperty);
            
            database.AddEntity(customerEntity);
            database.AddEntity(orderEntity);
            
            return database;
        }

        private IDatabase CreateComplexDatabase()
        {
            var database = new Database("TestDb", "dbo");
            
            // Create Customer entity
            var customerEntity = new Entity("Customer", "dbo", "Customer", EntityType.Table);
            var customerIdProperty = new Property("CustomerId", "Id", "int", "int", customerEntity, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var customerNameProperty = new Property("CustomerName", "Name", "varchar", "string", customerEntity, 2)
            {
                MaxLength = 100
            };
            customerEntity.AddProperty(customerIdProperty);
            customerEntity.AddProperty(customerNameProperty);
            
            // Create Order entity (one-to-many with Customer)
            var orderEntity = new Entity("Order", "dbo", "Order", EntityType.Table);
            var orderIdProperty = new Property("OrderId", "Id", "int", "int", orderEntity, 1)
            {
                IsPrimaryKey = true,
                IsIdentity = true
            };
            var orderDateProperty = new Property("OrderDate", "Date", "datetime", "DateTime", orderEntity, 2);
            var customerIdFkProperty = new Property("CustomerId", "CustomerId", "int", "int", orderEntity, 3)
            {
                IsForeignKey = true
            };
            orderEntity.AddProperty(orderIdProperty);
            orderEntity.AddProperty(orderDateProperty);
            orderEntity.AddProperty(customerIdFkProperty);
            
            // Create CustomerProfile entity (one-to-one with Customer)
            var profileEntity = new Entity("CustomerProfile", "dbo", "CustomerProfile", EntityType.Table);
            var profileIdProperty = new Property("ProfileId", "Id", "int", "int", profileEntity, 1)
            {
                IsPrimaryKey = true
            };
            var customerIdProfileFkProperty = new Property("CustomerId", "CustomerId", "int", "int", profileEntity, 2)
            {
                IsPrimaryKey = true, // Part of composite key for one-to-one
                IsForeignKey = true
            };
            var profileDataProperty = new Property("ProfileData", "Data", "varchar", "string", profileEntity, 3)
            {
                MaxLength = 500
            };
            profileEntity.AddProperty(profileIdProperty);
            profileEntity.AddProperty(customerIdProfileFkProperty);
            profileEntity.AddProperty(profileDataProperty);
            
            database.AddEntity(customerEntity);
            database.AddEntity(orderEntity);
            database.AddEntity(profileEntity);
            
            return database;
        }
    }
}
