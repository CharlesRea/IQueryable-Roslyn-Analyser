# IQueryable implicit conversion to IEnumerable Roslyn analyser

A Roslyn analyser which emits a warning when an `IQueryable<T>` is implicitly converted to `IEnumerable<T>`.

Normally this is unintended behaviour, since the query will be run as is once the `IEnumerable` is enumerated, and it makes the code hard to read as it hard to see where the query ends and when in memory execution begins. Usually you want to make the conversion explicit, by calling one of the
methods on IQueryable to terminate the query - such as `ToListAsync`, `FirstOrDefault`, etc.

This was written as an experiment in understanding how Roslyn analysers work.
