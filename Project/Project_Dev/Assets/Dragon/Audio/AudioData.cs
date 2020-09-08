using System;
using UnityEngine;
using DG.Tweening;

namespace Uqee.Audio
{
    public class AudioData:MonoBehaviour
    {
        public AudioSource audioSource;
        public string audioName;
        public uint timeoutId;
        public Action playEndHandler;
        public bool isEffect;
        public bool isDestroyed { get; private set; }
        public float releaseTime;
        public virtual void ChangeVolume(float vol)
        {
            if (audioSource == null) return;
            audioSource.volume = vol;
        }

        protected virtual void _OnPlayEnd()
        {
            playEndHandler?.Invoke();
            AudioPlayer.StopEffect(audioSource);
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            Release();
            audioSource = null;
        }

        public virtual void Release()
        {
            if (timeoutId > 0)
            {
                JobScheduler.I.ClearTimer(timeoutId);
                timeoutId = 0;
            }
            releaseTime = AppStatus.realtimeSinceStartup;
            isEffect = false;
            playEndHandler = null;
            if (audioSource == null)
            {
                return;
            }

            audioSource.Stop();
            audioSource.clip = null;
        }

        public bool inited { get; private set; }
        private bool _waitPlay;
        private void Awake()
        {
            inited = true;
            if (_waitPlay)
            {
                _waitPlay = false;
                Play();
            }
        }

        public void DoVolumeTween(float from ,float to,float duration, TweenCallback onComplete)
        {
            if (audioSource != null)
            {
                DOTween.Kill(audioSource);
                audioSource.volume = from;
                var tween = audioSource.DOFade(to, duration);
                if (onComplete != null) {
                    tween.OnComplete(onComplete);
                }
            }
        }

        public virtual bool Play()
        {
            if (!inited || audioSource == null)
            {
                _waitPlay = true;
                return false;
            }
            var isMute = isEffect ? AudioPlayer.isMuteEffect : AudioPlayer.isMuteMusic;
            if (audioSource.loop)
            {
                try
                {
                    audioSource.Play();
                    if (isMute)
                    {
                        audioSource.Pause();
                    }
                }catch(Exception ex)
                {

                }
            }
            else
            {
                if (!isMute)
                {
                    timeoutId = JobScheduler.I.SetTimeOut(_OnPlayEnd, audioSource.clip.length + 0.5f);
                    try
                    {
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                    catch (Exception ex)
                    {
                        JobScheduler.I.ClearTimer(timeoutId);
                        timeoutId = 0;
                        _OnPlayEnd();
                    }
                }
            }
            return true;
        }
    }
}