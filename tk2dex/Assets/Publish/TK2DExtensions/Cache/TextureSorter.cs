using UnityEngine;
using System.Collections.Generic;

public class TextureSorter : IComparer<TextureRef>
{
	public int Compare (TextureRef tex1, TextureRef tex2)
	{
		if(tex1.ReferenceCount > tex2.ReferenceCount)
		{
			return -1;
		}
		else if(tex1.ReferenceCount < tex2.ReferenceCount)
		{
			return 1;
		}
		return 0;
	}
}
