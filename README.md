# Introduction 

A C# analyzer to convert Rhino Mocks to NSubstitute.


[![Azure Pipelines CI job](https://ladeak.visualstudio.com/My%20private%20projects/_apis/build/status/TestUpdaterAnalyzers%20-%20CI?branchName=master)](https://ladeak.visualstudio.com/My%20private%20projects/_build/latest?definitionId=9&branchName=master)

[![NuGet](https://img.shields.io/nuget/v/LaDeak.TestMockUpdater.svg)](https://www.nuget.org/packages/LaDeak.TestMockUpdater/)

![analyzer-sample](docs/analyzer-sample.gif)

## Getting started

1. Add a package reference to the Test project using Rhino Mocks:

```xml
<PackageReference Include="LaDeak.TestMockUpdater" Version="*" />
```

1. Add a package reference to NSubstitute:

```xml
<PackageReference Include="NSubstitute" Version="*" />
```

Use the C# analyzer to fix and update Rhino Mocks tests. Your original source must compile before conversion may take place.

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

The following example shows a RhinoMocks test is converted to NSubstitute:

