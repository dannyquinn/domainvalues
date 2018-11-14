## Domain Values Extension for Visual Studio

[![Build status](https://ci.appveyor.com/api/projects/status/uobgrdh8dkaolofn?svg=true)](https://ci.appveyor.com/project/dannyquinn/domainvalues)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)

Download this extension from the [VSGallery](https://marketplace.visualstudio.com/items?itemName=DannyQuinn.DomainValues) or get the [CI build](http://vsixgallery.com/author/danny%20quinn).

## Road map

- [ ] Visual Studio 2019 support
- [ ] Better support for new project format
- [ ] Type objects for retrieving values
- [x] Path of generator file appears in sql output (comment).

Features that have a checkmark are complete and available for
download in the [CI build](http://vsixgallery.com/author/danny%20quinn).

## Change log

These are the changes to each version that has been released
on the official Visual Studio extension gallery.

### 1.4

- [x] Table alignment - now aligns all rows in a block.
- [x] Support for comments (Ctrl+K, Ctrl+C / Ctrl+U).
- [x] Support for *Format Document* (Ctrl+K, Ctrl+D).
- [x] Support for *Format Selection* (Ctrl+K, Ctrl+F).
- [x] Fix - Text is not coloured correctly when a tab is used between keyword and parameter.
- [x] Fix - Removed reference to System in item templates to prevent error when using new project format.

### 1.3

- [x] Fix - Saving Empty Document causes "Sequence contains no elements" exception.

### 1.2

- [x] Update projects to 2017 format.
- [x] Replaced default icons.

### 1.1

- [x] Compatible with Visual Studio 2017

### 1.0

- [x] Initial release