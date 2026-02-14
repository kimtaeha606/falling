using UnityEngine;
using UnityEngine.Video;
using System.Collections;

[RequireComponent(typeof(VideoPlayer))]
public class MainMenuVideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    private void Awake()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            Debug.LogError("[MainMenuVideoController] VideoPlayer is missing.", this);
            enabled = false;
            return;
        }

        if ((videoPlayer.renderMode == VideoRenderMode.CameraFarPlane || videoPlayer.renderMode == VideoRenderMode.CameraNearPlane)
            && videoPlayer.targetCamera == null)
        {
            videoPlayer.targetCamera = Camera.main;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.errorReceived += HandleVideoError;
        videoPlayer.prepareCompleted += HandlePrepared;
    }

    private void Start()
    {
        if (videoPlayer.clip == null && string.IsNullOrWhiteSpace(videoPlayer.url))
        {
            Debug.LogError("[MainMenuVideoController] No VideoClip or URL assigned.", this);
            return;
        }

        StartCoroutine(PrepareAndPlay());
    }

    private void OnDisable()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.errorReceived -= HandleVideoError;
        videoPlayer.prepareCompleted -= HandlePrepared;
    }

    private void HandlePrepared(VideoPlayer source)
    {
        source.Play();
    }

    private void HandleVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"[MainMenuVideoController] Video error: {message}", this);
    }

    private IEnumerator PrepareAndPlay()
    {
        if (!videoPlayer.isPrepared)
        {
            videoPlayer.Prepare();

            const float prepareTimeout = 2f;
            float elapsed = 0f;
            while (!videoPlayer.isPrepared && elapsed < prepareTimeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
    }
}
