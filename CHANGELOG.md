# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/) and this project adheres to [Semantic Versioning](http://semver.org/).

## Unreleased
### Added
- CHANGELOG.md introduced
### Changed
- Signing assemblies
- Using AssemblyInfo.cs for all library projects
- Function SLJsonNode.Serialize optimized and handling for nodes of type SLJsonNodeType.Missing added
- Conditional directives added to platform-specific files
### Fixed
- SLJsonNode changed to use SortedDictionary for SLJsonNodeType.Object (except .NET MF)
- Function SLJsonNode.ToString fixed for SLJsonNodeType.Object
