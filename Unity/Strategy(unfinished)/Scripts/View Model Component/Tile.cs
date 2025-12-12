using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Tile : MonoBehaviour
{
	public enum TileType {
		Dirt, // 0
		Rock, // 1
		Tree, // 2
		River // 3
		}
	public TileType type;
    public bool isPassable;
	public Material[] materials = new Material[4]; // 현재
	public Material OriginalMaterial { get; private set; }
	


	#region Properties
	public const float stepHeight = 0.25f;

	public Point pos;
	public int height;
	
	public Vector3 center { get { return new Vector3(pos.x, height * stepHeight, pos.y); }}
	public GeneralUnit general;
	
	public GameObject content;
	[HideInInspector] public Tile prev;
	[HideInInspector] public int distance;
	
	#endregion

	#region Public
	public void Materialize (int index)
	{
		if (index < 0 || index > 3)
			return;
		type = (TileType)index;
		if (type == TileType.Tree || type == TileType.River)
			isPassable = false;
		else
			isPassable = true;
		GetComponent<Renderer>().material = materials[index];
		OriginalMaterial = GetComponent<Renderer>().material;
	}
	public void Load (Point p, int h)
	{
		
		pos = p;
		height = h;
		Match();
	}

	public void Load (Vector3 v)
	{
		
		Load (new Point((int)v.x, (int)v.z), (int)v.y);
	}
	public void Load (Point p, int h, TileType t)
	{
		
		pos = p;
		height = h;
		type = t;
		Materialize((int)t);
		Match();
	}

	public void Grow ()
	{
		height++;
		Match();
	}

	public void Shrink ()
	{
		height--;
		Match ();
	}
	#endregion

	#region Private
	void Match ()
	{
		transform.localPosition = new Vector3( pos.x, height * stepHeight / 2f, pos.y );
		transform.localScale = new Vector3(1, height * stepHeight, 1);
	}
	#endregion
}