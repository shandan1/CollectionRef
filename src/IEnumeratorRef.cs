/*
    This file belong to CollectionRef project,link address [https://github.com/ColinGao225/CollectionRef]
    Written by Colin Gao at 16th/5/2019  
    This project is forked from Microsoft source codes [https://github.com/dotnet/corefx]
    These have a MIT Lisence.That means you can use and  modify the code at all,in the condition that you specify the refferences in your project. 
 */
namespace Colin.CollectionRef
{
    /// <summary>
    /// refference version of Enumerator
    /// </summary>
    public interface IEnumeratorRef<T>
    {
        ref T CurrentRef { get; }
        bool MoveNext();
        void Reset();
    }
}
