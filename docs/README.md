# Px.Utils

Px.Utils is a .NET library for reading and processing px files and processing data in px-type cube format. The goal of this library is to offer a high performance and easy to use library that can be integrated into any .NET project. This project aims to follow these core design principles:

### High performance
Everything is designed with performance in mind. Px files are commonly used in webserver environments where performance and low memory usage is important.
### Be as unopinionated as possible
We do not want to limit the user if not absolutely necessary. There are variations in the metadata between users and we want to support as many as possible.
### High modularity
Extending the library with a new features should be as easy as possible and every module should be useable on its own.

## Installation
Px.Utils can be installed using .NET CLI or NuGet Package Manager.

### .NET CL
#### Latest
```bash
dotnet add package Px.Utils
```
#### Specific version
```bash
dotnet add package Px.Utils --version 1.0.0
```


### NuGet Package Manager
#### Latest
```bash
nuget install Px.Utils
```
#### Specific version
```bash
nuget install Px.Utils -Version 1.0.0
```

## Features

### Reading the px files
TBA

### Validation

#### Px file validation
Px files can be validated either as a whole by using PxFileValidator or by using individual validators - SyntaxValidator, ContentValidator and DataValidator - for different parts of the file. Custom validation functions or validator classes can be added to the validation processes.

##### PxFileValidator
PxFileValidator is a class that validates the whole px file including its data, metadata syntax and metadata contents. The class can be instantiated with the following parameters:
- stream (Stream): The stream of the px file to be validated
- filename (string): Name of the file to be validated
- encoding (Encoding, optional): Encoding of the px file. Default is Encoding.Default
- syntaxConf (PxFileSyntaxConf, optional): Object that contains px file syntax configuration tokens and symbols.

Custom validator objects can be injected by calling the SetCustomValidatorFunctions or SetCustomValidators methods of the PxFileValidator object. Custom validators must implement either the IPxFileValidator or IPxFileValidatorAsync interface. Custom validation methods are stored in CustomSyntaxValidationFunctions and CustomContentValidationFunctions objects for syntax and content validation processes respectively.

Once the PxFileValidator object is instantiated, either the Validate or ValidateAsync method can be called to validate the px file. The Validate method returns a ValidationResult object that contains the validation results as ValidationFeedbackItem object array.

##### SyntaxValidator
SyntaxValidator is a class that validates the syntax of a px file's metadata. It needs to be run before other validators, because both the ContentValidator and DataValidator require information from the SyntaxValidationResult object that SyntaxValidator Validate and ValidateAsync methods return.
The class can be instantiated with the following parameters:
- stream (Stream): The stream of the px file to be validated
- encoding (Encoding): Encoding of the px file.
- filename (string): Name of the file to be validated
- syntaxConf (PxFileSyntaxConf, optional): Object that contains px file syntax configuration tokens and symbols.
- customValidationFunctions (CustomSyntaxValidationFunctions, optional): Object that contains custom validation functions for the syntax validation process.
- leaveStreamOpen (bool, optional): If true, the stream will not be closed after the validation process. Default is false.

##### ContentValidator
ContentValidator class validates the integrity of the contents of a px file's metadata. It needs to be run after the SyntaxValidator, because it requires information from the SyntaxValidationResult object that SyntaxValidator Validate and ValidateAsync methods return.
The class can be instantiated with the following parameters:
- filename (string): Name of the file to be validated
- encoding (Encoding): Encoding of the px file.
- entries (ValidationStructuredEntry[]): Array of ValidationStructuredEntry objects that contain the metadata entries of the px file. This object is returned by the SyntaxValidator Validate and ValidateAsync methods.
- customContentValidationFunctions (CustomContentValidationFunctions, optional): Object that contains custom functions for validating the px file metadata contents.
- syntaxConf (PxFileSyntaxConf, optional): Object that contains px file syntax configuration tokens and symbols.

##### DataValidator
DataValidator class is used to validate the data section of a px file. It needs to be run after the SyntaxValidator, because it requires information from both the SyntaxValidationResult and ContentValidationResult objects that SyntaxValidator and ContentValidator Validate and ValidateAsync methods return.
The class can be instantiated with the following parameters:
- stream (Stream): Px file stream to be validated
- rowLen (int): Length of one row of Px file data. ContentValidationResult object contains this information.
- numOfRows (int): Amount of rows of Px file data. This information is also stored in ContentValidationResult object.
- filename (string): Name of the file being validated
- startRow (long): The row number where the data section starts. This information is stored in the SyntaxValidationResult object.
- encoding (Encoding, optional): Encoding of the stream
- conf (PxFileSyntaxConf, optional): Syntax configuration for the Px file

#### Database validation
TBA

### Data models
TBA

### Computing
TBA

## Installing
TBA when published to NuGet