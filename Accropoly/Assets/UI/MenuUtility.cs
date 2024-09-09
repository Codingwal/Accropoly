using System;

public static class MenuUtility
{
    public static Action<float> onLoadingWorld; // TODO: call every frame while world is loading
    public static void CreateWorld(string worldName, string templateName)
    {
        SaveSystem.Instance.UpdateWorldName(worldName);
        SaveSystem.Instance.CreateWorld(worldName, templateName);
    }
    public static void LoadWorld(string worldName)
    {
        SaveSystem.Instance.UpdateWorldName(worldName);

        // TODO: Actually load world
    }
    public static void DeleteWorld(string mapName)
    {
        FileHandler.DeleteFile("Saves", mapName);
    }
    public static string[] GetMapTemplateNames()
    {
        return FileHandler.ListFiles("Templates");
    }
    public static string[] GetWorldNames()
    {
        return FileHandler.ListFiles("Saves");
    }
}
