namespace EzDbCodeGen.HandlebarsTests;

public class HandlebarsUtilityTests
{
    private readonly HandlebarsUtility _handlebarsUtility;

    public HandlebarsUtilityTests()
    {
        _handlebarsUtility = new HandlebarsUtility();
    }

    [Fact]
    public void Compile_WithValidTemplate_ReturnsCompiledTemplate()
    {
        // Arrange
        var template = "Hello, {{name}}!";
        
        // Act
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var result = compiledTemplate(new { name = "World" });
        
        // Assert
        Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void Compile_WithEmptyTemplate_ReturnsEmptyString()
    {
        // Arrange
        var template = "";
        
        // Act
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var result = compiledTemplate(new { name = "World" });
        
        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void Compile_WithNullTemplate_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _handlebarsUtility.Compile(null!));
    }

    [Fact]
    public void RegisterStringHelpers_RegistersAllHelpers()
    {
        // Arrange
        var template = "{{#if (eq 'test' 'test')}}Equal{{else}}Not Equal{{/if}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var result = compiledTemplate(new { });
        
        // Assert
        Assert.Equal("Equal", result);
    }

    [Fact]
    public void RegisterStringHelpers_EqHelper_ComparesStringsCorrectly()
    {
        // Arrange
        var template = "{{#if (eq 'test' 'test')}}Equal{{else}}Not Equal{{/if}}";
        var templateNotEqual = "{{#if (eq 'test' 'different')}}Equal{{else}}Not Equal{{/if}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var compiledTemplateNotEqual = _handlebarsUtility.Compile(templateNotEqual);
        var result = compiledTemplate(new { });
        var resultNotEqual = compiledTemplateNotEqual(new { });
        
        // Assert
        Assert.Equal("Equal", result);
        Assert.Equal("Not Equal", resultNotEqual);
    }

    [Fact]
    public void RegisterStringHelpers_NeHelper_ComparesStringsCorrectly()
    {
        // Arrange
        var template = "{{#if (ne 'test' 'different')}}Not Equal{{else}}Equal{{/if}}";
        var templateEqual = "{{#if (ne 'test' 'test')}}Not Equal{{else}}Equal{{/if}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var compiledTemplateEqual = _handlebarsUtility.Compile(templateEqual);
        var result = compiledTemplate(new { });
        var resultEqual = compiledTemplateEqual(new { });
        
        // Assert
        Assert.Equal("Not Equal", result);
        Assert.Equal("Equal", resultEqual);
    }

    [Fact]
    public void RegisterStringHelpers_LtHelper_ComparesStringsCorrectly()
    {
        // Arrange
        var template = "{{#if (lt 'a' 'b')}}Less Than{{else}}Not Less Than{{/if}}";
        var templateNotLessThan = "{{#if (lt 'b' 'a')}}Less Than{{else}}Not Less Than{{/if}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var compiledTemplateNotLessThan = _handlebarsUtility.Compile(templateNotLessThan);
        var result = compiledTemplate(new { });
        var resultNotLessThan = compiledTemplateNotLessThan(new { });
        
        // Assert
        Assert.Equal("Less Than", result);
        Assert.Equal("Not Less Than", resultNotLessThan);
    }

    [Fact]
    public void RegisterStringHelpers_GtHelper_ComparesStringsCorrectly()
    {
        // Arrange
        var template = "{{#if (gt 'b' 'a')}}Greater Than{{else}}Not Greater Than{{/if}}";
        var templateNotGreaterThan = "{{#if (gt 'a' 'b')}}Greater Than{{else}}Not Greater Than{{/if}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var compiledTemplateNotGreaterThan = _handlebarsUtility.Compile(templateNotGreaterThan);
        var result = compiledTemplate(new { });
        var resultNotGreaterThan = compiledTemplateNotGreaterThan(new { });
        
        // Assert
        Assert.Equal("Greater Than", result);
        Assert.Equal("Not Greater Than", resultNotGreaterThan);
    }

    [Fact]
    public void RegisterStringHelpers_LeHelper_ComparesStringsCorrectly()
    {
        // Arrange
        var template = "{{#if (le 'a' 'a')}}Less Than or Equal{{else}}Not Less Than or Equal{{/if}}";
        var templateNotLessThanOrEqual = "{{#if (le 'b' 'a')}}Less Than or Equal{{else}}Not Less Than or Equal{{/if}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var compiledTemplateNotLessThanOrEqual = _handlebarsUtility.Compile(templateNotLessThanOrEqual);
        var result = compiledTemplate(new { });
        var resultNotLessThanOrEqual = compiledTemplateNotLessThanOrEqual(new { });
        
        // Assert
        Assert.Equal("Less Than or Equal", result);
        Assert.Equal("Not Less Than or Equal", resultNotLessThanOrEqual);
    }

    [Fact]
    public void RegisterStringHelpers_GeHelper_ComparesStringsCorrectly()
    {
        // Arrange
        var template = "{{#if (ge 'a' 'a')}}Greater Than or Equal{{else}}Not Greater Than or Equal{{/if}}";
        var templateNotGreaterThanOrEqual = "{{#if (ge 'a' 'b')}}Greater Than or Equal{{else}}Not Greater Than or Equal{{/if}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var compiledTemplateNotGreaterThanOrEqual = _handlebarsUtility.Compile(templateNotGreaterThanOrEqual);
        var result = compiledTemplate(new { });
        var resultNotGreaterThanOrEqual = compiledTemplateNotGreaterThanOrEqual(new { });
        
        // Assert
        Assert.Equal("Greater Than or Equal", result);
        Assert.Equal("Not Greater Than or Equal", resultNotGreaterThanOrEqual);
    }

    [Fact]
    public void RegisterStringHelpers_ConcatHelper_ConcatenatesStringsCorrectly()
    {
        // Arrange
        var template = "{{concat 'Hello' ' ' 'World'}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var result = compiledTemplate(new { });
        
        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void RegisterStringHelpers_LowerHelper_ConvertsToLowerCase()
    {
        // Arrange
        var template = "{{lower 'HELLO WORLD'}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var result = compiledTemplate(new { });
        
        // Assert
        Assert.Equal("hello world", result);
    }

    [Fact]
    public void RegisterStringHelpers_UpperHelper_ConvertsToUpperCase()
    {
        // Arrange
        var template = "{{upper 'hello world'}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var result = compiledTemplate(new { });
        
        // Assert
        Assert.Equal("HELLO WORLD", result);
    }

    [Fact]
    public void RegisterStringHelpers_PascalHelper_ConvertsToPascalCase()
    {
        // Arrange
        var template = "{{pascal 'hello world'}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var result = compiledTemplate(new { });
        
        // Assert
        Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void RegisterStringHelpers_CamelHelper_ConvertsToCamelCase()
    {
        // Arrange
        var template = "{{camel 'Hello World'}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var result = compiledTemplate(new { });
        
        // Assert
        Assert.Equal("helloWorld", result);
    }

    [Fact]
    public void RegisterStringHelpers_PluralHelper_ConvertsToPlural()
    {
        // Arrange
        var template = "{{plural 'person'}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var result = compiledTemplate(new { });
        
        // Assert
        Assert.Equal("people", result);
    }

    [Fact]
    public void RegisterStringHelpers_SingularHelper_ConvertsToSingular()
    {
        // Arrange
        var template = "{{singular 'people'}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var result = compiledTemplate(new { });
        
        // Assert
        Assert.Equal("person", result);
    }

    [Fact]
    public void RegisterStringHelpers_ReplaceHelper_ReplacesSubstrings()
    {
        // Arrange
        var template = "{{replace 'Hello World' 'World' 'Universe'}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var result = compiledTemplate(new { });
        
        // Assert
        Assert.Equal("Hello Universe", result);
    }

    [Fact]
    public void RegisterStringHelpers_SubstringHelper_ExtractsSubstring()
    {
        // Arrange
        var template = "{{substring 'Hello World' 6}}";
        var templateWithLength = "{{substring 'Hello World' 0 5}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var compiledTemplateWithLength = _handlebarsUtility.Compile(templateWithLength);
        var result = compiledTemplate(new { });
        var resultWithLength = compiledTemplateWithLength(new { });
        
        // Assert
        Assert.Equal("World", result);
        Assert.Equal("Hello", resultWithLength);
    }

    [Fact]
    public void Compile_WithComplexTemplate_ProcessesCorrectly()
    {
        // Arrange
        var template = @"
{{#each people}}
  {{#if (eq name 'John')}}
    {{upper name}} is {{age}} years old.
  {{else}}
    {{name}} is {{age}} years old.
  {{/if}}
{{/each}}";
        var data = new { people = new[] { 
            new { name = "John", age = 30 }, 
            new { name = "Jane", age = 25 } 
        }};
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplate = _handlebarsUtility.Compile(template);
        var result = compiledTemplate(data);
        
        // Assert
        Assert.Contains("JOHN is 30 years old.", result);
        Assert.Contains("Jane is 25 years old.", result);
    }

    [Fact]
    public void RegisterStringHelpers_WithInvalidArguments_ThrowsHandlebarsException()
    {
        // Arrange
        _handlebarsUtility.RegisterStringHelpers();
        
        // Test eq helper with invalid arguments
        var templateEq = "{{eq 'test'}}";
        var compiledTemplateEq = _handlebarsUtility.Compile(templateEq);
        
        // Act & Assert
        var exEq = Assert.Throws<HandlebarsException>(() => compiledTemplateEq(new { }));
        Assert.Contains("{{eq}} helper requires exactly 2 arguments", exEq.Message);
        
        // Test replace helper with invalid arguments
        var templateReplace = "{{replace 'test' 'e'}}";
        var compiledTemplateReplace = _handlebarsUtility.Compile(templateReplace);
        
        // Act & Assert
        var exReplace = Assert.Throws<HandlebarsException>(() => compiledTemplateReplace(new { }));
        Assert.Contains("{{replace}} helper requires exactly 3 arguments", exReplace.Message);
    }

    [Fact]
    public void RegisterStringHelpers_SubstringHelper_WithInvalidArguments_ThrowsHandlebarsException()
    {
        // Arrange
        _handlebarsUtility.RegisterStringHelpers();
        
        // Test substring helper with invalid start index
        var templateInvalidStart = "{{substring 'test' 'invalid'}}";
        var compiledTemplateInvalidStart = _handlebarsUtility.Compile(templateInvalidStart);
        
        // Act & Assert
        var exInvalidStart = Assert.Throws<HandlebarsException>(() => compiledTemplateInvalidStart(new { }));
        Assert.Contains("{{substring}} helper's second argument must be a valid integer", exInvalidStart.Message);
        
        // Test substring helper with invalid length
        var templateInvalidLength = "{{substring 'test' 0 'invalid'}}";
        var compiledTemplateInvalidLength = _handlebarsUtility.Compile(templateInvalidLength);
        
        // Act & Assert
        var exInvalidLength = Assert.Throws<HandlebarsException>(() => compiledTemplateInvalidLength(new { }));
        Assert.Contains("{{substring}} helper's third argument must be a valid integer", exInvalidLength.Message);
    }

    [Fact]
    public void RegisterStringHelpers_SubstringHelper_WithOutOfRangeArguments_ReturnsEmptyString()
    {
        // Arrange
        var templateStartOutOfRange = "{{substring 'test' 10}}";
        var templateNegativeStart = "{{substring 'test' -1}}";
        var templateNegativeLength = "{{substring 'test' 0 -1}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplateStartOutOfRange = _handlebarsUtility.Compile(templateStartOutOfRange);
        var compiledTemplateNegativeStart = _handlebarsUtility.Compile(templateNegativeStart);
        var compiledTemplateNegativeLength = _handlebarsUtility.Compile(templateNegativeLength);
        
        var resultStartOutOfRange = compiledTemplateStartOutOfRange(new { });
        var resultNegativeStart = compiledTemplateNegativeStart(new { });
        var resultNegativeLength = compiledTemplateNegativeLength(new { });
        
        // Assert
        Assert.Equal("", resultStartOutOfRange);
        Assert.Equal("", resultNegativeStart);
        Assert.Equal("", resultNegativeLength);
    }

    [Fact]
    public void RegisterStringHelpers_CamelHelper_HandlesEdgeCases()
    {
        // Arrange
        var templateEmpty = "{{camel ''}}";
        var templateSingleWord = "{{camel 'test'}}";
        var templateWithSpecialChars = "{{camel 'hello_world-example test'}}";
        
        // Act
        _handlebarsUtility.RegisterStringHelpers();
        var compiledTemplateEmpty = _handlebarsUtility.Compile(templateEmpty);
        var compiledTemplateSingleWord = _handlebarsUtility.Compile(templateSingleWord);
        var compiledTemplateWithSpecialChars = _handlebarsUtility.Compile(templateWithSpecialChars);
        
        var resultEmpty = compiledTemplateEmpty(new { });
        var resultSingleWord = compiledTemplateSingleWord(new { });
        var resultWithSpecialChars = compiledTemplateWithSpecialChars(new { });
        
        // Assert
        Assert.Equal("", resultEmpty);
        Assert.Equal("test", resultSingleWord);
        Assert.Equal("helloWorldExampleTest", resultWithSpecialChars);
    }
}
