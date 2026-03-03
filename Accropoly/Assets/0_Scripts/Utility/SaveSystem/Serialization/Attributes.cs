using System;

public class SaveAttribute : Attribute { }
public class DontSaveAttribute : Attribute { }
public interface ICustomSaving
{
    public void Save(Serializer serializer);
    public void Load(Deserializer deserializer);
}