using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {
    const float scale = 2f;

    //Distancia a partir de la cual el chunk va a actualizarse
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    public static float maxViewDistance;
    public LODInfo[] detailLevels;
    public Transform viewer;
    public Material mapMaterial;

    static GeneradorMapa mapGenerator;
    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    int chunkSize;
    int chunksVisibles;

    Dictionary<Vector2, TerrainChunk> terrainChunckDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start() {
        mapGenerator = FindObjectOfType<GeneradorMapa>();

        maxViewDistance = detailLevels[detailLevels.Length-1].visibleDistanceThreshold;
        chunkSize = GeneradorMapa.tamañoMapaChunk -1;
        chunksVisibles = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;
        //Solo actualizamos los chunks a una distancia umbral de nosotros
        if( (viewerPositionOld-viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate ){
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
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
                }
                else{
                    terrainChunckDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, this.transform, mapMaterial));
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

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;

        MapData mapData1;
        bool mapDataReceived;
        int previousLODIndex = -1;

        //Constructor
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material mat){
            this.detailLevels = detailLevels;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("TerrainChunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = mat;


            meshObject.transform.position = positionV3 * scale;
            //Dividimos entre 10 porque la escala de la primitiva de forma predefinida es 10
            //Ya no hace falta porque usamos el mesh y no un plano meshObject.transform.localScale = Vector3.one * size/10f;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            //Los chunks son inicializados en invisible
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++){
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequesMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData){
            this.mapData = mapData;
            this.mapDataReceived = true;

            Texture2D texture = GeneradorTextura.TextureFromColourMap(mapData.mapaColores, GeneradorMapa.tamañoMapaChunk, GeneradorMapa.tamañoMapaChunk);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        /*Ya no es necesario void OnMeshDataReceived(MeshData meshdata){
            meshFilter.mesh = meshdata.CrearMesh();
        }*/

        //Comprobacion para ver si el chunk es visible o no
        public void UpdateTerrainChunk(){
            //Solo hace falta si recivimos el mapData
            if(mapDataReceived){
                //Distancia entre el punto dado y esta bounding box
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= maxViewDistance;

                if(visible){
                    int lodIndex = 0;
                    //Conseguimos el nivel de detalle de este chunk
                    for (int i = 0; i < detailLevels.Length-1; i++){
                        if(viewerDstFromNearestEdge > detailLevels[i].visibleDistanceThreshold){
                            lodIndex = i+1;
                        }
                        else{
                            break;
                        }
                    }
                    if(lodIndex != previousLODIndex){
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if(lodMesh.hasMesh){
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh){
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                    //Si el chunk es visible, lo metemos en la lista de chunks visibles que recorremos para ver cuales hay que dejar de renderizar
                    terrainChunksVisibleLastUpdate.Add(this);
                }
                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible){
            meshObject.SetActive(visible);
        }
        public bool IsVisible(){
            return meshObject.activeSelf;
        }
    }

    class LODMesh{
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback){
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshdata){
            mesh = meshdata.CrearMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData){
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo{
        public int lod;
        public float visibleDistanceThreshold;
    }
}
