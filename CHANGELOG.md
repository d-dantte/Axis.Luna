
## Release 6.0.2 - 2022/10/08
* Axis.*
	* Added support for the `IResult` type.
	* Added `Newtonsoft.Json` Converter for the `IResult` type.
	* Added support for resolving `IOperation` instances into `IResult` instances.
	* Added support for resolving `Lazy<T>¬ into `IResult<T>`
	* Fixed bug in `FInvoke` that causes `ArgumentException` to be thrown if the wrong owner-type is passed into the DynamicMethod constructor.
	*


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