using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;  // добавь этот using вверху!

public class MazeGenerator : MonoBehaviour
{
    [Header("Размер лабиринта")]
    public int width = 11;          // должно быть нечётным!
    public int height = 11;

    [Header("Размеры клетки")]
    public float cellSize = 4f;     // размер одной клетки в юнитах
    public float wallHeight = 3f;

    [Header("Префабы")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;

    [Header("Объекты сцены")]
    public GameObject player;
    public GameObject exit;
    public GameObject enemyPrefab;
    public int enemyCount = 1;

    // Сетка: true = стена, false = проход
    private bool[,] _grid;
    private NavMeshSurface _navMeshSurface;
    private List<GameObject> _spawnedObjects = new List<GameObject>(); // для очистки

    void Start()
    {
        // Сбрасываем шейдер в самом начале
        Shader.SetGlobalFloat("_PingRadius", -1f);
        Shader.SetGlobalFloat("_TrailRadius", -1f);
        Shader.SetGlobalFloat("_TrailIntensity", 0f);
    
        _navMeshSurface = GetComponent<NavMeshSurface>();
        Generate();
    }

    public void Generate()
    {
        foreach (var obj in _spawnedObjects)
            Destroy(obj);
        _spawnedObjects.Clear();

        _grid = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _grid[x, y] = true;

        RecursiveBacktrack(1, 1);
        BuildMaze();
        PlacePlayerAndExit();

        // Перепекаем NavMesh после того как стены построены
        _navMeshSurface.BuildNavMesh();

        SpawnEnemies();  // спавним врагов ПОСЛЕ навмеша!
    }

    // --- АЛГОРИТМ RECURSIVE BACKTRACKING ---
    void RecursiveBacktrack(int x, int y)
    {
        _grid[x, y] = false;    // отмечаем клетку как проход

        // Случайный порядок направлений
        int[] dirs = { 0, 1, 2, 3 };
        ShuffleArray(dirs);

        foreach (int dir in dirs)
        {
            // Шаг на 2 клетки (через стену)
            int nx = x + (dir == 1 ? 2 : dir == 3 ? -2 : 0);
            int ny = y + (dir == 0 ? 2 : dir == 2 ? -2 : 0);

            // Проверяем что новая клетка внутри сетки и ещё стена
            if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && _grid[nx, ny])
            {
                // Убираем стену между текущей и новой клеткой
                _grid[x + (nx - x) / 2, y + (ny - y) / 2] = false;
                RecursiveBacktrack(nx, ny);
            }
        }
    }

    // --- СТРОИМ ГЕОМЕТРИЮ ---
    void BuildMaze()
    {
        // Пол на весь лабиринт
        Vector3 floorPos = new Vector3(width * cellSize / 2f, 0f, height * cellSize / 2f);
        Vector3 floorScale = new Vector3(width * cellSize, 0.2f, height * cellSize);
        SpawnScaled(floorPrefab, floorPos, floorScale);

        // Стены по сетке
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!_grid[x, y]) continue;     // проход — пропускаем

                Vector3 pos = new Vector3(x * cellSize, wallHeight / 2f, y * cellSize);
                Vector3 scale = new Vector3(cellSize, wallHeight, cellSize);
                SpawnScaled(wallPrefab, pos, scale);
            }
        }
    }

    // --- РАССТАВЛЯЕМ ИГРОКА И ВЫХОД ---
    void PlacePlayerAndExit()
    {
        Vector2Int startCell = FindNearestOpen(1, 1);
        Vector2Int exitCell = FindFarthestOpen(startCell);
        exit.transform.position = GridToWorld(exitCell.x, exitCell.y);

        // Отключаем CharacterController на время телепортации
        CharacterController cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.transform.position = GridToWorld(startCell.x, startCell.y);
        cc.enabled = true;
    }

    void SpawnEnemies()
    {
        Vector2Int startCell = FindNearestOpen(1, 1);
        Vector2Int exitCell = FindFarthestOpen(startCell);
    
        List<Vector2Int> openCells = new List<Vector2Int>();
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
            {
                if (_grid[x, y]) continue;
    
                Vector2Int cell = new Vector2Int(x, y);
                if (Vector2Int.Distance(cell, startCell) < 6f) continue;
                if (cell == exitCell) continue;
    
                openCells.Add(cell);
            }
    
        for (int i = 0; i < enemyCount && openCells.Count > 0; i++)
        {
            // Спавним врага
            int idx = Random.Range(0, openCells.Count);
            Vector3 pos = GridToWorld(openCells[idx].x, openCells[idx].y);
            GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            _spawnedObjects.Add(enemy);
            openCells.RemoveAt(idx);
    
            // Генерируем 4 waypoint вокруг врага
            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            ai.waypoints = GenerateWaypoints(enemy.transform.position, 4);
        }
    }
    
    // Генерирует waypoints вокруг стартовой позиции врага
    Transform[] GenerateWaypoints(Vector3 enemyPos, int count)
    {
        List<Transform> points = new List<Transform>();
    
        // Собираем все открытые клетки в радиусе от врага
        List<Vector2Int> nearCells = new List<Vector2Int>();
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
            {
                if (_grid[x, y]) continue;
                Vector3 cellWorld = GridToWorld(x, y);
                if (Vector3.Distance(cellWorld, enemyPos) < cellSize * 4f)
                    nearCells.Add(new Vector2Int(x, y));
            }
    
        // Берём случайные клетки как waypoints
        ShuffleArray(nearCells);
        int waypointCount = Mathf.Min(count, nearCells.Count);
    
        for (int i = 0; i < waypointCount; i++)
        {
            GameObject wp = new GameObject("WP_Enemy_" + i);
            wp.transform.position = GridToWorld(nearCells[i].x, nearCells[i].y);
            _spawnedObjects.Add(wp);    // добавляем в список чтобы удалялись при регенерации
            points.Add(wp.transform);
        }
    
        return points.ToArray();
    }
    
    // Перегрузка ShuffleArray для List<Vector2Int>
    void ShuffleArray(List<Vector2Int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // Находит ближайший проход к заданной клетке
    Vector2Int FindNearestOpen(int targetX, int targetY)
    {
        Vector2Int best = new Vector2Int(1, 1);
        float bestDist = float.MaxValue;

        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
            {
                if (_grid[x, y]) continue;  // стена — пропускаем
                float dist = Vector2Int.Distance(new Vector2Int(x, y), new Vector2Int(targetX, targetY));
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = new Vector2Int(x, y);
                }
            }

        return best;
    }

    Vector2Int FindFarthestOpen(Vector2Int from)
    {
        Vector2Int best = from;
        float bestDist = 0f;

        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
            {
                if (_grid[x, y]) continue;
                float dist = Vector2Int.Distance(new Vector2Int(x, y), from);
                if (dist > bestDist)
                {
                    bestDist = dist;
                    best = new Vector2Int(x, y);
                }
            }

        return best;
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ ---
    Vector3 GridToWorld(int x, int y)
    {
        // Конвертируем координаты сетки в мировые, Y=1 чтобы стоять на полу
        return new Vector3(x * cellSize, 1f, y * cellSize);
    }

    GameObject SpawnScaled(GameObject prefab, Vector3 pos, Vector3 scale)
    {
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        obj.transform.localScale = scale;
        _spawnedObjects.Add(obj);
        return obj;
    }

    void ShuffleArray(int[] arr)
    {
        // Fisher-Yates shuffle — честный случайный порядок
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
}