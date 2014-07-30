using System;

public static class tk2dUIScrollableAreaExtensions  
{
	public static void EnableScroll(this tk2dUIScrollableArea scrollView)
	{
		if (scrollView.backgroundUIItem != null && !scrollView.allowTouchScrolling)
		{
			scrollView.allowTouchScrolling = true;

			scrollView.backgroundUIItem.OnDown += scrollView.BackgroundButtonDown;
			scrollView.backgroundUIItem.OnRelease += scrollView.BackgroundButtonRelease;
		}
	}

	public static void DisableScroll(this tk2dUIScrollableArea scrollView)
	{
		if (scrollView.backgroundUIItem != null && scrollView.allowTouchScrolling)
		{
			scrollView.allowTouchScrolling = false;

			scrollView.backgroundUIItem.OnDown -= scrollView.BackgroundButtonDown;
			scrollView.backgroundUIItem.OnRelease -= scrollView.BackgroundButtonRelease;
		}
		
		if (scrollView.isBackgroundButtonDown || scrollView.isSwipeScrollingInProgress)
		{
			if (tk2dUIManager.Instance__NoCreate != null)
			{
				tk2dUIManager.Instance.OnInputUpdate -= scrollView.BackgroundOverUpdate;
			}
			scrollView.isBackgroundButtonDown = false;
			scrollView.isSwipeScrollingInProgress = false;
		}
	
		scrollView.swipeCurrVelocity = 0;
	}

	public static void Scroll2End(this tk2dUIScrollableArea scrollView)
	{
		scrollView.Value = 1.0f;
	}
}
