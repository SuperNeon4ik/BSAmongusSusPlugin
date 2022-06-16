using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BSAmongusSusPlugin
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class BSAmongusSusPluginController : MonoBehaviour
    {
        public static BSAmongusSusPluginController Instance { get; private set; }

        // These methods are automatically called by Unity, you should remove any you aren't using.
        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
            //   and destroy any that are created while one already exists.
            if (Instance != null)
            {
                Plugin.Log?.Warn($"Instance of {GetType().Name} already exists, destroying.");
                GameObject.DestroyImmediate(this);
                return;
            }
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            Instance = this;
            Plugin.Log?.Debug($"{name}: Awake()");


            // Create Random Vineboom Handler
            new GameObject("Random Vineboom Handler").AddComponent<RandomVineboomHandler>();
        }

        private void Start()
        {
            StartCoroutine(LoadStartupSound());
        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Plugin.Log?.Debug($"{name}: OnDestroy()");
            if (Instance == this)
                Instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.

        }
        #endregion

        private IEnumerator LoadStartupSound()
        {
            var folderPath = Environment.CurrentDirectory + "\\UserData\\AmongusSusPlugin";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            AudioClip? clip = null;
            if (File.Exists(Path.Combine(folderPath, "vineboom.ogg")))
            {
                FileInfo fileInfo = new FileInfo(Path.Combine(folderPath, "startup.ogg"));
                var web = GetRequest(fileInfo.FullName);
                var operation = web.SendWebRequest();

                while (!operation.isDone)
                {
                    yield return null;
                }

                if (web.isNetworkError || web.isHttpError)
                {
                    Plugin.Log.Error($"Failed to load file {name} with error {web.error}");
                    clip = null;
                }
                else
                {
                    clip = DownloadHandlerAudioClip.GetContent(web);
                }
            }

            yield return null;

            var go = new GameObject("Hehehehaw exdee");
            var source = go.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            source.clip = clip;
            DontDestroyOnLoad(go);
            float randPosOffset = 1.5f;
            go.transform.position = new Vector3(
                UnityEngine.Random.Range(randPosOffset * -1, randPosOffset),
                UnityEngine.Random.Range(randPosOffset * -1, randPosOffset),
                UnityEngine.Random.Range(randPosOffset * -1, randPosOffset)
            );
            source.Play();

            yield return new WaitForSecondsRealtime(clip.length + 1);

            Destroy(go);
        }

        // Stolen from https://github.com/Meivyn/SoundReplacer/blob/main/SoundReplacer/SoundLoader.cs
        private static UnityWebRequest GetRequest(string fullPath)
        {
            var fileUrl = "file:///" + fullPath;
            var fileInfo = new FileInfo(fullPath);
            var extension = fileInfo.Extension;
            switch (extension)
            {
                case ".ogg":
                    return UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.OGGVORBIS);
                case ".mp3":
                    return UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.MPEG);
                case ".wav":
                    return UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.WAV);
                default:
                    return UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.UNKNOWN);
            }
        }
    }
}
