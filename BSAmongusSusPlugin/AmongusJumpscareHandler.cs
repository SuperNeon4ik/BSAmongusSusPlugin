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
    public class AmongusJumpscareHandler : MonoBehaviour
    {
        public static AmongusJumpscareHandler Instance { get; internal set; }

        public SpriteRenderer jumpscare;

        private void Awake()
        {
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
            jumpscare = new GameObject("Amongus Hehehehaw").AddComponent<SpriteRenderer>();
            jumpscare.transform.localScale = Vector3.one;
            jumpscare.transform.position = new Vector3(1.5f, 3f, 4.25f);
            jumpscare.gameObject.SetActive(false);
            DontDestroyOnLoad(jumpscare);
            StartCoroutine(LoadTexture());
        }

        private IEnumerator LoadTexture()
        {
            var folderPath = Environment.CurrentDirectory + "\\UserData\\AmongusSusPlugin";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            Texture2D? tex = null;
            if (File.Exists(Path.Combine(folderPath, "amongus.png")))
            {
                FileInfo fileInfo = new FileInfo(Path.Combine(folderPath, "amongus.png"));
                var web = GetRequest(fileInfo.FullName);
                var operation = web.SendWebRequest();

                while (!operation.isDone)
                {
                    yield return null;
                }

                if (web.isNetworkError || web.isHttpError)
                {
                    Plugin.Log.Error($"Failed to load file {name} with error {web.error}");
                    tex = null;
                }
                else
                {
                    tex = DownloadHandlerTexture.GetContent(web);
                }
            }

            yield return null;

            if (tex != null)
                jumpscare.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(1, 1), 100);

            Plugin.Log?.Debug("Camera pos: " + Camera.current.transform.position);
            yield return new WaitForSecondsRealtime(5);
            Plugin.Log?.Debug("Camera pos: " + Camera.current.transform.position);
        }

        private static UnityWebRequest GetRequest(string fullPath)
        {
            var fileUrl = "file:///" + fullPath;
            return UnityWebRequestTexture.GetTexture(fileUrl);
        }


        public IEnumerator Jumpscare()
        {
            jumpscare.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            jumpscare.gameObject.SetActive(false);
        }
    }
}
