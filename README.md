# NighlyCode.Json

Minimalistic Library used to write values and objects to json and read json data.

## Usage

### Write Json String

```
string json=Json.WriteString(<data>);
```

### Write Json to Stream

```
Json.Write(<data>,<stream>);
```

### Read Json to basic structures
The following code reads a json string or json data in a stream and converts it to framework structures.
Objects are read as **Dictionary<string,object>**, Arrays as **List** and values as string, double, long, boolean or null.

```
object data=Json.Read(<string|stream>)
```

### Read Json to a specific type
The following code reads a json string or json data and converts it to the specified type.

```
object data=Json.Read(<type>, <string|stream>)
```
```
object data=Json.Read<T>(<string|stream>)
```

### Options
The behavior when writing json can be modified by specifying **JsonOptions** as last parameter to any call to **Write**.

|Property||
|-|-|
|ExcludeNullProperties|specify true to suppress writing of any null values|
|NamingStrategy|Delegate called when writing property names. Standard strategies are available under using the **NamingStrategies** class|

### Custom Value Types
If values are to be converted to different types than well known framework types a custom converter can be specified with

```
Json.SetCustomConverter(<sourcetype>,<targettype>,<delegate>)
```

The specified delegate is valled when a value of **sourcetype** is read and is to be converted to a property of type **targettype**

## FAQ

### Why another Json Lib?

This library was written for a specific project where other libraries were either to overloaded with unnecessary stuff
or just too slow in actual usage.

### So this is the fastest existing library out there?

I don't think so.
While it generally should perform well enough and was tested in some projects it is not optimized in any way at all.
If you need the fastest library of them all you probably should look somewhere else.

Btw. Async methods are proven to be terribly slow as they are only implemented currently to work in async cases. Some refactoring
will be done here later on when everything else seems to work well enough.

### But then it definitively is the smallest library out there?

Probably not ... It was written to just contain the stuff needed to get the job done but again no specific steps were taken
to minimize size.

### Then why bother

Honestly ... if you don't know by now you probably shouldn't. This library is supposed to stay like it is and while obvious bugs or flaws will get fixed
it will not receive any major feature upgrades since it kind of defies the purpose of this library. So don't expect this libary to receive
serialization of expression trees, native code generation or whatever ...