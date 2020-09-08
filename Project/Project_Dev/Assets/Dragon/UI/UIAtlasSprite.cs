using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.U2D;
using UnityEngine.UI;
using Uqee.Pool;

[Serializable]
public class UIAtlasSprite
 {
    public SpriteAtlas Atlas;
    public String AtlasName;
    public string[] SpriteNames;
    private Sprite[] spriteList;

    public void CreateFrom(Transform tfm)
    {
        var imgList = DataListFactory<Image>.Get();
        tfm.GetComponentsInChildren<Image>(true, imgList);
        if (spriteList == null)
        {
            spriteList = new Sprite[SpriteNames.Length];
        }else if (spriteList.Length != SpriteNames.Length)
        {
            Array.Resize(ref spriteList, SpriteNames.Length);
        }

        for (int i = 0; i < imgList.Count; i++)
        {
            if (imgList[i].sprite!=null)
            {
                var idx = Array.IndexOf(SpriteNames, imgList[i].sprite.name);
                if (idx != -1)
                {
                    spriteList[idx] = imgList[i].sprite;
                }
            }
        }
        DataListFactory<Image>.Release(imgList);
    }

    /// <summary>
    /// 获取第一个
    /// </summary>
    /// <param name="assetData"></param>
    /// <returns></returns>
    public Sprite GetSprite()
    {
        return GetSprite(0);
    }

    public Sprite GetSprite(int index)
    {
        if (SpriteNames == null || SpriteNames.Length <= index)
            return null;
        if(spriteList!=null&&spriteList.Length>0)
            return spriteList[index];

        if(Atlas!=null){
            if(spriteList==null|| spriteList.Length!= SpriteNames.Length)
            {
                spriteList = new Sprite[SpriteNames.Length];
            }
            for(int i =0;i< SpriteNames.Length; i++)
            {
                spriteList[i] = Atlas.GetSprite(SpriteNames[i]);
            }
            return spriteList[index];
        }
        return null;
    }

    public Sprite GetSprite(string spriteName)
    {
        var tIdx = Array.IndexOf(SpriteNames, spriteName);
        if (tIdx==-1)
            return null;
        return GetSprite(tIdx);
    }
}