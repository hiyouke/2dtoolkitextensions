using UnityEngine;
using System.Collections;

public class TextureRef
{
	public string filepath = null;

	public Rect uvRect = new Rect(0,0,0,0);

	public int width;

	public int height;

	int mReferenceCount = 0;

	public string GetFileName()
	{
		int startIndex = filepath.LastIndexOf ('/');
		return filepath.Substring (startIndex + 1);
	}

	public void ClearAll()
	{
		filepath = null;

		width = height = 0;

		mReferenceCount = 0;
	}

	public bool IsPacked()
	{
		return uvRect.width != 0;
	}

	public int ReferenceCount
	{
		get { return mReferenceCount; }
	}

	public int Retain()
	{
		mReferenceCount++;
		return mReferenceCount;
	}
}
