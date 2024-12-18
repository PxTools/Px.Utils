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

### .NET CLI
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

The read pipeline consists of the following components: Reading the metadata, building the metadata and reading the data.
Each of these components can be used separately and replaced with custom implementations.
Especially if the contents of your px-files do not follow the standard px-file format, you might need to implement your own metadata builder.

#### PxFileMetadataReader : IPxFileMetadataReader
```ReadMetadata(Stream stream, Encoding encoding)``` reads the metadata entries from the provided stream as a IEnumerable of ```KeyValuePair<string, string>``` representing the keys and values of the entries.
**This method does not perform any validation on the metadata entries.**

```ReadMetadataAsync(Stream stream, Encoding encoding)``` is an asynchronous version of the ```ReadMetadata``` method.

#### MatrixMetadataBuilder : IMatrixMetadataBuilder
``` Build(IEnumerable<KeyValuePair<string, string>> metadataInput)``` creates a ```MatrixMetadata``` object from the provided metadata entries.
The entries need to be in the same key-value format as the output of the ```PxFileMetadataReader``` ```ReadMetadata``` method.

#### PxFileStreamDataReader : IPxFileStreamDataReader, IDisposable
 ```ReadDecimalDataValues(DecimalDataValue[] buffer, int offset, IMatrixMap target, IMatrixMap complete)``` reads data from the px-file into the provided buffer. There are simillary named methods for reading data in to other types of buffers.
 The data will be written in to the buffer in order, starting form the offset.

**IMPORTANT!** The target map must have the same order as the complete file map. This is for performance reasons, we do not want to move back and forth in the file or generate a second indexer for placing the data in the buffer.

### Metadata example
```csharp
    // Read meta
    using Stream pxFileStream = GetFileStream();
    PxFileMetadataReader reader = new();
    Encoding encoding = reader.GetEncoding(fileStream);
    IEnumerable<KeyValuePair<string, string>> entries = reader.ReadMetadata(fileStream, encoding);

    // Build meta
    MatrixMetadataBuilder builder = new();
    MatrixMetadata completeMeta = builder.Build(entries);
    MatrixMetadata metaWeUse = completeMeta.GetTransform(GetSomeSubsetMap());
```

### Data reader example
```csharp
    // Read data & build the matrix
    Matrix<DecimalDataValue> output = new(metaWeUse, new DecimalDataValue[indexer.DataLength]);
    using Stream fileStream = GetFileStream();
    using PxFileStreamDataReader dataReader = new(fileStream);
    dataReader.ReadDecimalDataValues(output.Data, 0, metaWeUse, completeMap);
```

### Reader/builder configuration
Behaviour of the reader and builder objects can be changed with ```PxFileConfiguration``` object.
```csharp
    PxFileConfiguration conf = PxFileConfiguration.Default;
    conf.Content.PropertyTypeDefinitions["EXAMPLE"] = MetaPropertyType.TextArray;
    MatrixMetadataBuilder builder = new(conf);
```

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

```GetTransform(IMatrixMap map)``` method can be used to take a subset of the the matrix and/or change the order of the dimensions or the dimension values.
It creates a new mutable deep copy of the matrix that have the structure defined by the map parameter. The data array will also be copied and reordered based on the map.

##### Example
```csharp
    MatrixMap map = new(
    [
        new DimensionMap("variable-0", ["variable-0_value-0", "variable-0_value-2", "variable-0_value-4"]),
        new DimensionMap("variable-1", ["variable-1_value-0", "variable-1_value-2"])
    ]);
    Matrix<DecimalDataValue> output = matrix.GetTransform(map);
```

#### ```MatrixMap : IMatrixMap```
This is a minimal way to represent the structure of the metadata. Does not contain any other information than the dimension and dimension value codes.
The ```IReadOnlyMatrixMetadata``` also implements the ```IMatrixMap``` interface.

#### ```MatrixMetadata : IReadOnlyMatrixMetadata```
Represents the table level metadata of the px file. Contains the dimension list and the language information. 
The ```IReadOnlyMatrixMetadata``` is a read-only interface for the metadata, this should be the primary way to access and use the metadata.

Similar to the ```Matrix<TData>```, the ```IReadOnlyMatrixMetadata``` has a ```GetTransform(IMatrixMap map)``` method that can be used to create a mutable deep copy with the structure defined by the map parameter.

#### ```Dimension : IReadOnlyDimension```
Represents the dimension level metadata of a px-file. This is a base class for all dimensions, and all dimension in the ```MatrixMetadata``` are in this type.
The dimensions have a ```Type``` property and dimensions with type ```Content``` or ```Time``` have some additional properties that can be accessed through type casting.

Each dimension has a unique string code among the dimension in the matrix.

Beacause the content dimension has its own type for dimension values, the dimensions hold their values in ```ValueList``` or ```ContentValueList``` collections to make it compatible with the C# type system.
Those collections have their own methods for accessing and going through the values (```Map()```, ```Find()```).
They both implement the ```IReadOnlyList<IReadOnlyDimensionValue>``` interface, but that does not allow accessing the values as mutable or as content dimension values.

#### ```ContentDimension : Dimension```
Differs from the base class by having values of type ```ContentDimensionValue```.

#### ```TimeDimension : Dimension```
Shares the same value type as the base class, but has an additional property in the dimension level metadata:
- ```Interval``` (```TimeDimensionInterval```[enum]): Represents the interval of the time dimension. Can be either Year, HalfYear, Quarter, Month, Week, Other or Irregular.
 
TimeDimension's Type (```DimensionType```[enum]) is always Time.

#### ```DimensionValue : IReadOnlyDimensionValue```
Represents the dimension value level metadata of a px-file. This is a base class for all dimension values. Each value has a unique string code among the values in the dimension.

#### ```ContentDimensionValue : DimensionValue```
Dimension value that contais content dimension value specific metadata properties:
- ```Unit``` (```MultilanguageString```): Stores the unit associated with the content dimension value (such as "EUR" or "%") as a multilanguage string.
- ```LastUpdated``` (DateTime): Stores the date and time of the last updated associated with the content dimension value.
- ```Precision``` (int): Stores the precision - the number of decimal places - of the content dimension value.

#### ```MetaProperty```
Px.Utils supports reading any metadata properties that follow the px file syntax. The properties are stored in a ```Dictionary<string, MetaProperty>``` collection called ```AdditionalProperties``` where the dictionary key is the property keyword.
The base class ```MetaProperty``` is abstract and each supported property type has its own class that inherits from it.
Currently supported property types (represented by ```MetaPropertyType``` enum) and their respective classes are: ```Text``` (```StringProperty```), ```MultilanguageText``` (```MultilanguageStringProperty```), ```TextArray``` (```StringListProperty```), ```MultilanguageTextArray``` (```MultilanguageStringListProperty```), ```Numeric``` (```NumericProperty```) and ```Boolean``` (```BooleanProperty```).

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
- conf (PxFileConfiguration, optional): Object that contains px file configuration.
Custom validator objects can be injected by calling the SetCustomValidatorFunctions or SetCustomValidators methods of the PxFileValidator object. Custom validators must implement either the IPxFileValidator or IPxFileValidatorAsync interface. Custom validation methods are stored in CustomSyntaxValidationFunctions and CustomContentValidationFunctions objects for syntax and content validation processes respectively.
Once the PxFileValidator object is instantiated, either the Validate or ValidateAsync method can be called to validate the px file. The Validate method returns a ValidationResult object that contains the validation results as a key value pair containing information about the rule violations.

##### Example
```csharp
	PxFileValidator validator = new PxFileValidator();
	ValidationResult result = validator.Validate(fileStream, "path/to/file.px", Encoding.UTF8);
	ValidationResult asyncResult = await validator.ValidateAsync(fileStream, "path/to/file.px", Encoding.UTF8, cancellationToken: cancellationToken);
```

#### SyntaxValidator : IPxFileStreamValidator, IPxFileStreamValidatorAsync
```SyntaxValidator``` is a class that validates the syntax of a px file's metadata. It needs to be run before other validators, because both the ```ContentValidator``` and ```DataValidator``` require information from the ```SyntaxValidationResult``` object that ```SyntaxValidator``` ```Validate()``` and ```ValidateAsync()``` methods return.
The class can be instantiated with the following parameters:
- conf (PxFileConfiguration, optional): Object that contains px file configuration.
- customValidationFunctions (CustomSyntaxValidationFunctions, optional): Object that contains custom validation functions for the syntax validation process.

##### Example
```csharp
	SyntaxValidator validator = new SyntaxValidator();
	SyntaxValidationResult result = validator.Validate(fileStream, "path/to/file.px", Encoding.UTF8);
	SyntaxValidationResult asyncResult = await validator.ValidateAsync(fileStream, "path/to/file.px", Encoding.UTF8, cancellationToken: cancellationToken);
```

#### ContentValidator : IValidator
```ContentValidator``` class validates the integrity of the contents of a px file's metadata. It needs to be run after the ```SyntaxValidator```, because it requires information from the ```SyntaxValidationResult``` object that ```SyntaxValidator``` ```Validate()``` and ```ValidateAsync()``` methods return.
The class can be instantiated with the following parameters:
- filename (string): Name of the file to be validated
- encoding (Encoding): Encoding of the px file.
- entries (ValidationStructuredEntry[]): Array of ValidationStructuredEntry objects that contain the metadata entries of the px file. This object is returned by the SyntaxValidator Validate and ValidateAsync methods.
- customContentValidationFunctions (CustomContentValidationFunctions, optional): Object that contains custom functions for validating the px file metadata contents.
- conf (PxFileConfiguration, optional): Object that contains px file configuration.

##### Example
```csharp
	Encoding encoding = Encoding.UTF8;
	SyntaxValidator syntaxValidator = new SyntaxValidator();
	SyntaxValidationResult syntaxResult = syntaxValidator.Validate(fileStream, "path/to/file.px", encoding);
	ContentValidator validator = new ContentValidator("path/to/file.px", encoding, syntaxResult.Result);
	ValidationResult result = validator.Validate();
```

#### DataValidator : IPxFileStreamValidator, IPxFileStreamValidatorAsync
```DataValidator``` class is used to validate the data section of a px file. It needs to be run after the ```SyntaxValidator``` and ```ContentValidator``` because it requires information provided by the ```SyntaxValidationResult``` and ```ContentValidationResult``` objects that the ```SyntaxValidator``` and ```ContentValidator``` ```Validate()``` and ```ValidateAsync()``` methods return.
The class can be instantiated with the following parameters:
- rowLen (int): Length of one row of Px file data. ContentValidationResult object contains this information.
- numOfRows (int): Amount of rows of Px file data. This information is also stored in ContentValidationResult object.
- startRow (long): The row number where the data section starts. This information is stored in the SyntaxValidationResult object.
- conf (PxFileConfiguration, optional): Configuration for the Px file

##### Example
```csharp
	Encoding encoding = Encoding.UTF8;
	SyntaxValidator syntaxValidator = new SyntaxValidator();
	SyntaxValidationResult syntaxResult = syntaxValidator.Validate(fileStream, "path/to/file.px", encoding);
	ContentValidator contentValidator = new ContentValidator("path/to/file.px", encoding, syntaxResult.Result);
	ValidationResult contentResult = contentValidator.Validate();
	DataValidator validator = new DataValidator(contentResult.DataRowLength, contentResult.DataRowAmount, syntaxResult.DataStartRow);
	ValidationResult result = validator.Validate(fileStream, "path/to/file.px", encoding);
```

#### DatabaseValidator : IValidator, IValidatorAsync
Whole px file databases can be validated using ```DatabaseValidator``` class. Validation can be done by using the blocking ```Validate()``` or asynchronous ```ValidateAsync()``` methods. ```DatabaseValidator``` class can be instantiated using the following parameters:
- directoryPath (string): Path to the database root 
- conf (PxFileConfiguration, optional): Configuration for the Px file
- fileSystem (IFileSystem, optional): Object that defines the file system used for the validation process. Default file system is used if none provided
- customPxFileValidators (IDatabaseValidator, optional): Object containing validator functions ran for each px file within the database
- customAliasFileValidators (IDatabaseValidator, optional): Object containing validator functions ran for each alias file within the database
- customDirectoryValidators (IDatabaseValidator, optional): Object containing validator functions ran for each subdirectory within the database 

Database validation process validates each px file within the database and also the required structure and consistency of the database languages and encoding formats. The return object is a ```ValidationResult``` object that contains ```ValidationFeedback``` objects gathered during the validation process.
The database needs to contain alias files for each language used in the database for each folder that contains either subcategory folders or px files. If either languages or encoding formats differ between alias or px files, warnings are generated.
   
##### Example
```csharp
	DatabaseValidator validator = new DatabaseValidator("path/to/database");
	ValidationResult result = validator.Validate();
	ValidationResult asyncResult = await validator.ValidateAsync(cancellationToken);
```

### Computing

```Matrix<TData>``` class has a set of extension methods for performing basic computations for the datapoints.

Values of dimensions can be summed or multiplied together to new values. If the original matrix has the following structure:

|| col0-0 || col0-1 || col0-2 ||
||-----------|-----------|-----------|-----------|-----------|-----------|
|| col1-0 | col1-1| col1-0 | col1-1| col1-0 | col1-1 |
|row0-0| 0 | 1 | 2 | 3 | 4 | 5 |
|row0-1| 6 | 7 | 8 | 9 | 10 | 11 |
|row0-2| 12 | 13 | 14 | 15 | 16 | 17 |

If we sum the row0 dimension's values 1 and 2 together to form a new value "rowSum", the resulting matrix will look like this:

|| col0-0 || col0-1 || col0-2 ||
||-----------|-----------|-----------|-----------|-----------|-----------|
|| col1-0 | col1-1| col1-0 | col1-1| col1-0 | col1-1 |
|row0-0| 0 | 1 | 2 | 3 | 4 | 5 |
|row0-1| 6 | 7 | 8 | 9 | 10 | 11 |
|row0-2| 12 | 13 | 14 | 15 | 16 | 17 |
|rowSum | 18 | 20 | 22 | 24 | 26 | 28 |

#### Sum
```SumToNewValue<TData>()``` computes sums of datapoints defined by a subset of values from a given dimension.
The method takes a new dimension value as a parameter that will define the resulting values.
The method also has an asyncronous variant ```SumToNewValueAsync<TData>()```.

##### Example
```csharp
    DimensionValue newDimensionValue = new("rowSum", new("en", "Sum value"));
    DimensionMap map = new("row0", ["row0-1", "row0-2"]);
    Matrix<DecimalDataValue> output = matrix.SumToNewValue(newDimensionValue, map);
```

```AddConstantToSubset<TData>()``` adds a constant to a subset of datapoints. Also has an asynchronous variant ```AddConstantToSubsetAsync<TData>()```.

##### Example
```csharp
    IMatrixMap map = new MatrixMap([
        new DimensionMap("col0", ["col0-0"]),
        new DimensionMap("col1", ["col1-0", "col1-1"]),
    ]);

    Matrix<DecimalDataValue> output = matrix.AddConstantToSubset(map, 5);
```

#### Multiplication
```MultiplyToNewValue<TData>()``` computes products of datapoints defined by a subset of values from a given dimension.
The method takes a new dimension value as a parameter that will define the resulting values.
The method also has an asyncronous variant ```MultiplyToNewValueAsync<TData>()```.  

##### Example
```csharp
    DimensionValue newDimensionValue = new("rowProduct", new("en", "Product value"));
    DimensionMap map = new("row0", ["row0-1", "row0-2"]);
    Matrix<DecimalDataValue> output = matrix.MultiplyToNewValue(newDimensionValue, map);
```

```MultiplySubsetByConstant<TData>()``` Multiply a subset of datapoints by a constant. Also has an asynchronous variant ```MultiplySubsetByConstantAsync<TData>()```.

##### Example
```csharp
    IMatrixMap map = new MatrixMap([
        new DimensionMap("col0", ["col0-0"]),
        new DimensionMap("col1", ["col1-0", "col1-1"]),
    ]);

    Matrix<DecimalDataValue> output = matrix.MultiplySubsetByConstant(map, 5);
```


#### Division
```DivideSubsetBySelectedValue<TData>()``` divides a subset of datapoints defined by values from one dimension with datapoints defined by a value from the same dimension.
Also has an asyncronous variant ```DivideSubsetBySelectedValueAsync()```

If the original matrix has the following structure:

|| col0-0 || col0-1 || col0-2 ||
||-----------|-----------|-----------|-----------|-----------|-----------|
|| col1-0 | col1-1| col1-0 | col1-1| col1-0 | col1-1 |
|row0-0| 0 | 1 | 2 | 3 | 4 | 5 |
|row0-1| 6 | 7 | 8 | 9 | 10 | 11 |
|row0-2| 12 | 13 | 14 | 15 | 16 | 17 |
|rowSum | 18 | 20 | 22 | 24 | 26 | 28 |

And we divide row dimension values row0-1 and row0-2 by rowSum the resulting matrix will look like this:

|| col0-0 || col0-1 || col0-2 ||
||-----------|-----------|-----------|-----------|-----------|-----------|
|| col1-0 | col1-1| col1-0 | col1-1| col1-0 | col1-1 |
|row0-0| 0 | 1 | 2 | 3 | 4 | 5 |
|row0-1| 0.33 | 0.35 | 0.36 | 0.38 | 0.39 | 0.41 |
|row0-2| 0.67 | 0.65 | 0.64 | 0.63 | 0.61 | 0.59 |
|rowSum | 18 | 20 | 22 | 24 | 26 | 28 |

##### Example
```csharp
    DimensionMap map = new("row0", ["row0-1", "row0-2"]);

    Matrix<DecimalDataValue> output = matrix.DivideSubsetBySelectedValue(map, "rowSum");
```

```DivideSubsetByConstant<TData>()``` Divide a subset of datapoints by a constant. Also has an asynchronous variant ```DivideSubsetByConstantAsync<TData>()```.

##### Example
```csharp
    IMatrixMap map = new MatrixMap([
        new DimensionMap("col0", ["col0-0"]),
        new DimensionMap("col1", ["col1-0", "col1-1"]),
    ]);

    Matrix<DecimalDataValue> output = matrix.DivideSubsetByConstant(map, 2);
```


#### General

```ApplyOverDimension<TData>()``` Generatas a new set datapoints by applying a function to datapoints defined by a subset of values from one dimension.
The method takes a new dimension value as a parameter that will define the resulting values.

```ApplyToSubMap<TData>()``` Applies a function to a set of datapoints.

```ApplyRelative<TData>()``` Applies a function to a set of datapoints defined by a subset of values from one dimension.
The method also takes a code of a value from the same dimension as a parameter which is used to define additional datapoints to be used as
an input for the function.
