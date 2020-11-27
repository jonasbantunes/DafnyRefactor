# Dafny Refactor (Preview)

⚠️ Warning: this utility is in very early stages. It probably will break your existing source code.

Dafny Refactor is a utility for apply some refactors on dafny sources.

## Requirements

- Windows 10 (tested on 20H2, may work on previous versions)
- Visual Studio 2019
- .NET Framework 4.8

## Building

- Clone these projects:
    - [Boogie](https://github.com/boogie-org/boogie) (v2.4.2)
    - [Dafny](https://github.com/dafny-lang/dafny) (v2.3.0)
    - [Dafny Refactor](https://github.com/jonasbantunes/DafnyRefactor)
- Build _Boogie_ project located on `boogie\Source\Boogie.sln`
- Modify Dafny's _DafnyDriver_ project to output a Library instead of an Executable

```diff
// dafny/Source/DafnyDriver/DafnyDriver.csproj:9
- <OutputType>Exe</OutputType>
+ <OutputType>Library</OutputType>
```

- Build _Dafny_ project located on `dafny\Source\Dafny.sln`
- Build _DafnyRefactor_ project located on `DafnyRefactor\DafnyRefactor.sln`

## Usage

```batch
DafnyRefactor.exe [refactor-type] [refactor-params*]
```

Currently, the these refactors are supported:

- Extract Variable;
- Inline Temp;
- Move Method.

### Extract Variable

```batch
DafnyRefactor.exe apply-extract-variable [options]
```

Example:

```batch
DafnyRefator.exe apply-extract-variable -f example.dfy -s "2:7" -e "2:9" -v "newVarName"
```

### Inline Temp

```batch
DafnyRefactor.exe apply-inline-temp [options]
```

Example:

```batch
DafnyRefator.exe apply-inline-temp -f example.dfy -p "2:7"
```

### Move Method

```batch
DafnyRefactor.exe apply-move-method [options]
```

Example:

```batch
DafnyRefator.exe apply-move-method -f example.dfy -i "2:7"
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)
