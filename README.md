# Introduction 

A C# analyzer to convert Rhino Mocks to NSubstitute.


[![Azure Pipelines CI job](https://ladeak.visualstudio.com/My%20private%20projects/_apis/build/status/TestUpdaterAnalyzers%20-%20CI?branchName=master)](https://ladeak.visualstudio.com/My%20private%20projects/_build/latest?definitionId=9&branchName=master)

![CI Build and Test](https://github.com/ladeak/TestUpdaterAnalyzers/workflows/CI%20Build%20and%20Test/badge.svg)

[![NuGet](https://img.shields.io/nuget/v/LaDeak.TestMockUpdater.svg)](https://www.nuget.org/packages/LaDeak.TestMockUpdater/)

## Getting started

1. Add a package reference to the Test project using Rhino Mocks:

```xml
<PackageReference Include="LaDeak.TestMockUpdater" Version="*" />
```

1. Add a package reference to NSubstitute:

```xml
<PackageReference Include="NSubstitute" Version="*" />
```

Use the **C# Analyzer** to fix and update Rhino Mocks tests. Your original source must compile before conversion may take place.

![analyzer-sample](docs/analyzer-sample.gif)

## Supported Conversions

The following RhinoMocks features are converted to NSubstitute:

| Rhino Method            | NSubstitute       | Comment                                                                                   |
|-------------------------|-------------------|-------------------------------------------------------------------------------------------|
| GenerateMock            | Substitute.For    |                                                                                           |
| GenerateStub            | Substitute.For    |                                                                                           |
| Expect                  | Native call       | Extracts lambda to call method or access property on the mock.                            |
| Return                  | Returns           |                                                                                           |
| Throw                   | Throws            |                                                                                           |
| IgnoreArguments         | ReturnsForAnyArgs | Respecting ThrowsForAnyArgs or ReturnsForAnyArgs.                                         |
| Out Arguments           | Returns lamda     | Using ```Func<CallInfo, T> returnThis``` to set Out arguments.                            |
| OutRef                  | Returns lamda     | Using ```Func<CallInfo, T> returnThis``` to set Out arguments.                            |
| Stub                    | Native call       | Extracts lambda to call method or access property on the mock.                            |
| AssertWasCalled         | Received          |                                                                                           |
| AssertWasNotCalled      | DidNotReceive     |                                                                                           |
| Repeat                  | -                 | Repeat calls are removed and ignored.                                                     |
| PropertyBehavior        | -                 | PropertyBehavior calls are removed and ignored.                                           |
| VerifyAllExpectations   | Received          | Generates a set of Received assertatation at the end of the block.                        |
| WhenCalled              | Returns           | ```Action<MethodInvocation>``` converted to lambda expression in Returns method.          |
| ```Arg<T>.Is.Null```    | Arg.Is            | With predicate comparing to null.                                                         |
| ```Arg<T>.Is.NotNull``` | Arg.Is            | With predicate comparing not null.                                                        |
| ```Arg<T>.Is.Equal```   | Arg.Is            | With predicate comparing using Equals method call.                                        |
| ```Arg<T>.Is.Same```    | Arg.Is            | With predicate comparing using ReferenceEquals method call.                               |
| ```Arg<T>.Is.Matches``` | Arg.Is            | With predicate defined in the original expression.                                        |

## Analyzer

The analyzer currently looks for the following method calls: ```Stub```,  ```Expect```,  ```Returns```, ```Throws```,  ```GenerateMock```,  ```GenerateStub```. When mocks defined as local fieds, document level of conversion is available.

## Sample

The following example shows a Rhino Mocks test is converted to NSubstitute.

> Note this example is for demonstration purpose only, generally it is suggested to have separate tests to verify independent behaviors.

The original Rhino Mocks test method:

```csharp
[Fact]
public void CalculateId_Returns_NumberEs_Multiplied_Age_Height()
{
    // Arrange
    var validatorStub = MockRepository.GenerateStub<IValidator>();
    validatorStub.Stub(x => x.TryValidate(Arg<Request>.Matches(y => y.Name == "test"), out Arg<bool>.Out(true).Dummy)).Return(true);

    var nameProviderMock = MockRepository.GenerateMock<INameProvider>();
    nameProviderMock.Expect(x => x.GetFullName("test")).IgnoreArguments().Return("eee").Repeat.Any();
    nameProviderMock.Stub(x => x.Initialized).Return(true);

    var loggerMock = MockRepository.GenerateMock<ILogger<ComplexBusinessLogic>>();
    loggerMock.Stub(x => x.IsEnabled).PropertyBehavior();

    var sut = new ComplexBusinessLogic(validatorStub, nameProviderMock, loggerMock);
    
    // Act
    var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });

    // Assert
    Assert.Equal(3, result);
    nameProviderMock.VerifyAllExpectations();
    loggerMock.AssertWasCalled(x => x.Log(Arg<string>.Is.NotNull));
    Assert.True(loggerMock.IsEnabled);
}
```

The NSubstitute converted test method:

```csharp
[Fact]
public void CalculateId_Returns_NumberEs_Multiplied_Age_Height()
{
    // Arrange
    var validatorStub = Substitute.For<IValidator>();
    validatorStub.TryValidate(NSubstitute.Arg.Is<Request>(y => y.Name == "test"), out NSubstitute.Arg.Any<bool>()).Returns(a0 => { a0[1] = true; return true; });

    var nameProviderMock = Substitute.For<INameProvider>();
    nameProviderMock.GetFullName("test").ReturnsForAnyArgs("eee");
    nameProviderMock.Initialized.Returns(true);

    var loggerMock = Substitute.For<ILogger<ComplexBusinessLogic>>();

    var sut = new ComplexBusinessLogic(validatorStub, nameProviderMock, loggerMock);
    
    // Act
    var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });

    // Assert
    Assert.Equal(3, result);
    nameProviderMock.Received().GetFullName("test");
    loggerMock.Received().Log(NSubstitute.Arg.Is<string>(a0 => a0 != null));
    Assert.True(loggerMock.IsEnabled);
}
```

## Contribution

1. If it is small issue (spelling or a bug fix) feel free to start working on a fix.
1. If you are submitting a feature or substantial code contribution open an issue for discussion.
1. Add your code changes, follow existing code conventions.
1. Make sure all existing tests are still correct.
1. Add new tests to cover the new use-case or bug fix.
1. Submit a pull request with commit description detailing the new feature or the fixed bug. Link the related issue or discussion.
1. Once your code changes are merged, a new release is created.
