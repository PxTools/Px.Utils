# Px-File Specification

Purpose of this document is to describe the syntax and content requirements for the pxfiles supported by this library. These requirements do not necessarily apply to other implementations of the pxfile format.

## Syntax

### Entries
The px file consists of list of entries. Each entry consists of two parts: the key and the value.
Which are reparated by an equal sign ```=```. Entries are separated by a semicolon ```;```.

Example entry:
``` KEYWORD[en]("Dimension_name")="Some value text here";```

#### Recommendations
- Each entry should be on a new line.

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
- Can contain only letters ```aA-zZ```, numbers ```0-9```, underscores ```_``` and hyphens ```-```.
- Must start with a letter.
- No whitespace is allowed.

##### Recommendations
- Only use uppercase letters.
- Use hyphens ```-``` to separate words.
- Dont use more that 20 characters.

#### Language code
- The code string can not contain ```;``` ```=``` ```[``` ```]``` or ```"```.
- The code string can not contain whitespace characters.
- ```[``` character marks the beginning of the language code.
- ```]``` character marks the end of the language code.
- Whitespace characters are not allowed inside the ```[ ]``` brackets.

##### Recommendations
- Use the ISO 639 language codes or BCP 47 language tags.

#### Specifier
- The spesifier consists of one or two strings marked by ```"``` characters and separated by a comma ```,```.
- The specifier strings can not contain ```;``` or ```"``` characters.
- ```(``` character marks the beginning of the specifier.
- ```)``` character marks the end of the specifier.
- Only the ```"``` marked string, whitespace characters and commas are allowed inside the specifier brackets.
- Trailing commas ```,``` are not allowed.
- Whitespace characters outside of of the ```"``` character marked string are not significant.

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

#### Boolean
- The boolean value is either ```YES``` or ```NO```.
- Whitespace characters are not significant in the value (as long as the ```YES``` and ```NO``` remain intact).

#### Number
- The number value can contain only numbers ```0-9```, a single dot ```.``` and a single minus sign ```-```.
- Decimal separator is dot ```.```.
- Thousands separator is not allowed.
- Whitespace characters are not allowed between the characters of the number.

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
- The list items are separated by a whitespace (ASCII hex ```20```).
- The data list can be split into multiple lines
- Each line must end with a whitespace character (ASCII hex ```20```).
- The number values are subject to the same rules as the number value type.
- The strings have a limited set of contents that are allowed: one to six dots ```.``` that mark missing values.
	- List of allowed strings: ```.```, ```..```, ```...```, ```....```, ```.....```, ```......```.
- The strings must be marked by ```"``` characters.

##### TIMEVAL
- Timeval has two value types unique to the entry:
	- **Interval specifier** is one of the following: ```A1```, ```H1```, ```Q1```, ```M1``` , ```W1```.
	- The interval specifiers are not strings, so they are not marked by ```"``` characters.
	- **timestamp** depends on the interval specifier:
		- A1 -> ```YYYY``` where ```Y``` is ```0-9```.
		- H1 -> ```YYYYH```, where ```Y``` is ```0-9``` and ```H``` is ```1``` or ```2```.
		- Q1 -> ```YYYYQ```, where ```Y``` is ```0-9``` and ```Q``` is ```1-4```.
		- M1 -> ```YYYYMM```, where ```Y``` is ```0-9``` and ```Q``` is ```01-12``` (note the leading zero).
		- W1 -> ```YYYYWW```, where ```Y``` is ```0-9``` and ```Q``` is ```01-52``` (note the leading zero).
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

### About the spesifiers
The specifiers are used to map the entry to the whole matrix, a spesific dimension or dimension value.
In the general case the first spesifier is used to provide the name of the dimension and the second is used to provide the name of the value.
The content dimension is an exception, entries releted to it do not have a second spesifier and the first spesifier is used to provide the name of the content dimension value.
If the entry is related to the whole matrix, the spesifiers are not used.

### Mandatory entries
CHARSET
CODEPAGE
LANGUAGE
STUB
HEADING
VALUES
DATA

If CONTVARIABLE is defines, the following entries are mandatory:
UNITS
LAST-UPDATED
PRECISION

### Recommended entries
TABLEID
DESCRIPTION
CONTVARIABLE
VARIABLECODE
CODES
VARIABLE-TYPE
TIMEVAL