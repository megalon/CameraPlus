using System;
using System.IO;
using UnityEngine;

namespace CameraPlus
{
    public class Config
    {
        public string FilePath { get; }
        public float fov = 90;
        public int antiAliasing = 2;
        public float renderScale = 1;
        public float positionSmooth = 10;
        public float rotationSmooth = 5;

        public bool thirdPerson = false;
        public bool showThirdPersonCamera = true;

        public float posx;
        public float posy = 2;
        public float posz = -1.2f;

        public float angx = 15;
        public float angy;
        public float angz;

        public float firstPersonPosOffsetX;
        public float firstPersonPosOffsetY;
        public float firstPersonPosOffsetZ;

        public int screenWidth = Screen.width;
        public int screenHeight = Screen.height;
        public int screenPosX;
        public int screenPosY;

        public int layer = -1000;

        public bool fitToCanvas = false;

        public string movementScriptPath = String.Empty;
        //public int maxFps = 90;

        public event Action<Config> ConfigChangedEvent;

        private readonly FileSystemWatcher _configWatcher;
        private bool _saving;

        public Vector2 ScreenPosition
        {
            get
            {
                return new Vector2(screenPosX, screenPosY);
            }
        }

        public Vector2 ScreenSize
        {
            get
            {
                return new Vector2(screenWidth, screenHeight);
            }
        }

        public Vector3 Position
        {
            get
            {
                return new Vector3(posx, posy, posz);
            }
            set
            {
                posx = value.x;
                posy = value.y;
                posz = value.z;
            }
        }

        public Vector3 DefaultPosition
        {
            get
            {
                return new Vector3(0f, 2f, -1.2f);
            }
        }

        public Vector3 Rotation
        {
            get
            {
                return new Vector3(angx, angy, angz);
            }
            set
            {
                angx = value.x;
                angy = value.y;
                angz = value.z;
            }
        }

        public Vector3 DefaultRotation
        {
            get
            {
                return new Vector3(15f, 0f, 0f);
            }
        }

        public Vector3 FirstPersonPositionOffset
        {
            get
            {
                return new Vector3(firstPersonPosOffsetX, firstPersonPosOffsetY, firstPersonPosOffsetZ);
            }
            set
            {
                firstPersonPosOffsetX = value.x;
                firstPersonPosOffsetY = value.y;
                firstPersonPosOffsetZ = value.z;
            }
        }

        public Vector3 DefaultFirstPersonPositionOffset
        {
            get
            {
                return new Vector3(0, 0, 0);
            }
        }

        public Config(string filePath)
        {
            FilePath = filePath;

            if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

            if (File.Exists(FilePath))
            {
                Load();
                var text = File.ReadAllText(FilePath);
                if (!text.Contains("fitToCanvas") && Path.GetFileName(FilePath) == "cameraplus.cfg")
                {
                    fitToCanvas = true;
                    Save();
                }
                if (text.Contains("rotx"))
                {
                    var oldRotConfig = new OldRotConfig();
                    ConfigSerializer.LoadConfig(oldRotConfig, FilePath);

                    var euler = new Quaternion(oldRotConfig.rotx, oldRotConfig.roty, oldRotConfig.rotz, oldRotConfig.rotw).eulerAngles;
                    angx = euler.x;
                    angy = euler.y;
                    angz = euler.z;

                    Save();
                }
            }
            else
            {
                Save();
            }

            _configWatcher = new FileSystemWatcher(Path.GetDirectoryName(FilePath))
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = Path.GetFileName(FilePath),
                EnableRaisingEvents = true
            };
            _configWatcher.Changed += ConfigWatcherOnChanged;
        }

        ~Config()
        {
            _configWatcher.Changed -= ConfigWatcherOnChanged;
        }

        public void Save()
        {
            _saving = true;
            ConfigSerializer.SaveConfig(this, FilePath);
        }

        public void Load()
        {
            ConfigSerializer.LoadConfig(this, FilePath);
        }

        private void ConfigWatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            if (_saving)
            {
                _saving = false;
                return;
            }

            Load();

            if (ConfigChangedEvent != null)
            {
                ConfigChangedEvent(this);
            }
        }

        public class OldRotConfig
        {
            public float rotx;
            public float roty;
            public float rotz;
            public float rotw;
        }
    }
}