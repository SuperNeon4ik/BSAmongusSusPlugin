using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BSAmongusSusPlugin
{
    public class RandomVineboomHandler : MonoBehaviour
    {
        public static RandomVineboomHandler Instance { get; private set; }

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
        }

        private void Start()
        {
            StartCoroutine(LoadSound());
        }

        private IEnumerator LoadSound()
        {
            var folderPath = Environment.CurrentDirectory + "\\UserData\\AmongusSusPlugin";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            AudioClip? clip = null;
            if (File.Exists(Path.Combine(folderPath, "vineboom.ogg")))
            {
                FileInfo fileInfo = new FileInfo(Path.Combine(folderPath, "vineboom.ogg"));
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

            StartCoroutine(DoRandomDelayVineboomSound(2, 180, 1, clip));
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

        private IEnumerator DoRandomDelayVineboomSound(int minSeconds, int maxSeconds, float randPosOffset, AudioClip? clip)
        {
            var go = new GameObject("Hehehehaw");
            var source = go.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            source.clip = clip;
            DontDestroyOnLoad(go);

            while (true)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(minSeconds, maxSeconds));
                if (clip != null)
                {
                    go.transform.position = new Vector3(
                        UnityEngine.Random.Range(randPosOffset * -1, randPosOffset),
                        UnityEngine.Random.Range(randPosOffset * -1, randPosOffset),
                        UnityEngine.Random.Range(randPosOffset * -1, randPosOffset)
                    );
                    source.Play();
                }
                StartCoroutine(AmongusJumpscareHandler.Instance.Jumpscare());
            }
        }
    }
}
