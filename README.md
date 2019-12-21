# Accelbuffer - preview
`Accelbuffer` 是一个快速，高效的序列化系统，可以用于数据持久化、网络数据传输等。

## 运行时支持
* `.NET Framework 4.6+`
* `/unsafe`

## 特点
* 时间消耗低
* 托管堆内存分配无限接近于序列化对象的必要分配大小
* 对于值类型，无装箱、拆箱
* 可以完全自定义的序列化流程
* 自动的运行时代理注入 (测试版本，当前不受到完全支持)
* 自动的C#代理代码生成 [`accelc`] (正在安排开发)

## 部分功能
|功能名称|当前是否支持|
|:-:|:-:|
|简单类型序列化(`sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `char`, `string`, `float`, `double`, `bool`)|支持|
|字符编码设置(`ASCII`, `Unicode`, `UTF-8`)|支持|
|动态长度整数(`VariableInteger`)，固定长度整数(`FixedInteger`)|支持|
|序列化事件回调接口(`ISerializeMessageReceiver`)|支持|
|序列化数据损坏检查(`StrictMode`)|支持|
|运行时代理自动注入(`RuntimeSerializeProxyInjection`)|不完全支持|
|C#代理脚本自动生成(`accelc`)|目前不受支持|

## 基本用法
### 1.利用特性标记类型
#### 方案一，利用运行时代理注入(`RuntimeSerializeProxyInjection`)
```c#
[SerializeContract(InitialBufferSize = 20L, StrictMode = true)]
public struct UserInput
{
  [SerializedValue(0), VariableInteger] public int CarId;
  [SerializedValue(1)] public float Horizontal;
  [SerializedValue(2)] public float Vertical;
  [SerializedValue(3)] public float HandBrake;
}
```

#### 方案二，手动实现代理
```c#
[SerializeContract(typeof(UserInputSerializeProxy), InitialBufferSize = 20L, StrictMode = true)]
public struct UserInput
{
  public int CarId;
  public float Horizontal;
  public float Vertical;
  public float HandBrake;
}

public sealed class UserInputSerializeProxy : ISerializeProxy<UserInput>
{
  unsafe void ISerializeProxy<UserInput>.Serialize(in UserInput* obj, in OutputBuffer* buffer)
  {
    buffer->WriteValue(0, obj.CarId, false);
    buffer->WriteValue(1, obj.Horizontal);
    buffer->WriteValue(2, obj.Vertical);
    buffer->WriteValue(3, obj.HandBrake);
  }

  unsafe UserInput ISerializeProxy<UserInput>.Deserialize(in InputBuffer* buffer)
  {
    return new UserInput
    {
      CarId = buffer->ReadVariableInt32(0),
      Horizontal = buffer->ReadFloat32(1),
      Vertical = buffer->ReadFloat32(2),
      HandBrake = buffer->ReadFloat32(3)
    };
  }
}
```

#### 方案三，利用C#代理脚本自动生成(`accelc`)
- 即将被支持

### 2.序列化对象
```c#
UserInput input = new UserInput { CarId = 1, Horizontal = 0, Vertical = 0, HandBrake = 0 };
byte[] data = Serializer<UserInput>.Serialize(input);
```

### 3.反序列化对象
```c#
byte[] data = File.ReadAllBytes(someFile);
ArraySegment<byte> bytes = new ArraySegment<byte>(data);
UserInput input = Serializer<UserInput>.Deserialize(bytes);
```

### 4.释放缓冲区内存
- 缓冲区内存从非托管内存中分配
- 如果一个类型的序列化器经常被使用，可以选择不释放内存，以减少内存的频繁分配
```c#
Serializer<UserInput>.FreeBufferMemory();
```

### 5.序列化事件回调接口(`ISerializeMessageReceiver`)
通过实现`ISerializeMessageReceiver`接口关注`OnBeforeSerialize`和`OnAfterDeserialize`事件，类似Unity的`ISerializationCallbackReceiver`
- 对于值类型，该操作会导致装箱
```c#
[SerializeContract(InitialBufferSize = 20L, StrictMode = true)]
public struct UserInput : ISerializeMessageReceiver
{
  [SerializedValue(0), VariableInteger] public int CarId;
  [SerializedValue(1)] public float Horizontal;
  [SerializedValue(2)] public float Vertical;
  [SerializedValue(3)] public float HandBrake;
  
  //装箱
  void ISerializeMessageReceiver.OnBeforeSerialize()
  {
    UnityEngine.Debug.Log("OnBeforeSerialize");
  }
  
  //装箱
  void ISerializeMessageReceiver.OnAfterDeserialize()
  {
    UnityEngine.Debug.Log("OnAfterDeserialize");
  }
}
```

## 性能对比
> 数据不存在JIT的影响，但可能存在允许范围内的部分误差

- 测试类型

```C#
[Serializable]
[SerializeContract(InitialBufferSize = 20L, StrictMode = true)]
public struct StudentData
{
  [SerializedValue(0)] [Encoding(CharEncoding.ASCII)] public string Name;
  [SerializedValue(1)] [VariableInteger] public int Age;
  [SerializedValue(2)] [VariableInteger] public int Number;
  [SerializedValue(3)] public bool IsHighSchoolStudent;
}

StudentData data = new StudentData
{
  Name = "O",
  Age = 16,
  Number = 45,
  IsHighSchoolStudent = false
};
```

|序列化器名称|序列化 GC Alloc/字节|反序列化 GC Alloc/字节|序列化时间/纳秒|反序列化时间/纳秒|序列化文件大小/字节|
|:-:|:-:|:-:|:-:|:-:|:-:|
|Accelbuffer|85|68|2002|941|13|
|UnityJsonSerializer|188|68|2775|6463|61|
|.NET BinarySerializer|5427.2|5222.4|39300|41842|178|
|.NET XmlSerializer|7270.4|16486.4|86540|140405|279|

## 支持
* 作者正在努力更新部分新的功能，这个序列化系统原本只是作者开发的Unity开源框架(在~~很久~~不久后也会开源)的一部分，由于没有对Unity的依赖而被单独分离，在这个序列化系统的大部分功能完善后，会继续着手开发Unity框架，同时不定期维护这个项目，更多细节可以参考源码，部分注释将在今后补全。

* 作者联系方式 QQ：1024751595
