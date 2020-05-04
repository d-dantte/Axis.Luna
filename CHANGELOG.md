
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