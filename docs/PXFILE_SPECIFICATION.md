# Px-File Specification

Purpose of this document is to describe the syntax and content requirements for the pxfiles supported by this library. These requirements do not necessarily apply to other implementations of the pxfile format.

## Syntax

### Entries
The px file consists of list of entries. Each entry consists of two parts: the key and the value.
Which are separated by an equal sign ```=```. Entries are separated by a semicolon ```;```.

Example entry:
``` KEYWORD[en]("Dimension_name")="Some value text here";```

#### Recommendations
- Each entry must be on a new line.

### Keys
Keys consist of three parts: the keyword name, the language code and the specifier. The parts must always be in that order, but only the keyword is mandatory.
In the example key ```KEYWORD[en]("Dimension_name")```:
	- ```KEYWORD``` is the keyword.
	- ```en``` is the language code.
	- ```"Dimension_name"``` is the specifier.

Whitespace characters are not significant in the key.

#### Recommendations
- Do not use whitespace characters in the key.

#### Keyword
- Can contain uppercase letters ```A-Z```, numbers ```0-9``` and hyphen ```-```, lowercase letters ```a-z``` and underscore ```_``` are allowed but not recommended.
- Must start with a letter.
- No whitespace is allowed.

##### Recommendations
- Only use uppercase letters.
- Use hyphens ```-``` to separate words.
- Don't use more that 20 characters.

#### Language code
- The code string can not contain ```;``` ```=``` ```[``` ```]``` or ```"```.
- The code string can not contain whitespace characters.
- ```[``` character marks the beginning of the language code.
- ```]``` character marks the end of the language code.
- Whitespace characters are not allowed inside the ```[ ]``` brackets.

##### Recommendations
- Use the ISO 639 language codes or either BCP 47 or MS-LCID language tags.

#### Specifier
- The specifier consists of one or two strings marked by ```"``` characters and separated by a comma ```,```.
- The specifier strings can not contain ```;``` or ```"``` characters.
- ```(``` character marks the beginning of the specifier.
- ```)``` character marks the end of the specifier.
- Only the ```"``` marked string, whitespace characters and commas are allowed inside the specifier brackets.
- Preceding or trailing commas ```,``` are not allowed.
- Whitespace characters outside of of the ```"``` character marked string are not significant.

Note that the CELLNOTE keyword is an exception to the specifier rule. Support and syntax for is being discussed at the time of writing this document.

##### Recommendations
- Only use one whitespace character after the comma and no other whitespace characters.

The string before the comma is considered **The first specifier value**
The string after it is considered **The second specifier value**

Example keys with specifiers:
```KEYWORD[en]("Dimension name")```
```KEYWORD("Dimension name")```
```KEYWORD[en]("Dimension name", "Dimension value name")```
```KEYWORD("Dimension name", "Dimension value name")```

### Values
Value can be in any of the following formats:
- A string of characters.
- A list of strings.
- A number.
- A boolean value.
- Keyword specific value types (see "Special entries").

Whitespace characters are not significant in the value outside of ```"``` separated strings.

#### String
- The string value begin and end with ```"``` character.
- A string value can contain any characters except ```"```.
- If the string is split into multiple lines, each line must begin and end with ```"``` characters.

#### Datetime
- String value in the format ```YYYYMMDD HH:MM```.

#### Boolean
- The boolean value is either ```YES``` or ```NO``` written in capital letters.
- Whitespace characters are not significant in the value (as long as the ```YES``` and ```NO``` remain intact).

#### Number
- The number value can contain only numbers ```0-9```, a single period ```.``` and a single minus sign ```-```.
- Decimal separator is period ```.```.
- Thousands separator is not allowed.
- Whitespace characters are not allowed between the characters of the number.
- Number must be in range of ±7.9228 x 10^28.

#### List of strings
- List items are separated by a comma ```,```.
- Rules of the string value type apply to the list items, except one list item can not be split into multiple lines.
- Preceding and trailing commas are not allowed.

#### Recommendations
- Avoid excessive use of whitespace characters outside of strings.
- Split only strings that exceed 150 characters into multiple lines.

### Special entries
Some keywords have special value types. These are described in the following sections.

#### DATA
- The value of the DATA entry is a list of values that can be either string or number.
- The list items are separated by a space (ASCII hex ```20```) or tab (ASCII hex ```09```).
- The data list can be split into multiple lines
- Each line must end with a space or tab character (ASCII hex ```20```) or (ASCII hex ```09```).
- Only one kind of separator can be used in the same list.
- The number values are subject to the same rules as the number value type.
- The strings have a limited set of contents that are allowed: one to six dots ```.``` that mark missing values or hyphen ```-``` that marks exact zero.
	- List of allowed strings: ```.```, ```..```, ```...```, ```....```, ```.....```, ```......```, ```-```.
- The strings must be marked by ```"``` characters.

##### TIMEVAL
- Timeval has two value types unique to the entry:
	- **Interval specifier** is one of the following: ```A1```, ```H1```, ```Q1```, ```M1``` , ```W1```.
	- The interval specifiers are not strings, so they are not marked by ```"``` characters.
	- **timestamp** depends on the interval specifier:
		- A1 -> ```YYYY``` where ```Y``` is ```0-9```.
		- H1 -> ```YYYYH```, where ```Y``` is ```0-9``` and ```H``` is ```1``` or ```2```.
		- T1 -> ```YYYYT```, where ```Y``` is ```0-9``` and ```T``` is ```1-3```.
		- Q1 -> ```YYYYQ```, where ```Y``` is ```0-9``` and ```Q``` is ```1-4```.
		- M1 -> ```YYYYMM```, where ```Y``` is ```0-9``` and ```MM``` is ```01-12``` (note the leading zero).
		- W1 -> ```YYYYWW```, where ```Y``` is ```0-9``` and ```WW``` is ```01-52``` (note the leading zero).
		- D1 -> ```YYYYMMDD```, where ```Y``` is ```0-9```, ```MM``` is ```01-12``` and ```DD``` is ```01-31``` (note the leading zeros).
		- The timestamps are always considered to be strings and must be marked by ```"``` characters.
- TIMEVAL value van be in one of two formats:
	1. a token ```TLIST(XX)``` followed by a list of timestamps, where ```XX``` is **the interval specifier**.
		- The TLIST(XX) token is not a string, so it is not marked by ```"``` characters.
		- The following list of timestamps is subject to the same rules as the list of strings value type.
		- The TLIST(XX) token and the list of string are separated by a comma ```,``` (the token can be viewed as the fist item on the list).
		- Example TIMEVAL value in this format: ```TLIST(A1),"2000","2001","2002"```
	2. The range format:
		- In the range format the token is in the following form: ```TLIST(XX, AAAA-ZZZZ)``` where ```XX``` is the interval specifier and ```AAAA``` and ```ZZZZ``` are timestamps.
		- In this format no other items are allowed after the token.
		- Example TIMEVAL value in this format: ```TLIST(A1, "2000-2002")```

## Content requirements

### About the languages
The default language of the file is defined by the LANGUAGE entry. If the file has more than one language, they are defined by the LANGUAGES entry (including the default). Language codes used in the file must all be found in the LANGUAGES entry. If a language code is found in the file that is not found in the LANGUAGES entry, the file is not valid.

Most entries in px-files are language dependent and the language of the entry is defined by the language code in the key. The default language is an exception, specifying it in the key is optional.
Language dependant keywords **must** have an entry for each language defined in the LANGUAGES entry per unique set of specifiers.

If a keyword has entries in multiple languages with same specifiers:
- The language code in the key must match one of the codes in the LANGUAGES entry.
- For entries with that keyword, but without language code in the key, the default language is assumed.

If a keyword has only one entry for a set of specifiers, the entry is considered to not depend on the language.

For files with only one language, the language codes in the keys can be omitted from the file.

### About the specifiers
The specifiers are used to map the entry to the whole matrix, a spesific dimension or dimension value.
In the general case the first spesifier is used to provide the name of the dimension and the second is used to provide the name of the value.
The content dimension is an exception, entries releted to it can be defined without a second specifier and the first spesifier is used to provide the name of the content dimension value. **This however is not recommended**.

If the entry is related to the whole matrix, the spesifiers are not used. If the entry is not dependant on any dimension or dimension value, using a specifier **is considered an error**.

### Mandatory entries

The keywords marked with an asterisk (*) are mandatory with conditions. The conditions are explained in the section of the keyword.

#### CHARSET
Value must be a string. Either ```ANSI``` or ```Unicode```. The value must also match the encoding of the file. If the file is not readable in the encoding specified, the file is not valid.
- This entry is language independent.
- This entry does not depend on any dimensions or dimension values.

#### CODEPAGE
More spesific encoding information. The value must be a string that matches the encoding of the file. If the file is not readable in the encoding specified, the file is not valid.
- If the CHARSET entry is ```ANSI```, the value must be the exact name of the encoding used. IE: ```ISO-8859-1```.
- If the CHARSET entry is ```Unicode```, the value must be the exact name of the encoding used. IE: ```UTF-8```, ```UTF-16``` etc.
- Values are not case sensitive, but uppercase characters are **recommended**.
- This entry is language independent.
- This entry does not depend on any dimensions or dimension values.

#### LANGUAGE
- The value must be a string.
- Syntax -> Keys -> Language code rules and recommendations apply to the value.
- This entry does not depend on any dimensions or dimension values.

#### LANGUAGES*
- This entry is required **IF** the file contains more than one language.
- The value must be a list of strings.
- Syntax -> Keys -> Language code rules and recommendations apply to each value.
- This entry does not depend on any dimensions or dimension values.

#### STUB*
- Can be omitted if all the dimensions are defined in the HEADING.
- Defines the dimensions of the matrix which are placed on the rows.
- The value must be a list of strings.
- The values are the names of the dimensions.
- Language dependant.

#### HEADING*
- Can be omitted if all the dimensions are defined in the STUB.
- Defines the dimensions of the matrix which are placed on the columns.
- The value must be a list of strings.
- The values are the names of the dimensions.
- Language dependant.

#### VALUES
- Defines the dimension values of one dimension per entry.
- Must be defined for each dimension in STUB and HEADING.
- The value must be a list of strings.
- Language dependant.

#### DATA
- Defines the data of the matrix.
- Length of one row must be exactly ```Product of number of values in each dimension defined with the HEADING keyword```
- The number of rows must be exactly ```Product of number of values in each dimension defined with the STUB keyword```
- No language code or specifiers are allowed in the key.

#### If CONTVARIABLE is defined, the following entries are mandatory:
##### UNITS*
- The value must be a string.
- Must be defined for each value of the dimension defined with the CONTVARIABLE keyword.
- Can be defined for the whole matrix without spesifiers.
- Recommended to be defined for the content dimension value with two specifiers.
- Can be defined for the content dimension values with the value name as only the first specifier, but this is not recommended.
- Language dependant.

##### LAST-UPDATED*
- Datetime value.
- Must be defined for each value of the dimension defined with the CONTVARIABLE keyword.
- Can be defined for the whole matrix without spesifiers.
- Recommended to be defined for the content dimension value with two specifiers.
- Can be defined for the content dimension values with the value name as only the first specifier, but this is not recommended.
- Can be defined for each language, but this is not recommended.

##### PRECISION*
- The value must be an integer (and valid number type).
- Recommended to be defined for each value of the dimension defined with the CONTVARIABLE keyword.
- Must be defined for each value where the number of decimals in the data is not zero (This is difficult to validate due to decimal rounding, so zero is assumed when not defined).
- Can be defined for the whole matrix without spesifiers.
- Recommended to be defined for the content dimension value with two specifiers.
- Can be defined for the content dimension values with the value name as only the first specifier, but this is not recommended.
- Can be defined for each language, but this is not recommended.

### Recommended entries
#### TABLEID
- The value must be a string.
- Unique identifier for the table within the database.
- This entry must be language independent.

#### DESCRIPTION
- The value must be a string.
- Language dependant.

#### CONTVARIABLE
- The value must be a string.
- Defines the content dimension.
- Value must be one of the dimension names defined in the STUB or HEADING.
- Language dependant.

#### VARIABLECODE
- The value must be a string.
- Must be defined for each dimension defined by the STUB or HEADING.
- One entry per dimension per language.
- Language dependant.

#### CODES
- The value must be a list of strings.
- Lenth of the list must be equal to the number of values in the dimension defined by the VALUES entries.
- Must be defined for each variable defined by the STUB or HEADING.
- Language dependant.

#### VARIABLE-TYPE
- The value must be a string.
- Recommended that the variable type is defined for each variable defined by the STUB or HEADING.
- Has a set of allowed values: ```Content```, ```Time```, ```Geographical```, ```Ordinal```, ```Nominal```, ```Other```, ```Unknown```.
- Can be defined for each language, but this is not recommended.

#### TIMEVAL
- See the TIMEVAL entry for the syntax and content requirements.