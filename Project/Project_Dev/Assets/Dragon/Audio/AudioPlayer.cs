using System.Collections.Generic;
using System;
using UnityEngine;

namespace Uqee.Audio
{
    public static class AudioPlayer
    {
        private static List<AudioData> _musicList = new List<AudioData>();
        private static List<AudioData> _effectList = new List<AudioData>();
        public static bool isMuteMusic { get; private set; }
        public static bool isMuteEffect { get; private set; }

        #region 背景音乐

        public static string currMusicName
        {
            get
            {
                if (_musicList.Count > 0)
                {
                    return _musicList[0].audioName;
                }
                return string.Empty;
            }
        }

        public static bool IsMusicPlaying(string audioName)
        {
            if (string.IsNullOrEmpty(audioName))
            {
                return false;
            }

            bool find = false;
            for (int i = 0; i < _musicList.Count; i++)
            {
                if (_musicList[i].audioName == audioName)
                {
                    find = _musicList[i].audioSource != null;
                    if (_musicList[i].audioSource == null)
                    {
                        _musicList[i].Release();
                        _musicList.RemoveAt(i);
                    }
                }
            }
            return find;
        }

        public static void PlayMusic(AudioData data)
        {
            if (data == null || data.audioSource == null)
            {
                return;
            }
            if (IsMusicPlaying(data.audioName))
            {
                return;
            }
            data.Play();
            _musicList.Add(data);
        }

        public static void MuteMusic(bool val)
        {
            isMuteMusic = val;
            for (int i = _musicList.Count - 1; i >= 0; i--)
            {
                try
                {
                    if (val)
                    {
                        _musicList[i].audioSource.Pause();
                    }
                    else
                    {
                        _musicList[i].audioSource.UnPause();
                    }
                }
                catch (Exception)
                {
                    _musicList[i].Release();
                    _musicList.RemoveAt(i);
                }
            }
        }

        public static void MuteEffect(bool val)
        {
            isMuteEffect = val;
        }

        public static void StopAllMusic()
        {
            for (int i = _musicList.Count - 1; i >= 0; --i)
            {
                _musicList[i].Release();
            }
            _musicList.Clear();
        }

        /// <summary>
        /// 停止播放音乐
        /// </summary>
        /// <param name="audioName"></param>
        public static void StopMusic(string audioName)
        {
            if (string.IsNullOrEmpty(audioName))
            {
                return;
            }
            for (var i = 0; i < _musicList.Count; ++i)
            {
                if (_musicList[i].audioName == audioName)
                {
                    _musicList[i].Release();
                    _musicList.RemoveAt(i);
                    break;
                }
            }
        }

        public static void ChangeMusicVol(float vol)
        {
            for (int i = 0; i < _musicList.Count; i++)
            {
                _musicList[i].ChangeVolume(vol);
            }
        }
        public static void PauseCurrentAllMusic(bool pause = true)
        {
            for (int i = _musicList.Count - 1; i >= 0; i--)
            {
                try
                {
                    if (pause)
                    {
                        _musicList[i].audioSource.Pause();
                    }
                    else
                    {
                        _musicList[i].audioSource.UnPause();
                    }
                }
                catch (Exception)
                {
                    _musicList[i].Release();
                    _musicList.RemoveAt(i);
                }
            }
        }
        #endregion 背景音乐

        #region 音效

        public static void PlayEffect(AudioData data)
        {
            if (data == null || data.audioSource == null) return;
            data.Play();
            _effectList.Add(data);
        }

        public static void StopEffect(string audioName)
        {
            if (string.IsNullOrEmpty(audioName))
            {
                return;
            }
            var len = _effectList.Count;
            for (int i = 0; i < len; i++)
            {
                if (_effectList[i].audioName == audioName)
                {
                    _effectList[i].Release();
                    _effectList.RemoveAt(i);
                    break;
                }
            }
        }

        public static void StopEffect(AudioSource source)
        {
            if (source == null)
            {
                return;
            }
            var len = _effectList.Count;
            for (int i = 0; i < len; i++)
            {
                if (_effectList[i].audioSource == source)
                {
                    _effectList[i].Release();
                    _effectList.RemoveAt(i);
                    break;
                }
            }
        }

        public static void ChangeEffectVol(float vol)
        {
            var len = _effectList.Count;
            for (int i = 0; i < len; i++)
            {
                _effectList[i].ChangeVolume(vol);
            }
        }

        #endregion 音效

        public static AudioData GetAudioData(string audioName)
        {
            foreach (var item in _musicList)
            {
                if (item.audioName == audioName)
                    return item;
            }

            foreach (var item in _effectList)
            {
                if (item.audioName == audioName)
                    return item;
            }
            return null;
        }
    }
}