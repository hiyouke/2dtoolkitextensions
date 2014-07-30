 using UnityEngine;
using System.Collections;

public class tk2dUIPageView : tk2dUIScrollableArea 
{
	public bool paged = false;

	public System.Action<tk2dUIPageView> OnActivePage;

	float mTouchBeganTime = 0;

	float mTouchBeganOffset = 0;

	int mTargetPage = 0;

	int mCurrPage = 0;//从第0页开始

	private float actionPercentage = 1,duration = 0;

	private float runningTime;

	private Vector3[] vector3s = new Vector3[3];
	
	protected override  void OnEnable()
	{
		base.OnEnable ();
		
		if (backgroundUIItem != null)
		{
			backgroundUIItem.OnDown += OnPageBackgroundDown;
			backgroundUIItem.OnRelease += OnPageBackgroundRelease;
		}
	}
	
	protected override  void OnDisable()
	{
		base.OnEnable ();
		
		if (backgroundUIItem != null)
		{
			backgroundUIItem.OnDown -= OnPageBackgroundDown;
			backgroundUIItem.OnRelease -= OnPageBackgroundRelease;
		}
	}

	public int CurrentPage
	{
		get
		{
			return mCurrPage;
		}
	}

	public void SetCurrentPage(int page,bool animated)
	{
		mCurrPage = mTargetPage = page;

		float pageTargetOffset = mTargetPage * VisibleAreaLength;
		Vector2 target = new Vector2 ();
		if(scrollAxes == Axes.YAxis)
		{
			target.x = ContentContainerOffset.x;
			target.y = pageTargetOffset;
		}
		else
		{
			target.y = ContentContainerOffset.y;
			target.x = pageTargetOffset;
		}

		if(animated)
		{
			SetContentOffsetInDuration (target, 0.4f);
		}
		else
		{
			SetContentOffsetInDuration (target, 0.04f);
		}
	}

	public int MaxPage()
	{
		//从第0页开始
		int maxPage = 0;
		int tmpContent = (int)ContentLength;
		int tmpVisible = (int)VisibleAreaLength;
		if (tmpContent > tmpVisible) 
		{
			maxPage = (0 == tmpContent % tmpVisible) ? tmpContent/tmpVisible - 1 : tmpContent/tmpVisible;
		}
		return maxPage;
	}

	void Update()
	{
		if(!isBackgroundButtonDown && actionPercentage != 1)
		{
			UpdateActionPercentage();

			float newPercent = Value;
			if(scrollAxes == Axes.YAxis)
			{
				vector3s[2].y = Mathf.Lerp(vector3s[0].y,vector3s[1].y,actionPercentage);

				newPercent = vector3s[2].y / (ContentLength - VisibleAreaLength);
			}
			else
			{
				vector3s[2].x = Mathf.Lerp(vector3s[0].x,vector3s[1].x,actionPercentage);

				newPercent = vector3s[2].x / (ContentLength - VisibleAreaLength);
			}
			Value = newPercent;
		}
	}

	void UpdateActionPercentage()
	{
		runningTime += Time.deltaTime;

		actionPercentage = runningTime/duration;

		if(actionPercentage  >= 1)
		{
			actionPercentage = 1;

			if(null != OnActivePage)
			{
				OnActivePage(this);
			}
		}
	}
	
	void OnPageBackgroundDown()
	{
		if(paged)
		{
			PageTouchBegan ();
		}
	}

	void OnPageBackgroundRelease()
	{
		if(paged)
		{
			PageTouchEnd ();

			PageTouchCancel ();
		}
	}

	void SetContentOffsetInDuration(Vector2 offset,float dt)
	{
		actionPercentage = 0;

		runningTime = 0;

		duration = dt;

		//from 
		vector3s [0] = new Vector3 (ContentContainerOffset.x, ContentContainerOffset.y,0);

		//to 
		vector3s [1] = new Vector3 (offset.x,offset.y,0);

		//value
		vector3s [2] = new Vector3 (0, 0,0);
	}

	#region touch 
	void PageTouchBegan()
	{
		mTouchBeganTime = Time.time;
		mTouchBeganOffset = (scrollAxes == Axes.YAxis ? ContentContainerOffset.y : ContentContainerOffset.x);
	}

	void PageTouchEnd()
	{
		if(VisibleAreaLength <= 0)
		{
			return;
		}
		int MAX_PAGE = (int)(ContentLength / VisibleAreaLength) - 1;//从第0页开始
		int MIN_PAGE = 0;
		float TURN_PAGE_MIN_OFFSET_RATIO = 0.4f;//滑动超过0.4
		float TURN_PAGE_SPEED = VisibleAreaLength;//designPixl/ms			
		float currOffset = scrollAxes == Axes.YAxis ? ContentContainerOffset.y : ContentContainerOffset.x;
		float deltaOffset = currOffset - mTouchBeganOffset;
		float currTime = Time.time;
		float speed = currTime != mTouchBeganTime ? deltaOffset / (currTime - mTouchBeganTime) : 0;
		mTargetPage = mCurrPage;
		if( Mathf.Abs(deltaOffset) > TURN_PAGE_MIN_OFFSET_RATIO * VisibleAreaLength )
		{
			//滑动距离大于一个阈值，认为是下一页
			if(deltaOffset > 0)
			{
				mTargetPage = mCurrPage + 1;
			}
			else 
			{
				mTargetPage = mCurrPage - 1;
			}
		}
		else if(Mathf.Abs(speed) > TURN_PAGE_SPEED)
		{
			//速度大于一个阈值,也认为是下一页
			if(speed > 0)
			{
				mTargetPage = mCurrPage + 1;
			}
			else 
			{
				mTargetPage = mCurrPage - 1;
			}
		}

		if(mTargetPage > MAX_PAGE)
		{
			mTargetPage = MAX_PAGE;
		}
		else if(mTargetPage < MIN_PAGE)
		{
			mTargetPage = MIN_PAGE;
		}

		float pageTargetOffset = mTargetPage * VisibleAreaLength;

		RemoveBackgroundOverUpdate ();

		Vector2 target = new Vector2 ();
		if(scrollAxes == Axes.YAxis)
		{
			target.x = ContentContainerOffset.x;
			target.y = pageTargetOffset;
		}
		else
		{
			target.y = ContentContainerOffset.y;
			target.x = pageTargetOffset;
		}
		SetContentOffsetInDuration (target, 0.4f);

		mCurrPage = mTargetPage;
	}

	void PageTouchCancel()
	{
		PageClearTouch ();
	}

	void PageClearTouch()
	{
		mTouchBeganOffset = 0;
		mTouchBeganTime = 0;
		mTargetPage = mCurrPage;
	}
	#endregion
}
