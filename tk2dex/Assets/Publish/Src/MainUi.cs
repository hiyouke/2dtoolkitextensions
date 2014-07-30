using UnityEngine;
using System.Collections;

public class MainUi : MonoBehaviour
{
	public tk2dTextMesh lblProgress = null;

	public tk2dSprite sprIcon = null;

	public tk2dUIGridView gridView = null;

	void Awake()
	{
		gameObject.AddComponent<IconSpriteManager> ();

		gridView.cellDelegate = GridCellForPos;

		gridView.ReloadData ();
	}

	#region UI Action
	void OnLoadClick()
	{
		for(int i= 101;i<=110;i++)
		{
			IconSpriteManager.msInst.AddImage2Cahce("Textures/item_" + i,78,78);
		}
		IconSpriteManager.msInst.BeginLoading ();
		IconSpriteManager.msInst.progressDelegate = ProgressCallback; 
		IconSpriteManager.msInst.loadedDelegate = LoadedCallback;
	}

	void ProgressCallback(IconSpriteManager manager,float progress)
	{
		lblProgress.text = (int)(progress*100) + "%";
	}

	void LoadedCallback(IconSpriteManager manager)
	{
		lblProgress.text = "loaded";

		sprIcon.SetSprite (IconSpriteManager.msInst.SpriteCollectionData, "item_101");
	}
	#endregion

	#region gridview 
	tk2dUIGridViewCell GridCellForPos(tk2dUIGridView gridView, int row, int col)
	{
		CustomGridViewCell cell = (CustomGridViewCell)gridView.DequeueCell();
		if (null == cell)
		{
			GameObject cellObj = UnityEngine.Object.Instantiate(gridView.cellTempObj) as GameObject;
			cell = cellObj.GetComponent<CustomGridViewCell>();
		}
		cell.lblTitle.text = "Title" + row;
		cell.gameObject.SetActive (true);
		return cell;
	}
	#endregion
}
