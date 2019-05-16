# CollectionRef ��

CollectionRef�� ��Ŀ���ӵ�ַ [github.com/ColinGao225/CollectionRef](https://github.com/ColinGao225/CollectionRef)  
�� Colin Gao �� 2019/5/16 ��д  
�ù��̴�΢��Դ������Ŀ[https://github.com/dotnet/corefx](https://github.com/dotnet/corefx)��֧����.  
����ӵ��MIT���,����ζ�� �ڹ�����ָ�����úͳ����������,����������޸ĺ�ʹ��Դ��.

## ����
������˵�ð���д��System.Collection.Generic��,�����޸İ���:
* ɾ���˱����������޸ļ��Ͻ��׳��쳣������.  
* Ϊ��������� **IEnumableRef** ��**IEnumatorRef**�Ľӿ�ʵ��.
�������ӿڽ����������ȡ���ϵ����ݵ�ַ,���ܹ��ڲ��޸ļ��Ͻṹ�������,�����޸ļ��ϵ�ֵ.  
* ʹ�������ķ�ʽ,����**IndexOf()** , **GetValueAt()** ,**SetValueAt()** ���������޸�Dictionary�ֵ��Valueֵ.
* ����ֱ�ӷ��ʼ��ϵĵײ�����,�⽫��������ת�Ƹ���.
* ������CCollection������,������System.Collection.Generic.List��,
�����ٿ������ݵ�˳��.��ִ����ɾ����������ƽ�Ƶײ�����.

**ע��:** Ϊ�˼򻯴����������ܵ�ԭ��,��ɾ���˶��������ݼ��.
������ʹ������Ч�Ĳ���ʱ,���ܽ������׳��쳣,�������ڴ�й¶.
�����ڷ��ʵײ�����ʱ,��������ݳ������ϵķ�Χ,�ͻᷢ���ڴ�й¶�����.
��������������ݰ�ȫ������,��������Ϊÿ������������ݼ��.

## ���ñ���
**IEnumableRef**�ӿڶ���:
>public interface IEnumerableRef &lt;T>  
>{  
&nbsp; &nbsp; IEnumeratorRef &lt;T> GetEnumeratorRef();  
>}

**IEnumatorRef**�ӿڶ���:
> public interface IEnumeratorRef&lt;T>  
>{  
>&nbsp; &nbsp;ref T CurrentRef { get; }  
>&nbsp; &nbsp;bool MoveNext();  
>&nbsp; &nbsp;void Reset();  
>}

**ʵ�����ñ�������**��ʹ��:
>var aDic = new CDictionary<int, int>() {[2]=2, [3] =3,[4]=4};  
>var aIter = aDic.Values.GetEnumeratorRef();  
> while(aIter.MoveNext())  
>{  
>&nbsp; &nbsp;ref var aValue = ref aIter.CurrentRef;  
>&nbsp; &nbsp;aValue = 1;  
>}

## �ṹ�仯����
�Լ��ϵĶ��ֲ�����,�����Ϸ�Ϊ����:
- �ṹ�仯����,����Add���� Removeɾ,Sort����,Compressѹ����.
- �ṹ�������,����GetValueȡֵ SetValue��ֵ Foreach����,Count��ȡ���ȵ�.

���νṹ�仯����֮��� ��νṹ���������ӵ��һ�µı������,��ӵ�и��õ�ִ��Ч��.


