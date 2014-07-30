using UnityEngine;
using System.Collections;

public delegate void IconSpriteManagerProgress(IconSpriteManager manager,float progress);

public delegate void IconSpriteManagerLoaded(IconSpriteManager manager);

public class IconSpriteManager : MonoBehaviour
{
	public static IconSpriteManager msInst = null;

	public IconSpriteManagerLoaded loadedDelegate = null;

	public IconSpriteManagerProgress progressDelegate = null;

	tk2dSpriteCollectionData mSpriteCollectionData = null;

	Texture2D mAtlasTexture = new Texture2D(2048, 2048,TextureFormat.RGB24, false);

	TextureCache mTextureCache = null;

	Texture2D[] mTextures = null;

	void Awake()
	{
		msInst = this;

		mTextureCache = this.gameObject.AddComponent<TextureCache> ();
	}

	public tk2dSpriteCollectionData SpriteCollectionData 
	{
		get { return mSpriteCollectionData; }
	}

	public TextureCache Cache
	{
		get { return mTextureCache; }
	}

	public bool IsSpriteCached(string name)
	{
		int spriteId = mSpriteCollectionData.GetSpriteIdByName (name, -1);
		if(-1 == spriteId)
		{
			return false;
		}
		return true;
	}
		
	public void BeginLoading()
	{
		if(mTextureCache.hasNew)
		{
			RebuildAtlas ();
		}
		else
		{
			EndLoading();
		}
	}

	void EndLoading()
	{
		if(null != loadedDelegate)
		{
			loadedDelegate (this);
		}
		mTextureCache.hasNew = false;
	}

	void ClearAll()
	{
		mTextures = null;
	}
	
	public void AddImageBatch2Cache (string[] filepaths, int width, int height)
	{
		mTextureCache.AddImageBatch (filepaths, width, height);
	}
	
	public TextureRef AddImage2Cahce(string filepath,int width,int height)
	{
		TextureRef texture = mTextureCache.AddImage (filepath, width, height);
		return texture;
	}

	public void RebuildAtlasSync()
	{
		ClearAll ();
		
		int texCnt = mTextureCache.GetTextureCount ();
		if(texCnt > 0)
		{
			mTextures = new Texture2D[mTextureCache.GetTextureCount()];

			TextureRef[] texs = mTextureCache.GetAllTextures();
			for (int i = 0; i < texs.Length; i++) 
			{
				if(texs[i].IsPacked())
				{
					mTextures[i] = mAtlasTexture.GetTexture(texs[i].width,texs[i].height,texs[i].uvRect);
				}
				else
				{
					mTextures[i] = Resources.Load(texs[i].filepath) as Texture2D;
				}
			}

			PackTexture ();

			mTextureCache.hasNew = false;
		}
	}

	void RebuildAtlas()
	{
		ClearAll ();

		int texCnt = mTextureCache.GetTextureCount ();
		if(texCnt > 0)
		{
			mTextures = new Texture2D[mTextureCache.GetTextureCount()];
			StartCoroutine("GenerateTextures");
		}
	}

	IEnumerator GenerateTextures() 
	{
		TextureRef[] texs = mTextureCache.GetAllTextures();
		for (int i = 0; i < texs.Length; i++) 
		{
			if(texs[i].IsPacked())
			{
				mTextures[i] = mAtlasTexture.GetTexture(texs[i].width,texs[i].height,texs[i].uvRect);
			}
			else
			{
				mTextures[i] = Resources.Load(texs[i].filepath) as Texture2D;
			}
			if(null != progressDelegate)
			{
				progressDelegate(this,(1.0f *i)/texs.Length);
			}
			yield return 0;
		}
		PackTexture ();
		EndLoading ();
	}

	void PackTexture()
	{
		TextureRef[] texs = mTextureCache.GetAllTextures();
		string[] texNames = new string[texs.Length];	
		for(int i = 0;i<mTextures.Length;i++)
		{
			texNames[i] = texs[i].GetFileName();
		}
		Rect[] uvRects = mAtlasTexture.PackTextures(mTextures, 2, 2048);
		ClearAll ();
		Rect[] regions = new Rect[texs.Length];
		Vector2[] anchors = new Vector2[texs.Length];
		for(int i=0;i<texs.Length;i++)
		{
			regions[i].x = uvRects[i].x * mAtlasTexture.width;
			regions[i].width = uvRects[i].width * mAtlasTexture.width;
			regions[i].y = mAtlasTexture.height - (uvRects[i].height + uvRects[i].y) * mAtlasTexture.height;
			regions[i].height = uvRects[i].height * mAtlasTexture.height;
			
			texs[i].uvRect = uvRects[i];
			
			anchors[i] = tk2dSpriteGeomGen.GetAnchorOffset( tk2dBaseSprite.Anchor.MiddleCenter, regions[i].width, regions[i].height );
		}
		
		if(null != mSpriteCollectionData)
		{
			GameObject.Destroy(mSpriteCollectionData.gameObject);
			mSpriteCollectionData = null;
		}
		float ppm = tk2dCamera.Instance.CameraSettings.orthographicPixelsPerMeter;
		tk2dSpriteCollectionSize size = tk2dSpriteCollectionSize.PixelsPerMeter(ppm);
		Vector2 textureDimensions = new Vector2( mAtlasTexture.width, mAtlasTexture.height );
		mSpriteCollectionData = tk2dRuntime.SpriteCollectionGenerator.CreateFromTexture(mAtlasTexture,size,textureDimensions,texNames,regions,null,anchors,null);
		mSpriteCollectionData.spriteCollectionName = "IconSpriteCollection";
		mSpriteCollectionData.gameObject.name = "IconSpriteCollection_Dynamic";
	}
}
