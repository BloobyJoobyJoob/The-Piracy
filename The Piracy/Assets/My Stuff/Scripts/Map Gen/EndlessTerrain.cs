using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;

[System.Serializable]
public class SharedFoliageData {
	public int triangleCountNameID;
	public int terrainMeshNameID;
	public int seedNameID;
}

public class EndlessTerrain : MonoBehaviour {

	public float chunkUpdateDelay = 1;
	public float maxViewDst = 450;
	public Transform viewer;
	public static Vector2 viewerPosition;
	public string triangleCountName;
	public string terrainMeshName;
	public string seedName;
	public VisualEffect[] foliagePrefabs;

	public SharedFoliageData foliageData = new();
	int chunkSize;
	int chunksVisibleInViewDst;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
	List<TerrainChunk> terrainChunksToCheck = new List<TerrainChunk>();

	void Start() {
		chunkSize = MapGenerater.Singleton.size;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
		
		foliageData.triangleCountNameID = Shader.PropertyToID(triangleCountName);
		foliageData.terrainMeshNameID = Shader.PropertyToID(terrainMeshName);
		foliageData.seedNameID = Shader.PropertyToID(seedName);

		UpdateVisibleChunks();
		InvokeRepeating("UpdateVisibleChunks", 0, chunkUpdateDelay);
	}
		
	void UpdateVisibleChunks() {
		
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);

		terrainChunksToCheck.Clear();
		for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
			terrainChunksVisibleLastUpdate [i].SetVisible (false);
			terrainChunksToCheck.Add(terrainChunksVisibleLastUpdate [i]);
		}
		terrainChunksVisibleLastUpdate.Clear ();
			
		int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / chunkSize);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
					terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();
					if (terrainChunkDictionary [viewedChunkCoord].IsVisible ()) {
						terrainChunksVisibleLastUpdate.Add (terrainChunkDictionary [viewedChunkCoord]);
					}
				} else {
					terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunkSize, transform, maxViewDst, foliageData, foliagePrefabs));
				}

				terrainChunksToCheck.Add(terrainChunkDictionary [viewedChunkCoord]);
			}
		}

		foreach (TerrainChunk chunk in terrainChunksToCheck)
		{
			if (chunk.IsVisible() != chunk.IsActive())
			{
				chunk.SetActive(chunk.IsVisible());
			}
		}
	}

	public class TerrainChunk {

		GameObject meshObject;
		SharedFoliageData sharedFoliageData;
		VisualEffect[] foliagePrefabs;
		MeshRenderer meshRenderer;
		MeshCollider meshCollider;
		MeshFilter meshFilter;
		Vector2 position;
		Bounds bounds;
		
		bool visability = false;

        float viewDst;
		public TerrainChunk(Vector2 coord, int size, Transform parent, float maxViewDst, SharedFoliageData sharedFoliageData, VisualEffect[] foliagePrefabs) {

            viewDst = maxViewDst;
			position = coord * size;
			bounds = new Bounds(position, Vector3.one * size);
			Vector3 positionV3 = new Vector3(position.x,0,position.y);

			meshObject = new GameObject("Terrain Chunk");
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshCollider = meshObject.AddComponent<MeshCollider>();
			meshRenderer.material = MapGenerater.Singleton.terrainMaterial;

			meshObject.transform.position = positionV3;
			meshObject.transform.parent = parent;

			this.sharedFoliageData = sharedFoliageData;
			this.foliagePrefabs = foliagePrefabs;

			MapGenerater.Singleton.GenorateMeshOnThread(position, OnMeshGenerated);
		}

		void OnMeshGenerated(MeshData meshData) {
			meshFilter.mesh = meshData.CreateMesh();
			meshCollider.sharedMesh = meshData.CreateColliderMesh();

			for (var i = 0; i < foliagePrefabs.Length; i++)
			{
				VisualEffect vfx = Instantiate(foliagePrefabs[i], meshObject.transform);

				vfx.SetMesh(sharedFoliageData.terrainMeshNameID, meshFilter.mesh);
				vfx.SetInt(sharedFoliageData.triangleCountNameID, meshFilter.mesh.triangles.Length - 1);

				System.Random random = new System.Random(meshObject.transform.position.GetHashCode());

				vfx.SetFloat(sharedFoliageData.seedNameID, (float)random.Next() / (float)int.MaxValue * (i * 1.2f));

				vfx.Play();
			}

			SetVisible(false);
		}

		public void UpdateTerrainChunk() {
			float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance (viewerPosition));
			bool visible = viewerDstFromNearestEdge <= viewDst;
			SetVisible (visible);
		}

		public void SetVisible(bool visible) {
			visability = visible;
		}

		public bool IsVisible() {
			return visability;
		}

		public bool IsActive() {
			return meshObject.activeSelf;
		}

		public void SetActive(bool active){
			meshObject.SetActive(active);
		}
	}
}