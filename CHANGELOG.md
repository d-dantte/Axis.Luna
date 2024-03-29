## Release 6.0.28 - 2023/12/10
* Refactors `Axis.Luna.Extension` and `Axis.Luna.Common` to remove the `.Throws(..., *Exception)` methods. There is a performance penalty
  for creating the exceptions on the hot-path.
* Added a few minor readability changes.
* Added `RollingHash` type for calculating rolling hashes (using polynomial hashing) of sequences.


## Release 6.0.21 - 2023/09/02
* Added `MapAs<>` method to the `IResult` API. [`Axis.Luna.Common`]
* Added `HasGenericTypeDefinition` method to the `Type` Extensions. [`Axis.Luna.Extensions`]
* Fixed a bug in `ConstructorInvoker` that failed when constructing structs. [`Axis.Luna.FInvoke`]


## Release Axis.Luna.Common 6.0.6 - 2023/04/29
* Removed everything under `.Types.Basic.*`, reason being the `Axis.Ion` Library is a lot more matured than this.
* Introduced the `BigDecimal` type.
* Refactored `IResult` types, moving their namespaace to `Axis.Luna.Common.Results`.


## Release 6.0.3 - 2022/10/23
* Axis.Luna.Common
    * Refactored `*.Types.Basic` types

* Axis.Luna.Common.NewtonsoftJson
    * Refactored the `BasicStructJsonConverter` type

* Axis.Luna.Extensions
    * Refactored `SecureCommon`, made it into a static class, since the RNG crypto type is now deprecated.


## Release 6.0.2 - 2022/10/08
* Axis.*
	* Added support for the `IResult` type.
	* Added `Newtonsoft.Json` Converter for the `IResult` type.
	* Added support for resolving `IOperation` instances into `IResult` instances.
	* Added support for resolving `Lazy<T>� into `IResult<T>`
	* Fixed bug in `FInvoke` that causes `ArgumentException` to be thrown if the wrong owner-type is passed into the DynamicMethod constructor.


## Release 6.0.0 - 2022/08/27
* Axis.*
	* Added support for `net6.0`
	* Added `EventTimer` type.
	* Added more methods for `AsyncLock`
	* Added `Fold` extension methods for Operations.
	* Covariance support for Operations via `IOperation` is not fully implemented.


## Released 5.1.0 - 2020/05/04

* Axis.Luna.Extensions
	* Added Permutation Extension for Enumerables
	* Syntax sugar
* Axis.Luna.Operation
	* Removed explicit Rollback logic - this can be implemented using a "then"
	* Converted AsyncAwaiter to struct
	* Converted LazyAwaiter to struct
	* Added a CustomLazy that is aware of the initialization state of the inner lazy
	* Converted SyncAwaiter to struct