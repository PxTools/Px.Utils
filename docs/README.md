# Px.Utils

Px.Utils is a .NET library for reading and processing px files and processing data in px-type cube format. The goal of this library is to offer a high performance and easy to use library that can be integrated into any .NET project. This project aims to follow these core design principles:

## Design goals
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

### Metadata models
#### ```Matrix<TData>```
Consists of ```IReadOnlyMatrixMetadata Metadata``` and ```TData[] Data```. This is the topmost level of the structure. The order of the data points is determined by the metadata in the following way:

| dim0-val0 || dim0-val1 || dim0-val2 ||
|-----------|-----------|-----------|-----------|-----------|-----------|
| dim1-val0 | dim1-val1| dim1-val0 | dim1-val1| dim1-val0 | dim1-val1|
| dim2-val0 | dim2-val0| dim2-val0 | dim2-val0| dim2-val0 | dim2-val0|
| 0 | 1 | 2 | 3 | 4 | 5 |

In this example, the dim0 is the first dimension in the dimension list of the metadata. And val0 is the first value in each dimension. Dim2 has only one value, so it does not affect the order of the data points (in other words if the length of the dimension is one, it has no impact on the volume of the data cube).
Example: If we choose the second value from dim0 (index 1) and the first value from dim1 (index 0), the data index is 2 (We can only choose val0 from dim2).

There are no limits for the number or size of dimensions. But it is important to note, that if even one dimension has a size of 0, the data array will be empty (the volume of the data cube will be 0).

#### ```MatrixMetadata : IReadOnlyMatrixMetadata```

#### ```Dimension : IReadOnlyDimension```

#### ```ContentDimension : Dimension```

#### ```TimeDimension : Dimension```

#### ```DimensionValue : IReadOnlyDimensionValue```

#### ```ContentDimensionValue : DimensionValue```

#### ```MetaProperty```


### Data models
```IDataValue``` is an interface for the data points that defines the basic computation methods for the data points. See the Computing section for more information.
If the ```TData``` in the ```Matrix<TData>``` implements the ```IDataValue``` interface, all of the extension methods for computing can be used. Other than that, the ```TData``` has no constraints.

There are two structs in Px.Utils that implement the ```IDataValue``` interface: ```DoubleDataValue``` and ```DecimalDataValue```. These structs can be used to store data values as doubles or decimals, but they also enable handling of the missing data values.
Px.Utils provides functionality for reading the data as these structs from the px files.

Standard numeric value types can be used as the ```TData``` in the ```Matrix<TData>``` class. However, the user must then provide the enconding for the missing data values.

### Validation

Px files can be validated either as a whole by using ```PxFileValidator``` or by using individual validators - ```SyntaxValidator```, ```ContentValidator``` and ```DataValidator``` - for different parts of the file. Custom validation functions or validator classes can be added to the validation processes.
Validator classes implement either ```IPxFileStreamValidator``` or ```IPxFileStreamValidatorAsync``` interfaces for synchronous and asynchronous validation processes respectively. Custom validation functions must implement either the ```IPxFileValidator``` or ```IPxFileValidatorAsync```, ```IValidator``` or ```IValidatorAsync``` interfaces.
```IPxFileStreamValidator``` and ```IPxFileStreamValidatorAsync``` interfaces required the following parameters to run their ```Validate()``` or ```ValidateAsync()``` functions:
- stream (Stream): The stream of the px file to be validated
- filename (string): Name of the file to be validated
- encoding (Encoding, optional): Encoding of the px file. Default is Encoding.Default
- fileSystem (IFileSystem, optional): Object that defines the file system used for the validation process. Default file called LocalFileSystem system is used if none provided.

#### PxFileValidator : IPxFileStreamValidator, IPxFileStreamValidatorAsync
```PxFileValidator``` is a class that validates the whole px file including its data, metadata syntax and metadata contents. The class can be instantiated with the following parameters:
- syntaxConf (PxFileSyntaxConf, optional): Object that contains px file syntax configuration tokens and symbols.
Custom validator objects can be injected by calling the SetCustomValidatorFunctions or SetCustomValidators methods of the PxFileValidator object. Custom validators must implement either the IPxFileValidator or IPxFileValidatorAsync interface. Custom validation methods are stored in CustomSyntaxValidationFunctions and CustomContentValidationFunctions objects for syntax and content validation processes respectively.
Once the PxFileValidator object is instantiated, either the Validate or ValidateAsync method can be called to validate the px file. The Validate method returns a ValidationResult object that contains the validation results as a key value pair containing information about the rule violations.

#### SyntaxValidator : IPxFileStreamValidator, IPxFileStreamValidatorAsync
```SyntaxValidator``` is a class that validates the syntax of a px file's metadata. It needs to be run before other validators, because both the ```ContentValidator``` and ```DataValidator``` require information from the ```SyntaxValidationResult``` object that ```SyntaxValidator``` ```Validate()``` and ```ValidateAsync()``` methods return.
The class can be instantiated with the following parameters:
- syntaxConf (PxFileSyntaxConf, optional): Object that contains px file syntax configuration tokens and symbols.
- customValidationFunctions (CustomSyntaxValidationFunctions, optional): Object that contains custom validation functions for the syntax validation process.

#### ContentValidator : IValidator
```ContentValidator``` class validates the integrity of the contents of a px file's metadata. It needs to be run after the ```SyntaxValidator```, because it requires information from the ```SyntaxValidationResult``` object that ```SyntaxValidator``` ```Validate()``` and ```ValidateAsync()``` methods return.
The class can be instantiated with the following parameters:
- filename (string): Name of the file to be validated
- encoding (Encoding): Encoding of the px file.
- entries (ValidationStructuredEntry[]): Array of ValidationStructuredEntry objects that contain the metadata entries of the px file. This object is returned by the SyntaxValidator Validate and ValidateAsync methods.
- customContentValidationFunctions (CustomContentValidationFunctions, optional): Object that contains custom functions for validating the px file metadata contents.
- syntaxConf (PxFileSyntaxConf, optional): Object that contains px file syntax configuration tokens and symbols.

#### DataValidator : IPxFileStreamValidator, IPxFileStreamValidatorAsync
```DataValidator``` class is used to validate the data section of a px file. It needs to be run after the ```SyntaxValidator```, because it requires information from both the ```SyntaxValidationResult``` and ```ContentValidationResult``` objects that ```SyntaxValidator``` and ```ContentValidator``` ```Validate()``` and ```ValidateAsync()``` methods return.
The class can be instantiated with the following parameters:
- rowLen (int): Length of one row of Px file data. ContentValidationResult object contains this information.
- numOfRows (int): Amount of rows of Px file data. This information is also stored in ContentValidationResult object.
- startRow (long): The row number where the data section starts. This information is stored in the SyntaxValidationResult object.
- conf (PxFileSyntaxConf, optional): Syntax configuration for the Px file

#### DatabaseValidator : IValidator, IValidatorAsync
Whole px file databases can be validated using ```DatabaseValidator``` class. Validation can be done by using the blocking ```Validate()``` or asynchronous ```ValidateAsync()``` methods. ```DatabaseValidator``` class can be instantiated using the following parameters:
- directoryPath (string): Path to the database root 
- syntaxConf (PxFileSyntaxConf, optional): Syntax configuration for the Px file
- fileSystem (IFileSystem, optional): Object that defines the file system used for the validation process. Default file system is used if none provided
- customPxFileValidators (IDatabaseValidator, optional): Object containing validator functions ran for each px file within the database
- customAliasFileValidators (IDatabaseValidator, optional): Object containing validator functions ran for each alias file within the database
- customDirectoryValidators (IDatabaseValidator, optional): Object containing validator functions ran for each subdirectory within the database 

Database validation process validates each px file within the database and also the required structure and consistency of the database languages and encoding formats. The return object is a ```ValidationResult``` object that contains ```ValidationFeedback``` objects gathered during the validation process.
The database needs to contain alias files for each language used in the database for each folder that contains either subcategory folders or px files. If either languages or encoding formats differ between alias or px files, warnings are generated.
   
### Computing

```Matrix<TData>``` class has a set of extension methods for performing basic computations for the datapoints.

#### Sum
```SumToNewValue<TData>()``` computes sums of datapoints defined by a subset of values from a given dimension.
The method takes a new dimension value as a parameter that will define the resulting values.
The method also has an asyncronous variant ```SumToNewValueAsync<TData>()```.  

```AddConstantToSubset<TData>()``` adds a constant to a subset of datapoints. Also has an asynchronous variant ```AddConstantToSubsetAsync<TData>()```.

#### Multiplication
```MultiplyToNewValue<TData>()``` computes products of datapoints defined by a subset of values from a given dimension.
The method takes a new dimension value as a parameter that will define the resulting values.
The method also has an asyncronous variant ```MultiplyToNewValueAsync<TData>()```.  

```MultiplySubsetByConstant<TData>()``` Multiply a subset of datapoints by a constant. Also has an asynchronous variant ```MultiplySubsetByConstantAsync<TData>()```.

#### Division
```DivideSubsetBySelectedValue()``` divides a subset of datapoints defined by values from one dimension with datapoints defined by a value from the same dimension.
Also has an asyncronous variant ```DivideSubsetBySelectedValueAsync()```

```DivideSubsetByConstant<TData>()``` Divide a subset of datapoints by a constant. Also has an asynchronous variant ```DivideSubsetByConstantAsync<TData>()```.

#### General

```ApplyOverDimension<TData>()``` Generatas a new set datapoints by applying a function to datapoints defined by a subset of values from one dimension.
The method takes a new dimension value as a parameter that will define the resulting values.

```ApplyToSubMap<TData>()``` Applies a function to a set of datapoints.

```ApplyRelative<TData>()``` Applies a function to a set of datapoints defined by a subset of values from one dimension.
The method also takes a code of a value from the same dimension as a parameter which is used to define additional datapoints to be used as
an input for the function.
