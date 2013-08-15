#Live

My go at enabling live-coding (however limited) in Visual Studio.

##Requirements
You will need:
+ Visual Studio 2012 (Version used to create this)
+ Roslyn CTP
+ Visual Studio SDK

## Live-coding?

Live-coding is the process of writing code and being able to see in realtime (or near realtime) the results of that code. The inspiration for the project came from some groundwork done by Bret Victor http://www.worrydream.com.

## How will it work?

C# is fundamentally adverse to live-coding because of its compiled nature. However, certain methods can be taken out of context and run standalone or with minimal referencing.

```csharp
public int Add(int x, int) {
	return x + y;
}

public object[] reverse(object[] arr)
{
    object[] nArr = new object[arr.Length];
    for(int i = 0; i < arr.Length; i++)
    {
        nArr[(arr.Length - 1) - i] = arr[i];
    }
    return nArr;
}
```
The above methods are simple examples of methods that can be taken out of context of the application and executed alone.

By analysing the source at edit-time and lifting such methods into a scriptable context e.g. Roslyn scripting engine, then they could be "compiled" and executed in near-real-time as you write them.

Using Visual Studio extensibility features, the information can then be surfaced via editor adornments / glyphs / tool windows etc.

## Why?

All too often algorithms can be difficult to comprehend and hold entirely in memory. Development still requires the developer to dry-run code in their head as they write it. Our computers are perfectly capable of doing this for us, so why aren't they?
