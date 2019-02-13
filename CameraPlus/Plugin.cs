using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CameraPlus
{
    public class Plugin : IPlugin
    {
        public ConcurrentDictionary<string, CameraPlusInstance> Cameras = new ConcurrentDictionary<string, CameraPlusInstance>();
        
        private bool _init;
        public static Plugin Instance { get; private set; }
        public string Name => "CameraPlus-mega-edition";
        public string Version => "v3.0.7";
        
        public void OnApplicationStart()
        {
            if (_init) return;
            _init = true;
            Instance = this;

            // Add our default cameraplus camera
            CameraUtilities.AddNewCamera("cameraplus");

            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        public void SceneManager_activeSceneChanged(Scene from, Scene to)
        {
            // If any new cameras have been added to the config folder, render them
            CameraUtilities.ReloadCameras();

            try
            {
                // Trigger our activeSceneChanged event for each camera, because subscribing to the events from within the CameraPlusBehaviour component yields inconsistent results.
                foreach (CameraPlusInstance c in Cameras.Values)
                {
                    c.Instance.SceneManager_activeSceneChanged(from, to);
                }
            }
            catch (Exception) { }
        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
            // Fix the cursor when the user resizes the main camera to be smaller than the canvas size and they hover over the black portion of the canvas
            if (CameraPlusBehaviour.currentCursor != CameraPlusBehaviour.CursorType.None && !CameraPlusBehaviour.anyInstanceBusy && 
                CameraPlusBehaviour.wasWithinBorder && CameraPlusBehaviour.GetTopmostInstanceAtCursorPos() == null)
            {
                CameraPlusBehaviour.SetCursor(CameraPlusBehaviour.CursorType.None);
                CameraPlusBehaviour.wasWithinBorder = false;
            }
        }

        public static void Log(string msg)
        {
            Console.WriteLine($"[{Plugin.Instance.Name}] {msg}");
        }
    }
}