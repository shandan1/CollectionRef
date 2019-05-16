# CollectionRef Package

CollectionRef project link address [github.com/ColinGao225/CollectionRef](https://github.com/ColinGao225/CollectionRef)  
Written by Colin Gao at 16th/5/2019  
This project is forked from Microsoft source codes [https://github.com/dotnet/corefx](https://github.com/dotnet/corefx)

These have a MIT Lisence.
That means you can use and  modify the code at all,in the condition that you specify the refferences in your project. 

## Summary
Basically, the package was rewritten from System.Collection.Generic package.  
the modifications include:
- Removed the feature that throwing exception when you modify the collection during Iterating it. 
- Add **IEnumableRef** and**IEnumatorRef** interfaces features to the collections.
- These allow you  to modify  the value directly during iterating the collections .  
- you can use the index feture to modify the value in collections ,such as **IndexOf()** , **GetValueAt()** ,**SetValueAt()** for **Dictionary**.
- Directly access to the underlying array of collections,this will increase efficiency for copying/moving data.
- You can compress/sort **Dictionary**.
- I add a CCollection class. Similar to the System.Collection.Generic.List class,but don't care about the order of data. 
When you do Insert/Remove methods,the class use swaping data instead of moving data.

**Caution:** To simplify the code and improve performance, I deleted most of the data checks.
When you invoke a method  with invalid parameters, exceptions may not thrown and may cause some memory leaks.
For example, when copy data to the underlying array, memory leaks will occur if insert data out the range of the collection.
If you particularly concern with data, you should add the data checks for each function on your own.

## Ref Iterator
**IEnumableRef** interface:
>public interface IEnumerableRef &lt;T>  
>{  
&nbsp; &nbsp; IEnumeratorRef &lt;T> GetEnumeratorRef();  
>}

**IEnumatorRef** interface:
> public interface IEnumeratorRef&lt;T>  
>{  
>&nbsp; &nbsp;ref T CurrentRef { get; }  
>&nbsp; &nbsp;bool MoveNext();  
>&nbsp; &nbsp;void Reset();  
>}

**Useage of the collection that implement the IEnumerableRef:**
>var aDic = new CDictionary<int, int>() {[2]=2, [3] =3,[4]=4};  
>var aIter = aDic.Values.GetEnumeratorRef();  
> while(aIter.MoveNext())  
>{  
>&nbsp; &nbsp;ref var aValue = ref aIter.CurrentRef;  
>&nbsp; &nbsp;aValue = 1;  
>}

## Sturcture changing Opt
There are basically two types of operations to colllections:
- Structure changing operations, including **Add** /**Replace** /**Remove** /**Sort** /**Compress** etc.
- Structure stable operations,includeing **IndexOf** /**GetValueAt** /**SetValueAt** /**ForeachRef** /**GetCount** etc.

Structure stable operations between two structure changing operations will have the same itarator results.Structure stable operations have a better performance.


