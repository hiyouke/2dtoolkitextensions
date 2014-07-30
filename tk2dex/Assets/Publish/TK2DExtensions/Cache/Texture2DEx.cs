using UnityEngine;
using System.Collections;

public static class Texture2DEx 
{
	public static Texture2D GetTexture(this Texture2D texture, int w,int h,Rect uvRect)
	{
		Texture2D destTex = new Texture2D(w, h);
		Color[] destPix = new Color[destTex.width * destTex.height];
		int x = (int)(uvRect.x * texture.width);
		int y = (int)(uvRect.y * texture.height);
		int width = (int)(uvRect.width * texture.width);
		int height = (int)(uvRect.height * texture.height);
		destPix = texture.GetPixels (x, y, width, height);
		destTex.SetPixels(destPix);
		destTex.Apply();
		return destTex;
	}
}
