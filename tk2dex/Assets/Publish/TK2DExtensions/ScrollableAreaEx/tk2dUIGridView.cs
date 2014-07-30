using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class tk2dUIGridView : tk2dUIPageView 
{	
	const int UI_INVALID_POS = -1;
		
	//子项模板
	public GameObject cellTempObj = null;

	//第一个位置坐标，默认为{0,0}
	public Vector2 firstPosition = new Vector2(0,0);

	//行数
	public int numberOfRows = 0;
	public int NumberOfRows
	{
		set { numberOfRows = value;  }
		get { return numberOfRows; }
	}

	//列数
	public int numberOfCols = 0;

	//间格
	public int spacing = 0;

	//cell 大小 
	public Vector2 cellSize = new Vector2(0,0);
		
	public bool startReload = false;

	public bool isEdting = false;

	public tk2dUIGridViewCellSizeDelegate cellSizeDelegate = null;

	public tk2dUIGridViewCellDelegate cellDelegate = null;

	public tk2dUIGridViewCellClickdDelegate  cellClickedDelegate = null;
	
	public tk2dUIGridViewCellLongPressDelegate cellLongPressDelegate = null;

	public tk2dUIGridViewCellTouchBeginDelegate cellTouchBeginDelegate = null;

	public tk2dUIGridViewCellMovedDelegate cellMovedDelegate = null;

	public tk2dUIGridViewCellTouchEndDelegate cellTouchEndDelegate = null;
	
	protected List<tk2dUIGridViewCell> m_CellsUsed  = new List<tk2dUIGridViewCell>();
	
	protected List<tk2dUIGridViewCell> m_CellsFreed = new List<tk2dUIGridViewCell>();

	protected Vector2[][] m_CellsPositions = null;
		
	private bool isGridViewBackgroundButtonDown = false;

	private tk2dUIGridViewCell mSelectedCell = null;

	//private Vector3 mTouchDownPoint = Vector3.zero;//按下坐标坐标位置

	Vector2 mTouchDownContentOffset = Vector3.zero;
	
	void Awake()
	{
		OnScroll += ScrollViewDidScroll;
	}

	void Start()
	{
		if(startReload)
		{
			 ReloadData ();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable ();

		if (backgroundUIItem != null)
		{
			backgroundUIItem.OnClick += GridViewBackgroundButtonClick;

			backgroundUIItem.OnDown += GridViewBackgroundButtonDown;

			backgroundUIItem.OnRelease += GridViewBackgroundButtonRelease;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable ();

		if (backgroundUIItem != null)
		{
			backgroundUIItem.OnClick -= GridViewBackgroundButtonClick;

			backgroundUIItem.OnDown -= GridViewBackgroundButtonDown;

			backgroundUIItem.OnRelease -= GridViewBackgroundButtonRelease;
		}
	}

	public void ReloadData()
	{
		foreach(tk2dUIGridViewCell cell in m_CellsUsed)
		{
			m_CellsFreed.Add(cell);
			cell.ResetRowAndCol();
			cell.gameObject.SetActive(false);
		}
		m_CellsUsed.Clear ();
		
		UpdateGridItemPositions ();
		
		UpdateContentLength ();
		
		ScrollViewDidScroll (this);
	}

	public void ClearAll()
	{
		foreach(tk2dUIGridViewCell cell in m_CellsUsed)
		{
			GameObject.Destroy(cell.gameObject);
		}
		m_CellsUsed.Clear ();

		foreach(tk2dUIGridViewCell cell in m_CellsFreed)
		{
			GameObject.Destroy(cell.gameObject);
		}
		m_CellsFreed.Clear ();
	}

	public void ScrollToRow(int row)
	{
		if(row < 0  || row >= numberOfRows)
		{
			return;
		}

		float tmpOffet = 0;
		for(int i=1;i<=row;i++)
		{
			tmpOffet += m_CellsPositions[i][0].y;
		}
		Value = tmpOffet / (ContentLength - VisibleAreaLength);

	}

	public void ScrollToCol(int col)
	{
		if(col < 0  || col >= numberOfCols)
		{
			return;
		}
		
		float tmpOffet = 0;
		for(int i=1;i<=col;i++)
		{
			tmpOffet += m_CellsPositions[0][i].x;
		}
		Value = tmpOffet / (ContentLength - VisibleAreaLength);
	}

	#region Input 事件
	public Vector3 GetTouchedPointInWorld()
	{
		Vector2 point = backgroundUIItem.Touch.position;//点击屏幕坐标
		Camera camera = tk2dUIManager.Instance.GetUICameraForControl (gameObject);
		Vector3 worldPos = camera.ScreenToWorldPoint (new Vector3 (point.x, point.y, 0));//点击的世界坐标
		worldPos.z = 0;
		return worldPos;
	}

	Vector3 GetTouchedPointInLocal()
	{
		Vector2 point = backgroundUIItem.Touch.position;//点击屏幕坐标
		Camera camera = tk2dUIManager.Instance.GetUICameraForControl (gameObject);
		Vector3 worldPos = camera.ScreenToWorldPoint (new Vector3 (point.x, point.y, 0));//点击的世界坐标
		Vector3 localPos = worldPos - transform.position;//相对当前gameObject坐标
		return localPos;
	}

	Vector2 GetTouchedLocalOffset()
	{
		Vector3 localPos = GetTouchedPointInLocal ();

		localPos.y = -localPos.y;
		
		Vector3 localOffset = ContentContainerOffset + localPos;

		return localOffset;
	}
	
	public tk2dUIGridViewCell GetTouchedCell()
	{
		Vector3 localPos = GetTouchedPointInLocal ();

		localPos.y = -localPos.y;

		Vector3 localOffset = ContentContainerOffset + localPos;

		tk2dUIGridPos position = GridPosFromOffset (localOffset);

		tk2dUIGridViewCell cell = GridCellAtPosition (position.row,position.col);

		return cell;
	}

	bool IsTouchedValid()
	{
		bool ret = true;
		Vector2 localOffset = GetTouchedLocalOffset ();
		if(Axes.XAxis == scrollAxes)
		{
			float offset = m_CellsPositions[0][numberOfCols].x;

			if(localOffset.x > offset)
			{
				ret = false;
			}
		}
		else
		{
			float offset = m_CellsPositions[numberOfRows][0].y;

			if(localOffset.y > offset)
			{
				ret = false;
			}
		}
		return ret;
	}

	void GridViewBackgroundButtonClick()
	{
		if(IsTouchedValid())
		{
			tk2dUIGridViewCell cell = GetTouchedCell();
			if(null != cell && null != mSelectedCell)
			{
				if(mSelectedCell.Row == cell.Row && mSelectedCell.Col == cell.Col)
				{
					Vector3 nowOffset = ContentContainerOffset;
					float distance = Vector3.Distance(nowOffset,mTouchDownContentOffset);
					float DISTANCE_THRESHOLD = 4.0f ;// 4.0f 检测移动小于一个临界值，检测有效点击
					if(distance < DISTANCE_THRESHOLD)
					{
						if(null != cellClickedDelegate){ cellClickedDelegate(this,cell); }
					}
				}
			}
		}
	}

	void GridViewBackgroundButtonDown()
	{
		mTouchDownContentOffset = ContentContainerOffset;

		mSelectedCell = GetTouchedCell();

		if(isEdting)
		{
			isGridViewBackgroundButtonDown = true;

			if(null != cellTouchBeginDelegate){ cellTouchBeginDelegate(this,mSelectedCell);}

			tk2dUIManager.Instance.OnInputUpdate += GridViewBackgroundTouchUpdate;

			if(null != cellLongPressDelegate)
			{
				Vector3 downPoint = GetTouchedPointInLocal();
				StopCoroutine("CheckLongPress");
				StartCoroutine(CheckLongPress(downPoint));
			}
		}
	}

	void GridViewBackgroundButtonRelease()
	{
		mTouchDownContentOffset = Vector3.zero;

		if(isEdting)
		{
			isGridViewBackgroundButtonDown = false;
	
			if(null != cellTouchEndDelegate){ cellTouchEndDelegate(this,GetTouchedCell()); }

			tk2dUIManager.Instance.OnInputUpdate -= GridViewBackgroundTouchUpdate;

			StopCoroutine("CheckLongPress");
		}
	}

	void GridViewBackgroundTouchUpdate()
	{
		if(isGridViewBackgroundButtonDown)
		{
			if(null != cellMovedDelegate){ cellMovedDelegate(this,mSelectedCell); } 
		}
	}

	IEnumerator CheckLongPress(Vector3 downPoint) 
	{
		float TIME_THRESHOLD = 0.2f;//检测时间临界值
		for (float t = 0; t < TIME_THRESHOLD; t += tk2dUITime.deltaTime) 
		{
			yield return 0;
		}
		if(isGridViewBackgroundButtonDown)
		{
			Vector3 nowPoint = GetTouchedPointInLocal();
			float distance = Vector3.Distance(nowPoint,downPoint);
			float DISTANCE_THRESHOLD = 4.0f;//检测移动小于一个临界值，检测长按
			if(distance < DISTANCE_THRESHOLD)
			{
				if(null != cellLongPressDelegate){ cellLongPressDelegate(this,mSelectedCell); }
			}
		}
	}	
	#endregion

	public Vector2 GridCellSizeAtPos(int row,int col)
	{
		if(null == cellSizeDelegate)
		{
			return cellSize;
		}
		else
		{
			return cellSizeDelegate(this,row,col);
		}
	}

	public tk2dUIGridViewCell GridCellAtPosition(int row,int col)
	{
		tk2dUIGridViewCell findCell = null;
		System.Predicate<tk2dUIGridViewCell> match = delegate(tk2dUIGridViewCell cell) { return (cell.Row == row) && (cell.Col == col);};
		findCell = m_CellsUsed.Find (match);
		return findCell;
	}
		
	public void UpdateContentLength()
	{
		float maxPosition = 0;
		if(scrollAxes == Axes.YAxis)
		{
			maxPosition = m_CellsPositions[numberOfRows][0].y;
			ContentLength = Mathf.Max (maxPosition, VisibleAreaLength);
		}
		else
		{
			maxPosition = m_CellsPositions[0][numberOfCols].x;
			ContentLength = Mathf.Max (maxPosition, VisibleAreaLength);
		}
	}
	
	public void UpdateGridItemPositions() 
	{
		//初始化数据
		m_CellsPositions = new Vector2[numberOfRows + 1][];
		for(int i=0;i<m_CellsPositions.Length;i++)
		{
			m_CellsPositions[i] = new Vector2[numberOfCols + 1];
		}

		Vector2 currentPos = new Vector2 (0,0);
		Vector2 tGridSize = new Vector2(0,0);
		float maxSize = 0;
		if(scrollAxes == Axes.YAxis)
		{
			//横向滚动，Content Length，为第行最大高度相加
			for(int row=0;row<numberOfRows;row++)
			{
				maxSize = 0;
				for(int col=0;col<numberOfCols;col++)
				{
					tGridSize = GridCellSizeAtPos(row,col);

					if(tGridSize.y > maxSize)
					{
						maxSize = tGridSize.y;
					}

					m_CellsPositions[row][col] = currentPos;
					currentPos.x += (tGridSize.x + spacing);
				}
				m_CellsPositions[row][numberOfCols] = currentPos;
				currentPos.y += (maxSize + spacing);
				currentPos.x = 0;
			}
			m_CellsPositions[numberOfRows][0] = currentPos;
		}
		else 
		{
			//纵向滚动，Content Length，为第列最大高度相加
			for(int col=0;col<numberOfCols;col++)
			{
				maxSize = 0;
				for(int row=0;row<numberOfRows;row++)
				{
					tGridSize = GridCellSizeAtPos(row,col);
					if(tGridSize.x > maxSize)
					{
						maxSize = tGridSize.x;
					}
					m_CellsPositions[row][col] = currentPos;
					currentPos.y += (tGridSize.y + spacing);
				}
				m_CellsPositions[numberOfRows][col] = currentPos;
				currentPos.x += (maxSize + spacing);
				currentPos.y = 0;
			}
			m_CellsPositions[0][numberOfCols] = currentPos;
		}
	}

	void SetGridItemPosition(tk2dUIGridViewCell gridItem,int row,int col)
	{ 
		Vector2 position = firstPosition + m_CellsPositions [row] [col];
		position.y *= -1;
		gridItem.transform.localPosition = position;

		//Utility.Log ("idx : " + idx + " CellSightSize : " + CellSightSize + " position : " + position);
	}
	
	tk2dUIGridPos GridPosFromOffset(Vector3 offset)
	{
		float search = 0;
		Vector2 low = new Vector2(0,0);
		Vector2 high = new Vector2(0,0);
		int row = -1,col = -1;
		if(scrollAxes == Axes.YAxis )
		{
			search =  offset.y;
		
			//如果偏移位置 > ContentLength,直接返回最后索引位置
			if(search >= ContentLength)
			{
				row = numberOfRows - 1;
			}
			else if(search < 0)
			{
				row = 0;
			}
			else 
			{
				for(int i = 0;i< numberOfRows;i++)
				{
					low = m_CellsPositions[i][0];
					high = m_CellsPositions[i+1][0];
					if(search >= low.y && search < high.y)
					{
						row = i; 
						break;
					}
				}
			}

			//获取列
			search =  offset.x;
			if(row != -1)
			{
				for(int i=0;i<numberOfCols;i++)
				{
					low = m_CellsPositions[row][i];
					high = m_CellsPositions[row][i+1];
					if(search >= low.x && search < high.x)
					{
						col = i; 
						break;
					}
				}
			}
		}
		else
		{
			//先查找出列
			search = offset.x;

			if(search >= ContentLength)
			{
				col = numberOfCols -1;
			}
			else if(search < 0)
			{
				col = 0;
			}
			else
			{
				for(int i=0;i<numberOfCols;i++)
				{
					low  = m_CellsPositions[0][i];
					high = m_CellsPositions[0][i+1];
					if(search >= low.x && search < high.x)
					{
						col = i;
						break;
					}
				}
			}
			//获取行
			search = offset.y;
			if(col != -1)
			{
				for(int i=0;i<numberOfRows;i++)
				{
					low = m_CellsPositions[i][col];
					high = m_CellsPositions[i+1][col];
					if(search >= low.y && search < high.y)
					{
						row = i;
						break;
					}
				}
			}
		}
		return new tk2dUIGridPos(row,col);
	}
	
	void MoveCellOutOfSight(tk2dUIGridViewCell cell)
	{
		m_CellsFreed.Add (cell);
		m_CellsUsed.Remove (cell);
		if(scrollAxes == Axes.YAxis)
		{
			m_CellsUsed.Sort (new tk2dUIGridViewCellRowSort ()); 
		}
		else
		{
			m_CellsUsed.Sort(new tk2dUIGridViewCellColSort());
		}

		cell.transform.localPosition = new Vector3 (0, Screen.height * 2 ,0);
		cell.ResetRowAndCol ();
	}
	
	public tk2dUIGridViewCell CreateNewCell()
	{
		if(cellTempObj == null)
		{
			return null;
		}
		GameObject cellObj =  Object.Instantiate(cellTempObj) as GameObject;
		cellObj.transform.parent = contentContainer.transform;
		return cellObj.GetComponent<tk2dUIGridViewCell> ();
	}
	
	public tk2dUIGridViewCell DequeueCell()
	{
		if(m_CellsFreed.Count == 0)
		{
			return null;
		}
		else
		{
			tk2dUIGridViewCell cell = m_CellsFreed[0];
			m_CellsFreed.RemoveAt(0);
			return cell;
		}
	}
	
	void ResetUnuseStartAndEnd(tk2dUIGridPos startPos,tk2dUIGridPos endPos)
	{
		if(scrollAxes == Axes.YAxis)
		{
			//不可以行移除
			int row = 0,maxRow = Mathf.Max (numberOfRows - 1, 0);
			if(m_CellsUsed.Count > 0)
			{
				tk2dUIGridViewCell gridItem = m_CellsUsed[0];
				row = gridItem.Row;
				while(row < startPos.row)
				{
					MoveCellOutOfSight(gridItem);
					if(m_CellsUsed.Count > 0)
					{
						gridItem = m_CellsUsed[0];
						row = gridItem.Row;
					}
					else
					{
						break;
					}
				}

				if(m_CellsUsed.Count - 1 > 0)
				{
					gridItem = m_CellsUsed[m_CellsUsed.Count-1];
					row = gridItem.Row;
					while(row <= maxRow && row >endPos.row)
					{
						MoveCellOutOfSight(gridItem);
						if(m_CellsUsed.Count - 1 > 0)
						{
							gridItem = m_CellsUsed[m_CellsUsed.Count-1];
							row = gridItem.Row;
						}
						else
						{
							break;
						}
					}
				}
			}
		}
		else
		{
			//不可见列移除
			int col = 0,maxCol = Mathf.Max(numberOfCols -1,0);
			if(m_CellsUsed.Count > 0)
			{
				//起始位置不可见部分
				tk2dUIGridViewCell cell = m_CellsUsed[0];
				col = cell.Col;
				while(col < startPos.col)
				{
					MoveCellOutOfSight(cell);
					if(m_CellsUsed.Count > 0)
					{
						cell = m_CellsUsed[0];
						col = cell.Col;
					}
					else
					{
						break;
					}
				}

				//结束位置不可见部分
				if(m_CellsUsed.Count - 1 > 0)
				{
					cell = m_CellsUsed[m_CellsUsed.Count - 1];
					col = cell.Col;
					while(col <= maxCol && col > endPos.col)
					{
						MoveCellOutOfSight(cell);
						if(m_CellsUsed.Count - 1 > 0)
						{
							cell = m_CellsUsed[m_CellsUsed.Count - 1];
							col = cell.Col;
						}
						else
						{
							break;
						}
					}
				}
			}
		}
	}
	
	void UpdateGridItemAtPos(int row,int col)
	{
		if(row == UI_INVALID_POS || col == UI_INVALID_POS)
		{
			return;
		}
		
		tk2dUIGridViewCell gridItem = null;
		for(int i=0;i<m_CellsUsed.Count;i++)
		{
			if(m_CellsUsed[i].Row == row && m_CellsUsed[i].Col == col)
			{
				gridItem = m_CellsUsed[i];
				MoveCellOutOfSight(gridItem);
				break;
			}
		}
		gridItem = cellDelegate != null ? cellDelegate (this, row, col) : DequeueCell ();
		if(gridItem == null)
		{
			gridItem = CreateNewCell();
		}
		gridItem.transform.parent = contentContainer.transform;
		gridItem.SetPosition (row, col);
		SetGridItemPosition (gridItem, row,col);
		m_CellsUsed.Add(gridItem);
		if(cellDelegate == null)
		{
			//delegate 去控制是否Active
			gridItem.gameObject.SetActive(true);
		}
		if(scrollAxes == Axes.YAxis)
		{
			m_CellsUsed.Sort (new tk2dUIGridViewCellRowSort ()); 
		}
		else
		{
			m_CellsUsed.Sort(new tk2dUIGridViewCellColSort());
		}
	}

	void UpdateGridItemInRange(tk2dUIGridPos start,tk2dUIGridPos end)
	{
		if(scrollAxes == Axes.YAxis)
		{
			for(int row = start.row;row <= end.row;row++)
			{
				for(int col = 0;col < numberOfCols;col++)
				{
					System.Predicate<tk2dUIGridViewCell> match = delegate(tk2dUIGridViewCell gridItem) 
					{
						return (gridItem.Row == row && gridItem.Col == col);
					};
					if(null == m_CellsUsed.Find(match))
					{
						UpdateGridItemAtPos(row,col);
					}
				}
			}
		}
		else
		{
			for(int col = start.col;col <= end.col;col++)
			{
				for(int row = 0;row < numberOfRows;row++)
				{
					System.Predicate<tk2dUIGridViewCell> match = delegate(tk2dUIGridViewCell cell) 
					{
						return (cell.Row == row && cell.Col == col);
					};
					if(null == m_CellsUsed.Find(match))
					{
						UpdateGridItemAtPos(row,col);
					}
				}
			}
		}
	}
	public void Reset()
	{
		numberOfCols = 0;
		numberOfRows = 0;
		ContentContainerOffset = new Vector3(0,0,ContentContainerOffset.z);
		Value = 0;
		ReloadData ();
	}
	void ScrollViewDidScroll(tk2dUIScrollableArea scrollView)
	{
		tk2dUIGridPos start, end;
		Vector3 offset = ContentContainerOffset; 
		start = GridPosFromOffset (offset);
		if(scrollAxes == Axes.YAxis)
		{
			offset.y += VisibleAreaLength;
			end = GridPosFromOffset (offset);
		}
		else
		{
			offset.x += VisibleAreaLength;
			end = GridPosFromOffset (offset);
		}
		if(end.row == -1 || end.col == -1)
		{
			end.row = numberOfRows -1;
			end.col = numberOfCols -1;
		}
		ResetUnuseStartAndEnd (start, end);
		UpdateGridItemInRange (start, end);

		//Utility.Log (" cell count : " + m_CellsUsed.Count + " free count : " + m_CellsFreed.Count);
	}
}
