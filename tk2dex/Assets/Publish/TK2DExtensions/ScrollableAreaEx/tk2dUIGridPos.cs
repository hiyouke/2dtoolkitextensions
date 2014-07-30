using System;

public struct tk2dUIGridPos
{
	public int row;
	public int col;
	
	public tk2dUIGridPos(int r,int c)
	{
		row = r;
		col = c;
	}

	public override string ToString()
	{
		return "row : " + row + " col : " + col;
	}
}