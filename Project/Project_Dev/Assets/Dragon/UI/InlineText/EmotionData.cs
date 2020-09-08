using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.U2D;

public class EmotionData : ScriptableObject
{
    [Serializable]
    public class Config
    {
        public string tag;
        public string spriteName;
        public string atlas;
        public Rect[] rects;
        public int frameRate { get; private set; } = 3;
        public bool isStatic { get { return string.IsNullOrEmpty(atlas); } }
    }

    //public SpriteAtlas atlas;
    public List<Config> emotionList;
}