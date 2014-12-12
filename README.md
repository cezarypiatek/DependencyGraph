DependencyGraph
===============

A small F# application to visualize .NET assembly dependencies.
It use GraphViz library do draw dependency graph.

![Sample screenshot](https://raw.githubusercontent.com/cezarypiatek/DependencyGraph/master/doc/screen01.jpg)

##Solution structure
**DependencyGraph** -  main application

**CopyGraphViz** - msbuild project responsible for copy all GraphViz files to output directory (fsproj currently doesn't support wildcards so it's unable to copy whole directory using copy task)
