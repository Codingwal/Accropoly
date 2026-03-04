using System;

[AttributeUsage(AttributeTargets.Struct)]
public class SaveAttribute : Attribute
{
    private string name = "";
    public SaveAttribute(string name)
    {
        this.name = name;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class DontSaveAttribute : Attribute { }

public interface ICustomSaving
{
    public void Save(Serializer serializer);
    public void Load(Deserializer deserializer);
}