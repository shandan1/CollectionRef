# CollectionRef 包

CollectionRef包 项目链接地址 [github.com/ColinGao225/CollectionRef](https://github.com/ColinGao225/CollectionRef)  
由 Colin Gao 在 2019/5/16 书写  
该工程从微软源代码项目[https://github.com/dotnet/corefx](https://github.com/dotnet/corefx)分支而来.  
工程拥有MIT许可,这意味着 在工程中指明引用和出处的情况下,你可以任意修改和使用源码.

## 概述
基本来说该包重写了System.Collection.Generic包,具体修改包括:
* 删除了遍历过程中修改集合将抛出异常的特性.  
* 为集合类添加 **IEnumableRef** 和**IEnumatorRef**的接口实现.
这两个接口将允许遍历获取集合的数据地址,这能够在不修改集合结构的情况下,批量修改集合的值.  
* 使用索引的方式,例如**IndexOf()** , **GetValueAt()** ,**SetValueAt()** 方法访问修改Dictionary字典的Value值.
* 可以直接访问集合的底层数组,这将方便批量转移复制.
* 增加了CCollection集合类,类似于System.Collection.Generic.List类,
但不再考虑数据的顺序.当执行增删操作将不再平移底层数组.

**注意:** 为了简化代码和提高性能的原因,我删除了多数的数据检查.
当调用使用了无效的参数时,可能将不再抛出异常,并导致内存泄露.
例如在访问底层数组时,插入的数据超过集合的范围,就会发生内存泄露的情况.
如果尤其在意数据安全的问题,可以自行为每个函数添加数据检查.

## 引用遍历
**IEnumableRef**接口定义:
>public interface IEnumerableRef &lt;T>  
>{  
&nbsp; &nbsp; IEnumeratorRef &lt;T> GetEnumeratorRef();  
>}

**IEnumatorRef**接口定义:
> public interface IEnumeratorRef&lt;T>  
>{  
>&nbsp; &nbsp;ref T CurrentRef { get; }  
>&nbsp; &nbsp;bool MoveNext();  
>&nbsp; &nbsp;void Reset();  
>}

**实现引用遍历集合**的使用:
>var aDic = new CDictionary<int, int>() {[2]=2, [3] =3,[4]=4};  
>var aIter = aDic.Values.GetEnumeratorRef();  
> while(aIter.MoveNext())  
>{  
>&nbsp; &nbsp;ref var aValue = ref aIter.CurrentRef;  
>&nbsp; &nbsp;aValue = 1;  
>}

## 结构变化操作
对集合的多种操作中,基本上分为两类:
- 结构变化操作,包含Add增加 Remove删,Sort排序,Compress压缩等.
- 结构不变操作,包括GetValue取值 SetValue赋值 Foreach遍历,Count获取长度等.

两次结构变化操作之间的 多次结构不变操作将拥有一致的遍历结果,并拥有更好的执行效率.


