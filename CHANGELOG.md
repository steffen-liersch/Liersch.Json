# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/) and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]

## [1.0.0] - 2019-05-13
### Added
- CHANGELOG.md introduced
- Conditional directives added to platform-specific files
- Constructor constraint added to SLJsonDeserializer.Deserialize
- Function SLJsonParser.Parse(string jsonExpression, bool allowArraysAndValues) added
- Functions CreateEmptyArray and CreateEmptyObject added to SLJsonNode
- Support for List<T> added to SLJsonDeserializer and SLJsonSerializer
- Property SLJsonTokenizer.AreSingleQuotesEnabled
- Non-static functions in class SLJsonParser: ParseAny and ParseObject
- SLJsonNode.Parse
### Changed
- Signing assemblies
- Using AssemblyInfo.cs for all library projects
- Function SLJsonNode.Serialize optimized and handling for nodes of type SLJsonNodeType.Missing added
- Projects and demos reworked, tests moved to test project
- Using a single project file for multiple target frameworks (VS2017)
- Using the same library name for all target frameworks
- SLJsonParser.Parse changed to work more restrictive
- SLJsonParser can be instantiated to specify different options:
  AreSingleQuotesAllowed, AreUnquotedNamesAllowed, IsNumericCheckDisabled
### Fixed
- SLJsonNode changed to use SortedDictionary for SLJsonNodeType.Object (except .NET MF)
- Function SLJsonNode.ToString fixed for SLJsonNodeType.Object
- SLJsonTokenizer and SLJsonWriter improved to support all required escape sequences
- NullReferenceException in SLJsonDeserializer and SLJsonSerializer on missing getter or setter
- NullReferenceException in implicit type conversion on null value

[Unreleased]: https://github.com/steffen-liersch/Liersch.Json/compare/v1.0.0...HEAD
[1.0.0]:      https://github.com/steffen-liersch/Liersch.Json/tree/v1.0.0
