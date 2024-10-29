using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.PlayerLoop;

public class MapSystem : MonoBehaviour
{
    public static MapSystem Instance { get; private set; }
    public readonly float squareSize = 0.64f;
    public readonly float xStart = -6.4f;
    public readonly float yStart = -3.84f;
    public readonly float xEnd = 6.4f;
    public readonly float yEnd = 3.84f;
    public readonly int rows = 12;
    public readonly int columns = 20;
    public List<MapItemType>[,] mapItems;
    public Tree [,] trees;

    public GameObject hole;

    int? allSandCount = null;
    int? doneSandCount = null;
    // Start is called before the first frame update

    void Awake() {
        mapItems = new List<MapItemType>[rows, columns];
        trees = new Tree[rows, columns];
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    void Start() {
        alignItems();
        Initialization();
    }

    // Update is called once per frame
    void Update() {
        generateMap();
    }

    void Initialization() {
        foreach (Transform child in transform) {
            if (Enum.TryParse(child.tag, out MapItemType mapItemType)) {
                if (mapItemType == MapItemType.Sand) {
                    allSandCount = (allSandCount ?? 0) + 1;
                }
            }
        }
    }

    public void AddDoneSand() {
        doneSandCount = (doneSandCount ?? 0) + 1;
        if(doneSandCount == allSandCount) {
            hole.GetComponent<Hole>().OpenHole();
        }
    }

    public Vector2Int getXAndYFromPosition(Vector2 position) {
        return new Vector2Int(getXFromPosition(position.x), getYFromPosition(position.y));
    }

    public int getXFromPosition(float position) {
        return (int)Math.Floor((position - xStart) / squareSize);
    }

    public int getYFromPosition(float position) {
        return (int)Math.Floor((position - yStart) / squareSize);
    }

    public Vector2 getPositionFromXAndY(int x, int y) {
        return new Vector2(x * squareSize + xStart + squareSize / 2, y * squareSize + yStart + squareSize / 2);
    }


    //jesli elementy są ułożone nie równo na mapie to je wyrownuje do siatki
    private void alignItems() {
        foreach (Transform child in transform) {
            float x = getPositionFromXAndY(getXFromPosition(child.position.x), getYFromPosition(child.position.y)).x;
            float y = getPositionFromXAndY(getXFromPosition(child.position.x), getYFromPosition(child.position.y)).y;
            child.position = new Vector3(x, y, child.position.z);
        }
    }
    private void generateMap() {
        for (int y = 0; y < rows; y++) {
            for (int x = 0; x < columns; x++) {
                mapItems[y, x] = new List<MapItemType>();
                trees[y, x] = null;
            }
        }
        foreach (Transform child in transform) {
            int x = getXFromPosition(child.position.x); // Kolumna
            int y = getYFromPosition(child.position.y); // Wiersz
            if (Enum.TryParse(child.tag, out MapItemType mapItemType)) {
                if(child.tag == "Tree") {
                    Tree tree = child.GetComponent<Tree>();
                    TreeOrientationType orientation = tree.orientation;
                    if(orientation == TreeOrientationType.Horizontal) {
                        mapItems[y, x - 1].Add(MapItemType.Tree);
                        mapItems[y, x + 1].Add(MapItemType.Tree);
                        trees[y, x - 1] = tree;
                        trees[y, x + 1] = tree;
                    } else {
                        mapItems[y - 1, x].Add(MapItemType.Tree);
                        mapItems[y + 1, x].Add(MapItemType.Tree);
                        trees[y - 1, x] = tree;
                        trees[y + 1, x] = tree;
                    }
                    mapItems[y, x].Add(MapItemType.Wall);
                    continue;
                }
                mapItems[y, x].Add(mapItemType); // Używaj y dla wierszy, a x dla kolumn
            }
        }
    }
    
}
