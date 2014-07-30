using System;
using UnityEngine;

//Grid Item 大小在给定的row,col
public delegate Vector2 tk2dUIGridViewCellSizeDelegate(tk2dUIGridView gridView, int row,int col);

//索引位置的cell实例
public delegate tk2dUIGridViewCell tk2dUIGridViewCellDelegate(tk2dUIGridView gridView,int row,int col);

//响应touch begin事件
public delegate void tk2dUIGridViewCellTouchBeginDelegate(tk2dUIGridView gridView,tk2dUIGridViewCell cell);

//响应点击事件
public delegate void tk2dUIGridViewCellClickdDelegate(tk2dUIGridView gridView,tk2dUIGridViewCell cell);

//响应长按事件 
public delegate void tk2dUIGridViewCellLongPressDelegate(tk2dUIGridView gridView,tk2dUIGridViewCell cell);

//响应touch End事件
public delegate void tk2dUIGridViewCellTouchEndDelegate(tk2dUIGridView gridView,tk2dUIGridViewCell cell);

//响应移动事件
public delegate void tk2dUIGridViewCellMovedDelegate(tk2dUIGridView gridView,tk2dUIGridViewCell cell);