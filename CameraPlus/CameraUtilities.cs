using System;
using System.IO;
using System.Linq;

namespace CameraPlus
{
    public class CameraUtilities
    {
        public static bool CameraExists(string cameraName)
        {
            return Plugin.Instance.Cameras.Keys.Where(c => c == cameraName + ".cfg").Count() > 0;
        }
        
        public static void AddNewCamera(string cameraName, Config CopyConfig = null)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "UserData\\CameraPlus\\" + cameraName + ".cfg");
            if (!File.Exists(path))
            {
                // Try to copy their old config file into the new camera location
                if(cameraName == "cameraplus")
                {
                    string oldPath = Path.Combine(Environment.CurrentDirectory, "cameraplus.cfg");
                    if (File.Exists(oldPath))
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(path)))
                            Directory.CreateDirectory(Path.GetDirectoryName(path));

                        File.Move(oldPath, path);
                        Plugin.Log("Copied old cameraplus.cfg into new CameraPlus folder in UserData");
                    }
                }

                Config config = null;
                if (CopyConfig != null)
                    File.Copy(CopyConfig.FilePath, path, true);

                config = new Config(path);
                foreach (CameraPlusInstance c in Plugin.Instance.Cameras.Values.OrderBy(i => i.Config.layer))
                {
                    if (c.Config.layer > config.layer)
                        config.layer += (c.Config.layer - config.layer);
                    else if (c.Config.layer == config.layer)
                        config.layer++;
                }
                if (CopyConfig == null && cameraName != "cameraplus")
                {
                    config.screenHeight /= 4;
                    config.screenWidth /= 4;
                }
                
                config.Position = config.DefaultPosition;
                config.Rotation = config.DefaultRotation;
                config.FirstPersonPositionOffset = config.DefaultFirstPersonPositionOffset;
                config.Save();
                Plugin.Log($"Success creating new camera \"{cameraName}\"");
            }
            else
            {
                Plugin.Log($"Camera \"{cameraName}\" already exists!");
            }
        }

        public static string GetNextCameraName()
        {
            int index = 1;
            string cameraName = String.Empty;
            while (true)
            {
                cameraName = $"customcamera{index.ToString()}";
                if (!CameraUtilities.CameraExists(cameraName))
                    break;

                index++;
            }
            return cameraName;
        }

        public static bool RemoveCamera(CameraPlusBehaviour instance, bool delete = true)
        {
            try
            {
                if (Plugin.Instance.Cameras.TryRemove(Plugin.Instance.Cameras.Where(c => c.Value.Instance == instance && c.Key != "cameraplus.cfg")?.First().Key, out var removedEntry))
                {
                    if (delete)
                    {
                        if (File.Exists(removedEntry.Config.FilePath))
                            File.Delete(removedEntry.Config.FilePath);
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                Plugin.Log("Can't remove cam!");
            }
            return false;
        }

        public static void ReloadCameras()
        {
            try
            {
                string[] files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "UserData\\CameraPlus"));
                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileName(filePath);
                    if (fileName.EndsWith(".cfg") && !Plugin.Instance.Cameras.ContainsKey(fileName))
                    {
                        Plugin.Log($"Found config {filePath}!");
                        Plugin.Instance.Cameras.TryAdd(fileName, new CameraPlusInstance(filePath));
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log($"Exception while reloading cameras! {e.ToString()}");
            }
        }
        
        //public static IEnumerator Spawn38Cameras()
        //{
        //    lock (Plugin.Instance.Cameras)
        //    {
        //        for (int i = 0; i < 38; i++)
        //        {
        //            AddNewCamera(GetNextCameraName(), null, true);
        //            ReloadCameras();

        //            yield return null;
        //        }
        //    }
        //}
    }
}
