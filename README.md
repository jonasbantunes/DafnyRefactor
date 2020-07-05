# Dafny Refactor

Dafny Refactor is a utility for apply some refactors on dafny sources.

## Requirements

- Windows 10 (tested on 2004 version, may work on previous versions)
- Visual Studio 2019
- NetFramework v4.5

## Installation

- Clone these projects:
    - [Boogie](https://github.com/boogie-org/boogie) (v2.4.2)
    - [Dafny](https://github.com/dafny-lang/dafny) (v2.3.0)
    - [Dafny Refactor](https://github.com/jonasbantunes/DafnyRefactor)
- Build _Boogie_ project located on `boogie\Source\Boogie.sln`
- Modify Dafny's _DafnyDriver_ project to output a Library instead of an Executable

```diff
// DafnyDriver.csproj:9
- <OutputType>Exe</OutputType>
+ <OutputType>Library</OutputType>
```

- Build _Dafny_ project located on `dafny\Source\Dafny.sln`
- Build _Dafny Refactor_ project located on `DafnyRefactor\DafnyRefactor\DafnyRefactor.sln`

## Usage

```batch
DafnyRefactor.exe [path-to-dafny-source]
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)
