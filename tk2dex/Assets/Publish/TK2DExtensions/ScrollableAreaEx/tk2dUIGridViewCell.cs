using UnityEngine;
using System.Collections.Generic;

public class tk2dUIGridViewCell : MonoBehaviour 
{
	const int INVALID_GRID_ITEM = -1;

	public tk2dUIGridPos position = new tk2dUIGridPos(INVALID_GRID_ITEM,INVALID_GRID_ITEM);
	
	public int Row { get { return position.row; } }

	public int Col { get { return position.col; } }
			
	public void SetPosition(int row,int col)
	{
		position.row = row;
		position.col = col;
	}

	public void ResetRowAndCol()
	{
		position.row = INVALID_GRID_ITEM;
		position.col = INVALID_GRID_ITEM;
	}
}

public class tk2dUIGridViewCellRowSort : IComparer<tk2dUIGridViewCell>
{
	
	public int Compare (tk2dUIGridViewCell gridItem1, tk2dUIGridViewCell gridItem2)
	{
		if(gridItem1.Row > gridItem2.Row)
		{
			return 1;
		}
		else if(gridItem1.Row < gridItem2.Row)
		{
			return -1;
		}
		else 
		{
			if(gridItem1.Col > gridItem2.Col)
			{
				return 1;
			}
			else if(gridItem1.Col < gridItem2.Col)
			{
				return -1;
			}
			else 
			{
				return 0;
			}
		}
	}
}


public class tk2dUIGridViewCellColSort : IComparer<tk2dUIGridViewCell>
{
	
	public int Compare (tk2dUIGridViewCell gridItem1, tk2dUIGridViewCell gridItem2)
	{
		if(gridItem1.Col > gridItem2.Col)
		{
			return 1;
		}
		else if(gridItem1.Col < gridItem2.Col)
		{
			return -1;
		}
		else 
		{
			if(gridItem1.Row > gridItem2.Row)
			{
				return 1;
			}
			else if(gridItem1.Row < gridItem2.Row)
			{
				return -1;
			}
			else 
			{
				return 0;
			}
		}
	}
}
