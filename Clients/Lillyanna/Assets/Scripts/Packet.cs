using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class Packet : IDisposable
{
    public string id;
    public string sender;
    public List<object> data = new List<object>();

    public Packet(string sender = default(string), List<object> data = default(List<object>))
    {
        id = Guid.NewGuid().ToString().Split('-')[0];
        this.sender = string.IsNullOrEmpty(sender) ? "N/A" : sender;
        this.data = data == null ? new List<object>() : data;
    }

    public void WriteString(string value)
    {
        if (!string.IsNullOrEmpty(value))
            data.Add(value);
    }
    public void WriteInt(int value)
    {
        data.Add(value);
    }
    public void WriteFloat(float value)
    {
        data.Add(value);
    }
    public void WriteBool(bool value)
    {
        data.Add(value);
    }
    public void WriteObject(object value)
    {
        if (value != null)
            data.Add(value);
    }

    public string ReadString()
    {
        try
        {
            var value = (string)data[0];
            data.RemoveAt(0);
            return value;
        }
        catch { return default(string); }
    }
    public int ReadInt() {
        try
        {
            var value = (int)data[0];
            data.RemoveAt(0);
            return value;
        }
        catch { return default(int); }
    }
    public float ReadFloat()
    {
        try
        {
            var value = (float)data[0];
            data.RemoveAt(0);
            return value;
        }
        catch { return default(float); }
    }
    public bool ReadBool()
    {
        try {
            var value = (bool)data[0];
            data.RemoveAt(0);
            return value;
        }
        catch { return default(bool); }
    }
    public object ReadObject()
    {
        try
        {
            var value = data[0];
            data.RemoveAt(0);
            return value;
        }
        catch { return default(object); }
    }

    public byte[] Serialize()
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Binder = TypeOnlyBinder.Default;
        bf.Serialize(ms, this);
        return ms.ToArray();
    }
    public Packet Deserialize(byte[] buffer)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        bf.Binder = TypeOnlyBinder.Default;
        Packet p = (Packet)bf.Deserialize(ms);
        id = p.id;
        sender = p.sender;
        data = p.data;
        return this;
    }

    public void Dispose()
    {
        id = null;
        sender = null;
        data = null;
    }
}

/// <summary>
/// removes assembly name from type resolution
/// </summary>
public class TypeOnlyBinder : SerializationBinder
{
    private static SerializationBinder defaultBinder = new BinaryFormatter().Binder;

    public override Type BindToType(string assemblyName, string typeName)
    {
        if (assemblyName.Equals("NA"))
            return Type.GetType(typeName);
        else
            return defaultBinder.BindToType(assemblyName, typeName);

    }
    public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        // but null out the assembly name
        assemblyName = "NA";
        typeName = serializedType.FullName;

    }

    private static object locker = new object();
    private static TypeOnlyBinder _default = null;

    public static TypeOnlyBinder Default
    {
        get
        {
            lock (locker)
            {
                if (_default == null)
                    _default = new TypeOnlyBinder();
            }
            return _default;
        }
    }
}