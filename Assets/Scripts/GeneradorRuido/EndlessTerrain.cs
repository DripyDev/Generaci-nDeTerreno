using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {
    public const float maxViewDistance = 450f;
    public Transform viewer;
    public Material mapMaterial;

    static GeneradorMapa mapGenerator;
    public static Vector2 viewerPosition;
    int chunkSize;
    int chunksVisibles;

    Dictionary<Vector2, TerrainChunk> terrainChunckDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start() {
        mapGenerator = FindObjectOfType<GeneradorMapa>();
        chunkSize = GeneradorMapa.tamañoMapaChunk -1;
        chunksVisibles = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }

    void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks(){

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        //Recorremos los chunks adyaentes a nosotros
        for (int y = -chunksVisibles; y <= chunksVisibles; y++) {
            for (int x = -chunksVisibles; x < chunksVisibles; x++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + x, currentChunkCoordY + y);
                
                if(terrainChunckDictionary.ContainsKey(viewedChunkCoord)){
                    terrainChunckDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if(terrainChunckDictionary[viewedChunkCoord].IsVisible()){
                        terrainChunksVisibleLastUpdate.Add(terrainChunckDictionary[viewedChunkCoord]);
                    }
                }
                else{
                    terrainChunckDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, this.transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        MapData mapData;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        //Constructor
        public TerrainChunk(Vector2 coord, int size, Transform parent, Material mat){
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("TerrainChunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = mat;


            meshObject.transform.position = positionV3;
            //Dividimos entre 10 porque la escala de la primitiva de forma predefinida es 10
            //Ya no hace falta porque usamos el mesh y no un plano meshObject.transform.localScale = Vector3.one * size/10f;

            meshObject.transform.parent = parent;
            //Los chunks son inicializados en invisible
            SetVisible(false);

            mapGenerator.RequesMapData(OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData){
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

        void OnMeshDataReceived(MeshData meshdata){
            meshFilter.mesh = meshdata.CrearMesh();
        }

        //Comprobacion para ver si el chunk es visible o no
        public void UpdateTerrainChunk(){
            //Distancia entre el punto dado y esta bounding box
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible){
            meshObject.SetActive(visible);
        }
        public bool IsVisible(){
            return meshObject.activeSelf;
        }
    }
}
