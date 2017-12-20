using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSpawner : MonoBehaviour {

	private Object[] buildings;
	private GameObject ground;
	private Terrain terrain;
	private GameObject buildingObject;
	private List<Building> offLimits;
	private Vector2 xBounds, zBounds;
	private Vector3 terrainDimensions;

	private List<Vector3> clusterList;


	private Terrain rightNeighbor;
	private Terrain leftNeighbor;
	private Terrain bottomNeighbor;
	private Terrain topNeighbor;
	//use terrain neighbors or not, no == performance
	public bool useNeighbors;

	//allows random rotation of buildings
	public bool rotateBuildings;

	//controls building distance from each other
	public float buildingBuffer;

	//controls the strictness of spawning buildings on uneven terrain
	public float buildingHeightBuffer;

	//Randomize the densities of the clusters
	public bool randomDensities;

	//randomize the clustersize
	public bool randomClusterSize;

	public int clusters;
	public int clusterSize;
	public float clusterBuildingDensity;
	private float clusterDistance;
	private Quadrant[] quadrants;
	private int clustersPerQuad;

	public List<GameObject> buildingLocations;
	private GameObject itemSpawner;
	void Start () {
		buildingLocations = new List<GameObject> ();
		ground = GameObject.FindGameObjectWithTag ("Ground");

		terrain = ground.GetComponent<Terrain> ();
		terrainDimensions = terrain.terrainData.size;
		if (useNeighbors) {
			SetTerrainNeighbors ();
		}
		/*
		if (!useNeighbors) {
			
			DisableNeighbors ();
		}
		*/
		CalculateQuadrants ();

		offLimits = new List<Building>();
		buildings = Resources.LoadAll ("Prefabs/Buildings");
		buildingObject = GameObject.Find ("Buildings");


		clustersPerQuad = clusters / 4;
		clusterList = new List<Vector3> ();
		clusterDistance = clusterBuildingDensity*clusters/clusterSize;

		//if using a cube for the ground
		//xBounds = new Vector2 (-ground.transform.localScale.x/2, ground.transform.localScale.x/2);
		//zBounds = new Vector2 (-ground.transform.localScale.z/2, ground.transform.localScale.z/2);

		//else terrain
		xBounds = new Vector2 (-terrainDimensions.x / 2,terrainDimensions.x / 2);
		zBounds = new Vector2 (-terrainDimensions.z / 2, terrainDimensions.z / 2);

		int quadrant = 0;
		for (int i = 0; i < clusters; ++i) {
			if (i != 0 && i % clustersPerQuad == 0 ) {
				quadrant++;
			}
			xBounds = quadrants [quadrant].xLimits;
			zBounds = quadrants [quadrant].zLimits;

				float randX = Random.Range (xBounds.x, xBounds.y);
				float randZ = Random.Range (zBounds.x, zBounds.y);
				//randomize cluster location
				Vector3 cluster;
				bool tooClose;
				//make sure clusters are spread out
			int tries = -1;
				do {
				tries++;
					tooClose = false;
					cluster = new Vector3 (randX, 0, randZ);
					foreach (Vector3 otherCluster in clusterList) {
						if ((otherCluster - cluster).sqrMagnitude < clusterDistance * clusterDistance) {
							tooClose = true;
						}
					}
				if(tries > 10){
					break;
				}
				} while(tooClose);
				clusterList.Add (cluster);
	

				//set cluster local bounds
			float buildingDensity;
			if (randomDensities) {
				buildingDensity = Random.Range (5f, clusterBuildingDensity);
			} else {
				buildingDensity = clusterBuildingDensity;
			}
				Vector2 clusterXBounds = new Vector2 (cluster.x - buildingDensity, cluster.x + buildingDensity);
				Vector2 clusterYBounds = new Vector2 (cluster.z - buildingDensity, cluster.z + buildingDensity);

			int townSize;
			if (randomClusterSize) {
				townSize = Random.Range (1, clusterSize);
			} else {
				townSize = clusterSize;
			}
				for (int j = 0; j < townSize; ++j) {
				
					//get building to spawn
					int temp = Random.Range (0, buildings.Length);
					GameObject newBuilding = (GameObject)buildings [temp];
					tries = 0;
					while (tries < 10) {
						//get local random bounds
						randX = Random.Range (clusterXBounds.x, clusterXBounds.y);
						randZ = Random.Range (clusterYBounds.x, clusterYBounds.y);

						//random rotation
						float randRot;
						if (rotateBuildings) {
							randRot = Random.Range (0f, 359f);
						} else {
							randRot = 0f;
						}

						Building offLimitBuilding = new Building ();
						newBuilding.transform.position = new Vector3 (randX, ground.transform.position.y, randZ);
						offLimitBuilding.center = newBuilding.transform.position;
						offLimitBuilding.xDim = newBuilding.transform.GetChild (0).GetChild (0).localScale.x;
						offLimitBuilding.zDim = newBuilding.transform.GetChild (0).GetChild (0).localScale.z;

						if (CheckBuildingLocation (offLimitBuilding)) {
							newBuilding = (GameObject)Instantiate (newBuilding, new Vector3 (randX, terrain.SampleHeight (newBuilding.transform.position), randZ), Quaternion.Euler (new Vector3 (0, randRot, 0)));
							newBuilding.transform.parent = buildingObject.transform;
							offLimits.Add (offLimitBuilding);
							buildingLocations.Add (newBuilding);
							break;
						}
						tries++;
					}
				}
		}

		GameObject wall = GameObject.FindGameObjectWithTag ("Wall");
		if (wall) {
			wall.GetComponent<Wall> ().Init ();
		}

		GameObject characterSpawner = GameObject.Find ("CharacterSpawner");
		if (characterSpawner) {
			characterSpawner.GetComponent<CharacterSpawner> ().Init ();
		}

		itemSpawner = GameObject.Find ("ItemSpawner");
		if (itemSpawner) {
			itemSpawner.GetComponent<ItemSpawner>().Spawn ();
		}

	}


	//checks for rectangle collisions. logic follows a 2d layout with top left being 0,0
	bool CheckBuildingLocation(Building newBuilding){
		foreach (Building building in offLimits) {

			//check collisions w/ other buildings
			if((Mathf.Abs(newBuilding.center.x-building.center.x) * 2 <= (newBuilding.xDim + building.xDim + buildingBuffer))&&
				(Mathf.Abs(newBuilding.center.z-building.center.z) * 2 <= (newBuilding.zDim + building.zDim + buildingBuffer))){
				return false;
			}

			//check world bounds
			if((newBuilding.center.x-newBuilding.xDim/2) < xBounds.x ||
				(newBuilding.center.x+newBuilding.xDim/2) > xBounds.y ||
				(newBuilding.center.z-newBuilding.zDim/2) < zBounds.x ||
				(newBuilding.center.z+newBuilding.zDim/2) > zBounds.y){
				return false;
			}

			//check terrain
			float frontRightHeight = terrain.SampleHeight(new Vector3(newBuilding.center.x+newBuilding.xDim/2,newBuilding.center.y,newBuilding.center.z+newBuilding.zDim/2));
			float frontLeftHeight = terrain.SampleHeight(new Vector3(newBuilding.center.x-newBuilding.xDim/2,newBuilding.center.y,newBuilding.center.z+newBuilding.zDim/2));
			float right = terrain.SampleHeight(new Vector3(newBuilding.center.x+newBuilding.xDim/2,newBuilding.center.y,newBuilding.center.z/2));
			float front = terrain.SampleHeight(new Vector3(newBuilding.center.x,newBuilding.center.y,newBuilding.center.z+newBuilding.zDim/2));
			float back = terrain.SampleHeight(new Vector3(newBuilding.center.x,newBuilding.center.y,newBuilding.center.z-newBuilding.zDim/2));
			float left = terrain.SampleHeight(new Vector3(newBuilding.center.x-newBuilding.xDim/2,newBuilding.center.y,newBuilding.center.z/2));
			float backLeftHeight = terrain.SampleHeight(new Vector3(newBuilding.center.x-newBuilding.xDim/2,newBuilding.center.y,newBuilding.center.z-newBuilding.zDim/2));
			float backRightHeight = terrain.SampleHeight(new Vector3(newBuilding.center.x+newBuilding.xDim/2,newBuilding.center.y,newBuilding.center.z-newBuilding.zDim/2));
			float centerHeight = terrain.SampleHeight(newBuilding.center);
			float[] values = new float[] {
				frontRightHeight,
				frontLeftHeight,
				front,
				back,
				backLeftHeight,
				backRightHeight,
				centerHeight,
				right,
				left
			};

			//makes sure that the building will be compared at the highest point
			float max = Mathf.Max (values);
			for(int i = 0; i < values.Length; ++i){
				if (values[i] == max) {
					values[i] = 0f;
					break;
				}
			}

			//remove the max value for calculation
			float avg = 0f;
			foreach (float val in values) {
				avg += val;
			} 
			avg = avg / 8;

			//compare to max value

			if (avg > max + buildingHeightBuffer || avg < max - buildingHeightBuffer) {
				//terrain too uneven
				return false;
			}
		}
		return true;
	}

	void SetTerrainNeighbors(){
		terrain = ground.GetComponent<Terrain> ();
		rightNeighbor = GameObject.Find ("RightNeighbor").GetComponent<Terrain>();
		leftNeighbor = GameObject.Find ("LeftNeighbor").GetComponent<Terrain>();
		topNeighbor = GameObject.Find ("TopNeighbor").GetComponent<Terrain>();
		bottomNeighbor = GameObject.Find ("BottomNeighbor").GetComponent<Terrain>();

		terrain.SetNeighbors (leftNeighbor, topNeighbor, rightNeighbor, bottomNeighbor);
		rightNeighbor.SetNeighbors (terrain, null, null, null);
		leftNeighbor.SetNeighbors (null, null, terrain, null);
		topNeighbor.SetNeighbors (null, null, null, terrain);
		bottomNeighbor.SetNeighbors (null, terrain, null, null);
	}

	void CalculateQuadrants(){
		quadrants = new Quadrant[4];
		for (int i = 0; i < quadrants.Length; ++i) {
			quadrants [i] = new Quadrant ();
		}
//		 _ _
//		|0|1|
//		|2|3|
		 
		quadrants [0].xLimits = new Vector2 (-terrainDimensions.x / 2, 0f);
		quadrants [0].zLimits = new Vector2 (0f, terrainDimensions.z / 2);
		quadrants [1].xLimits = new Vector2 (0f, terrainDimensions.x / 2);
		quadrants [1].zLimits = new Vector2 (0f, terrainDimensions.z / 2);
		quadrants [2].xLimits = new Vector2 (-terrainDimensions.x/2, 0f);
		quadrants [2].zLimits = new Vector2 (-terrainDimensions.z/2, 0f);
		quadrants [3].xLimits = new Vector2 (0f, terrainDimensions.x / 2);
		quadrants [3].zLimits = new Vector2 (-terrainDimensions.z / 2, 0f);


	}

	void DisableNeighbors(){
		rightNeighbor.enabled = false;
		leftNeighbor.enabled = false;
		topNeighbor.enabled = false;
		bottomNeighbor.enabled = false;

		terrain.SetNeighbors (null, null, null, null);
	}
}

struct Building{
	//center of gameobject
	public Vector3 center;

	//size of x and y directions
	public float xDim,zDim;
}

//used for storing in an array
struct Quadrant{
	//xmin and xmax
	public Vector2 xLimits;

	//zmin and zmax
	public Vector2 zLimits;
}