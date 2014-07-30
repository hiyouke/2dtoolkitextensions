using UnityEngine;
using System;
using System.Collections.Generic;

public class TextureCache : MonoBehaviour 
{
	public const int MAX_CACHE_SIZE = 2048 * 2048 * 4;//4M

	public bool hasNew = false;

	List<TextureRef> mTextures = new List<TextureRef> ();

	int mCachedSize = 0;

	public override string ToString()
	{
		return string.Format("<TextureCache | Number of textures = {0}>", mTextures.Count);
	}

	public void AddImageBatch (string[] filepaths, int width, int height)
	{
		for(int i=0;i<filepaths.Length;i++)
		{
			AddImage(filepaths[i],width,height);
		}
	}
	
	public TextureRef AddImage(string filepath,int width,int height)
	{
		TextureRef texture = GetTexture(filepath);
		if(null != texture)
		{
			texture.Retain();
		}
		else if(null == texture)
		{
			texture = new TextureRef();
			texture.filepath = filepath;
			texture.width = width;
			texture.height = height;
			texture.Retain();
			mTextures.Add(texture);
			mTextures.Sort(new TextureSorter());
			
			mCachedSize += width * height * 4;//内存大小
			if(mCachedSize > MAX_CACHE_SIZE)
			{
				RemoveUnusedTextures();
			}

			hasNew = true;
		}
		return texture;
	}

	public int GetTextureCount()
	{
		return mTextures.Count;
	}

	public TextureRef[] GetAllTextures()
	{
		return  mTextures.ToArray();
	}

	public TextureRef GetTexture2DAtIndex(int index)
	{
		if(index >= 0 && index < mTextures.Count)
		{
			return mTextures[index];
		}
		return null;
	}

	public TextureRef GetTexture(string filepath)
	{
		System.Predicate<TextureRef> match = delegate(TextureRef obj) {
				return obj.filepath == filepath;
				};
		return mTextures.Find (match);
	}
	
	public void RemoveTextureForKey(string key)
	{
		TextureRef texture = GetTexture(key);
		if(null != texture)
		{
			texture.ClearAll();
			mTextures.Remove(texture);
		}
	}

	public void RemoveAllTextures()
	{
		foreach(TextureRef tex in mTextures)
		{
			tex.ClearAll();
		}
		mTextures.Clear ();
	}

	public void RemoveUnusedTextures()
	{
		while(mTextures.Count > 0 && mCachedSize > MAX_CACHE_SIZE)
		{
			TextureRef tex = mTextures[mTextures.Count -1];
			RemoveTextureForKey(tex.filepath);
			mCachedSize -= tex.width * tex.height * 4;//内存大小
		}
	}
	
	public void Test()
	{
		string[] testpaths = new string[20];
		testpaths [0] = "UI/Texture/Icon/head/card/head_changer";
		testpaths [1] = "UI/Texture/Icon/head/card/head_chiyou";
		testpaths [2] = "UI/Texture/Icon/head/card/head_fengbo";
		testpaths [3] = "UI/Texture/Icon/head/card/head_houyi";
		testpaths [4] = "UI/Texture/Icon/head/card/head_huaxian";
		testpaths [5] = "UI/Texture/Icon/head/card/head_huobutianjun";
		testpaths [6] = "UI/Texture/Icon/head/card/head_huoling";
		testpaths [7] = "UI/Texture/Icon/head/card/head_jianxian";
		testpaths [8] = "UI/Texture/Icon/head/card/head_jinjiashen";
		testpaths [9] = "UI/Texture/Icon/head/card/head_jiutianxuannv";
		for(int i=0;i<10;i++)
		{
			AddImage(testpaths[i],90,90);
		}

		testpaths [10] = "UI/Texture/Icon/avatar/avatar_baigujing";
		testpaths [11] = "UI/Texture/Icon/avatar/avatar_baihuaxianzi";
		testpaths [12] = "UI/Texture/Icon/avatar/avatar_ceshika";
		testpaths [13] = "UI/Texture/Icon/avatar/avatar_changer";
		testpaths [14] = "UI/Texture/Icon/avatar/avatar_chiyou";
		testpaths [15] = "UI/Texture/Icon/avatar/avatar_default";
		testpaths [16] = "UI/Texture/Icon/avatar/avatar_fengbo";
		testpaths [17] = "UI/Texture/Icon/avatar/avatar_houyi";
		testpaths [18] = "UI/Texture/Icon/avatar/avatar_jianxian";
		testpaths [19] = "UI/Texture/Icon/avatar/avatar_jingshika";
		for(int i=10;i<20;i++)
		{
			AddImage(testpaths[i],200,270);
		}
	}
}
