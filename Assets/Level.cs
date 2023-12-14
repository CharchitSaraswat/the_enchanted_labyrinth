using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Assertions;

enum TileType
{
    WALL = 0,
    FLOOR = 1,
    WATER = 2,
    DRUG = 3,
    VIRUS = 4,
}

public class ObjectData
{
    public string ObjectType { get; private set; }
    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set; }
    public Vector3 Scale { get; private set; }

    public ObjectData(string objectType, Vector3 position, Quaternion? rotation = null, Vector3? scale = null)
    {
        ObjectType = objectType;
        Position = position;
        Rotation = rotation ?? Quaternion.identity;
        Scale = scale ?? Vector3.one;
    }
}


public class Level : MonoBehaviour
{
    // fields/variables you may adjust from Unity's interface
    public int width = 16;   // size of level (default 16 x 16 blocks)
    public int length = 16;
    public float storey_height = 2.5f;   // height of walls
    public float virus_speed = 3.0f;     // virus velocity
    public GameObject fps_prefab;        // these should be set to prefabs as provided in the starter scene
    public GameObject virus_prefab;
    public GameObject water_prefab;
    public GameObject house_prefab;
    public GameObject gem_prefab;
    public Text displayEquation;
    public GameObject text_box;
    // public GameObject scroll_bar;

    private int coefficient_of_gems; //a
private int number_of_gems; //x
private int coefficient_of_dragons; //b
private int number_of_dragons; //y
private int result;
private int numberOfGems;
private    int numberOfDragons;
    

    // fields/variables accessible from other scripts
    internal GameObject fps_player_obj;   // instance of FPS template
    internal float player_health = 1.0f;  // player health in range [0.0, 1.0]
    internal int num_virus_hit_concurrently = 0;            // how many viruses hit the player before washing them off
    internal bool virus_landed_on_player_recently = false;  // has virus hit the player? if yes, a timer of 5sec starts before infection
    internal float timestamp_virus_landed = float.MaxValue; // timestamp to check how many sec passed since the virus landed on player
    internal bool drug_landed_on_player_recently = false;   // has drug collided with player?
    internal bool player_is_on_water = false;               // is player on water block
    internal bool player_entered_house = false;             // has player arrived in house?

    // fields/variables needed only from this script
    private Bounds bounds;                   // size of ground plane in world space coordinates 
    private float timestamp_last_msg = 0.0f; // timestamp used to record when last message on GUI happened (after 7 sec, default msg appears)
    private int function_calls = 0;          // number of function calls during backtracking for solving the CSP
    private int num_viruses = 0;             // number of viruses in the level
    private List<int[]> pos_viruses;         // stores their location in the grid

    // private HashSet<string> memoizationCache = new HashSet<string>();

    public Canvas play_again_canvas;
    public Canvas try_again_canvas;
    public Canvas solve_canvas;

    public Canvas main_canvas;
    public List<GameObject> createdGameObjs = new List<GameObject>();

    public List<ObjectData> objDetails = new List<ObjectData>();

    private AudioSource source;
    public AudioClip virus_sound;
    public AudioClip player_health_gone_sound;
    public AudioClip water_sound;

    public AudioClip drug_sound;

    public AudioClip house_reached_sound;

    public int ExitSoundPlayed = 0;

    public Material grassMaterial;

    public float virusMaxHeight = 0.0f;
    // public InputField answerInput;

    public Text successText;

    private int correctAnswer = 4;


    // feel free to put more fields here, if you need them e.g, add AudioClips that you can also reference them from other scripts
    // for sound, make also sure that you have ONE audio listener active (either the listener in the FPS or the main camera, switch accordingly)

    // a helper function that randomly shuffles the elements of a list (useful to randomize the solution to the CSP)

// private string LoadTextFromFile(string filePath)
// {
//     if (File.Exists(filePath))
//     {
//         return File.ReadAllText(filePath);
//     }
//     else
//     {
//         Debug.LogError("Cannot find file at " + filePath);
//         return "";
//     }
// }


private void GenerateAndDisplayEquation()
{
    // Set the range for the random numbers
    int min = 1; // Example minimum value
    int max = 10; // Example maximum value
    numberOfGems = GetNumberOfGems();
    numberOfDragons = GetNumberOfDragons();
    // Assign random numbers to the variables
    coefficient_of_gems = Random.Range(min, max + 1);
    number_of_gems = Random.Range(min, numberOfGems+1); //change
    coefficient_of_dragons = Random.Range(min, max + 1);
    number_of_dragons = Random.Range(min, numberOfDragons); //change
     Debug.Log("coefficient_of_gems " + coefficient_of_gems);
    Debug.Log("x " + number_of_gems );
    Debug.Log("coefficient_of_dragons" + coefficient_of_dragons);
    Debug.Log("y " + number_of_dragons);
    // Debug.Log("Number of Gems: " + numberOfGems);
    // Debug.Log("Number of Dragons: " + numberOfDragons);

    // Calculate the result of the equation
    result = coefficient_of_gems * number_of_gems + coefficient_of_dragons * number_of_dragons;
    Assert.AreEqual(result, coefficient_of_gems * number_of_gems + coefficient_of_dragons * number_of_dragons, "The result does not match the expected value of a * x + b * y");
    // Create the equation string
    string equation = $"{coefficient_of_gems}x + {coefficient_of_dragons}y = {result}";

    // Display the equation on the screen
    if(displayEquation != null)
    {
        displayEquation.text = equation;
    }
    else
    {
        Debug.LogError("Equation Text not assigned in the Inspector");
    }
}

    private void Shuffle<T>(ref List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        play_again_canvas.enabled = false;
        try_again_canvas.enabled = false;
        solve_canvas.enabled = false;
        main_canvas.enabled = true;
        // InitializeLevel("start");
    }

    public void StartGame(){
        main_canvas.enabled = false;
        InitializeLevel("start");
    }


public int GetNumberOfGems()
{
    
    return objDetails.Count(obj => obj.ObjectType == "Capsule");
}

public int GetNumberOfDragons()
{
    return objDetails.Count(obj => obj.ObjectType == "Virus"); 
}

    public void InitializeLevel(string processType){
        // initialize internal/private variables
        //  scoresFilePath = Path.Combine("/Users/somyaaaggarwal/Documents/Scores", scoresFileName);
    //     string textToShow = LoadTextFromFile("/Users/somyaaaggarwal/Downloads/Equations.txt");
    // displayEquation.text = textToShow;

     
        virus_landed_on_player_recently = false;
        timestamp_virus_landed = float.MaxValue;
        drug_landed_on_player_recently = false;
        player_is_on_water = false;
        player_entered_house = false;
        player_health = 1.0f;

        foreach (GameObject obj in createdGameObjs)
            if (obj != null){
                Destroy(obj.gameObject);
                Destroy(obj);
            }
        if (processType == "tryAgain"){
            // spawn objs in ObjDetails
            foreach (ObjectData objData in objDetails)
            {
                GameObject obj = null;
                switch (objData.ObjectType)
                {
                    case "Virus":
                        obj = Instantiate(virus_prefab, objData.Position, objData.Rotation);
                        obj.name = "COVID";
                        obj.AddComponent<Virus>();
                        obj.GetComponent<Rigidbody>().mass = 10000;
                        createdGameObjs.Add(obj);
                        break;
                    case "Water":
                        obj = Instantiate(water_prefab, objData.Position, objData.Rotation);
                        obj.name = "WATER";
                        obj.transform.localScale = objData.Scale;
                        obj.transform.position = objData.Position;
                        createdGameObjs.Add(obj);
                        break;
                    case "House":
                        obj = Instantiate(house_prefab, objData.Position, objData.Rotation);
                        obj.name = "HOUSE";

                        obj.AddComponent<BoxCollider>();
                        obj.GetComponent<BoxCollider>().isTrigger = true;
                        obj.GetComponent<BoxCollider>().size = new Vector3(3.0f, 3.0f, 3.0f);
                        obj.AddComponent<House>();
                        createdGameObjs.Add(obj);
                        break;
                    case "FPSPlayer":
                        // fps_player_obj = Instantiate(fps_prefab);
                        fps_player_obj = fps_prefab;
                        fps_player_obj.name = "PLAYER";
                        // character is placed above the level so that in the beginning, he appears to fall down onto the maze
                        fps_player_obj.transform.position = objData.Position;
                        createdGameObjs.Add(fps_player_obj);
                        break;
                    case "Capsule":
                        obj = Instantiate(gem_prefab, objData.Position, objData.Rotation);
                        obj.name = "DRUG";
                        obj.transform.localScale = objData.Scale;
                        obj.transform.position = objData.Position;
                        obj.GetComponent<Renderer>().material.color = Color.green;
                        obj.AddComponent<Drug>();
                        createdGameObjs.Add(obj);
                        break;
                    case "WaterBox":
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        obj.name = "WATER_BOX";
                        obj.transform.localScale = objData.Scale;
                        obj.transform.position = objData.Position;
                        obj.GetComponent<Renderer>().material.color = Color.grey;
                        obj.GetComponent<BoxCollider>().size = new Vector3(1.1f, 20.0f * storey_height, 1.1f);
                        obj.GetComponent<BoxCollider>().isTrigger = true;
                        obj.AddComponent<Water>();
                        createdGameObjs.Add(obj);
                        break;
                    case "Wall":
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        obj.name = "WALL";
                        obj.transform.localScale = objData.Scale;
                        obj.transform.position = objData.Position;
                        // obj.GetComponent<Renderer>().material.color = Color.red;
                        obj.GetComponent<Renderer>().material = grassMaterial;
                        createdGameObjs.Add(obj);
                        break;
                    default:
                        break;
                }
            }
            UpdateVirusReferencesToPlayer();
        } else {
        bounds = GetComponent<Collider>().bounds; 
        timestamp_last_msg = 0.0f;
        objDetails.Clear();
        function_calls = 0;
        num_viruses = 0;
        player_health = 1.0f;
        num_virus_hit_concurrently = 0;
        source = gameObject.GetComponent<AudioSource>();
        virus_sound = Resources.Load<AudioClip>("VirusSound");
        player_health_gone_sound = Resources.Load<AudioClip>("HealthSound");
        water_sound = Resources.Load<AudioClip>("WaterSound");
        drug_sound = Resources.Load<AudioClip>("DrugSound");
        house_reached_sound = Resources.Load<AudioClip>("EndSound");
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }
        // initialize 2D grid
        List<TileType>[,] grid = new List<TileType>[width, length];
        // useful to keep variables that are unassigned so far
        List<int[]> unassigned = new List<int[]>();

        // will place x viruses in the beginning (at least 1). x depends on the sise of the grid (the bigger, the more viruses)        
        num_viruses = width * length / 25 + 1; // at least one virus will be added
        pos_viruses = new List<int[]>();
        // create the wall perimeter of the level, and let the interior as unassigned
        // then try to assign variables to satisfy all constraints
        // *rarely* it might be impossible to satisfy all constraints due to initialization
        // in this case of no success, we'll restart the random initialization and try to re-solve the CSP
        bool success = false;
        while (!success) // keep trying until we find a solution
        {
            for (int v = 0; v < num_viruses; v++)
            {
                while (true) // try until virus placement is successful (unlikely that there will no places)
                {
                    // try a random location in the grid
                    int wr = Random.Range(1, width - 1);
                    int lr = Random.Range(1, length - 1);

                    // if grid location is empty/free, place it there
                    if (grid[wr, lr] == null)
                    {
                        grid[wr, lr] = new List<TileType> { TileType.VIRUS };
                        pos_viruses.Add(new int[2] { wr, lr });
                        break;
                    }
                }
            }

            for (int w = 0; w < width; w++)
                for (int l = 0; l < length; l++)
                    if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                        grid[w, l] = new List<TileType> { TileType.WALL };
                    else
                    {
                        if (grid[w, l] == null) // does not have virus already or some other assignment from previous run
                        {
                            // CSP will involve assigning variables to one of the following four values (VIRUS is predefined for some tiles
                            List<TileType> candidate_assignments = new List<TileType> { TileType.WALL, TileType.FLOOR, TileType.WATER, TileType.DRUG };
                            Shuffle<TileType>(ref candidate_assignments);
                            grid[w, l] = candidate_assignments;
                            unassigned.Add(new int[] { w, l });
                        }
                    }

            // YOU MUST IMPLEMENT this function!!!
            Shuffle<int[]>(ref unassigned);
            success = BackTrackingSearch(grid, unassigned);
            if (!success)
            {
                unassigned.Clear();
                grid = new List<TileType>[width, length];
                function_calls = 0; 
            }
        }
        DrawDungeon(grid);
   GenerateAndDisplayEquation();
    numberOfGems = GetNumberOfGems();
    numberOfDragons = GetNumberOfDragons();
    Debug.Log("Number of Gems after initialize " + numberOfGems);
    Debug.Log("Number of Dragons after initialize " + numberOfDragons);
        }
    }

    // one type of constraint already implemented for you
    bool DoWeHaveTooManyInteriorWallsORWaterORDrug(List<TileType>[,] grid)
    {
        int[] number_of_assigned_elements = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                if (grid[w, l].Count == 1)
                    number_of_assigned_elements[(int)grid[w, l][0]]++;
            }

        if ((number_of_assigned_elements[(int)TileType.WALL] > num_viruses * 10) ||
             (number_of_assigned_elements[(int)TileType.WATER] > (width + length) / 4) ||
             (number_of_assigned_elements[(int)TileType.DRUG] >= num_viruses / 2))
            return true;
        else
            return false;
    }

    // another type of constraint already implemented for you
    bool DoWeHaveTooFewWallsORWaterORDrug(List<TileType>[,] grid)
    {
        int[] number_of_potential_assignments = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                for (int i = 0; i < grid[w, l].Count; i++)
                    number_of_potential_assignments[(int)grid[w, l][i]]++;
            }

        if ((number_of_potential_assignments[(int)TileType.WALL] < (width * length) / 4) ||
             (number_of_potential_assignments[(int)TileType.WATER] < num_viruses / 4) ||
             (number_of_potential_assignments[(int)TileType.DRUG] < num_viruses / 4))
            return true;
        else
            return false;
    }

    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // must return true if there are three (or more) interior consecutive wall blocks either horizontally or vertically
    // by interior, we mean walls that do not belong to the perimeter of the grid
    // e.g., a grid configuration: "FLOOR - WALL - WALL - WALL - FLOOR" is not valid
    // bool TooLongWall(List<TileType>[,] grid)
    // {
    //     /*** implement the rest ! */
    //     return false;
    // }

    bool TooLongWall(List<TileType>[,] grid)
    {
        for (int x = 1; x < width - 1; x++) // Skip exterior walls
        {
            for (int y = 1; y < length - 1; y++) // Skip exterior walls
            {
                bool isSingleWall = grid[x, y].Count == 1 && grid[x, y].Contains(TileType.WALL);
                // Check horizontally and vertically for three consecutive walls
                if (x+1 < width && isSingleWall && grid[x + 1, y].Count == 1 && grid[x + 1, y].Contains(TileType.WALL) && 
                        x+2 < width && grid[x + 2, y].Count == 1 && grid[x + 2, y].Contains(TileType.WALL))
                {
                    return true; // Found three consecutive interior walls
                }

                if (y+1 < length && isSingleWall && grid[x, y + 1].Count == 1 && grid[x, y + 1].Contains(TileType.WALL) && 
                        y+2 < length && grid[x, y + 2].Count == 1 && grid[x, y + 2].Contains(TileType.WALL))
                {
                    return true; // Found three consecutive interior walls
                }
            }
        }
        return false; // No three consecutive walls found
    }

    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // must return true if there is no WALL adjacent to a virus 
    // adjacency means left, right, top, bottom, and *diagonal* blocks

    // bool NoWallsCloseToVirus(List<TileType>[,] grid)
    // {
    //     /*** implement the rest ! */
    //     return false;
    // }

    bool NoWallsCloseToVirus(List<TileType>[,] grid)
    {
        /*** implement the rest ! */
        foreach (var pos in pos_viruses)
        {
            int x = pos[0];
            int y = pos[1];
            bool wallNearby = false;

            // Check all adjacent positions including diagonals
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue; // Skip the virus position itself

                    int checkX = x + i;
                    int checkY = y + j;

                    // // Check bounds
                    if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < length)
                    {
                        if (grid[checkX, checkY].Contains(TileType.WALL) || grid[checkX, checkY].Count == 0)
                        {
                            wallNearby = true;
                            break;
                        }
                    }
                }    
                if (wallNearby) break;
            }

            if (!wallNearby) return true; // No wall found near this virus
        }
        return false; // Walls are found near all viruses
    }


    // check if attempted assignment is consistent with the constraints or not
    bool CheckConsistency(List<TileType>[,] grid, int[] cell_pos, TileType t)
    {
        int w = cell_pos[0];
        int l = cell_pos[1];

        List<TileType> old_assignment = new List<TileType>(grid[w, l]);
        old_assignment.AddRange(grid[w, l]);
        grid[w, l] = new List<TileType> { t };

		// note that we negate the functions here i.e., check if we are consistent with the constraints we want
        bool areWeConsistent = !DoWeHaveTooFewWallsORWaterORDrug(grid) && !DoWeHaveTooManyInteriorWallsORWaterORDrug(grid) && !TooLongWall(grid) && !NoWallsCloseToVirus(grid);

        grid[w, l] = new List<TileType>();
        grid[w, l].AddRange(old_assignment);
        return areWeConsistent;
    }


    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // implement backtracking 
    bool BackTrackingSearch(List<TileType>[,] grid, List<int[]> unassigned)
    {
        // if there are too many recursive function evaluations, then backtracking has become too slow (or constraints cannot be satisfied)
        // to provide a reasonable amount of time to start the level, we put a limit on the total number of recursive calls
        // if the number of calls exceed the limit, then it's better to try a different initialization
        function_calls += 1;
        if (function_calls > 100000)       
            return false;

        // we are done!
        if (unassigned.Count == 0)
            return true;
           //     // Get the next unassigned cell
        int[] cell = unassigned[0];
        unassigned.RemoveAt(0); // Remove this cell from the list of unassigned cells
        /*** implement the rest ! */
        List<TileType> candidateAssignments = grid[cell[0],cell[1]];
        Shuffle<TileType>(ref candidateAssignments);
        List<TileType> old_assignment = new List<TileType>(candidateAssignments);

        // foreach (TileType tile in System.Enum.GetValues(typeof(TileType)))
        while (candidateAssignments.Count > 0)
        {
            int randomIndex = Random.Range(0, candidateAssignments.Count);
            TileType tile = candidateAssignments[randomIndex];
            candidateAssignments.RemoveAt(randomIndex);

            if (CheckConsistency(grid, cell, tile))
            {
                grid[cell[0], cell[1]] = new List<TileType> { tile }; // Assign the tile type

                if (BackTrackingSearch(grid, new List<int[]>(unassigned))){ // Recurse with remaining unassigned cells
                    return true;
                }

                grid[cell[0], cell[1]] = old_assignment; // Backtrack: unassign the cell
            }
        }
        unassigned.Insert(0, cell); 
        return false;
    }

    public struct Vector2Int
    {
        public int xCoordinate, yCoordinate;

        public Vector2Int(int xCoordinate, int yCoordinate)
        {
            this.xCoordinate = xCoordinate;
            this.yCoordinate = yCoordinate;
        }

        public static bool operator ==(Vector2Int vec1, Vector2Int vec2)
        {
            return vec1.xCoordinate == vec2.xCoordinate && vec1.yCoordinate == vec2.yCoordinate;
        }

        public static bool operator !=(Vector2Int vec1, Vector2Int vec2)
        {
            return !(vec1 == vec2);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2Int)) return false;
            Vector2Int vec = (Vector2Int)obj;
            return xCoordinate == vec.xCoordinate && yCoordinate == vec.yCoordinate;
        }

        public override int GetHashCode()
        {
            return xCoordinate.GetHashCode() ^ yCoordinate.GetHashCode();
        }
    }

    private IEnumerable<Vector2Int> fetch_neighs(Vector2Int position, List<TileType>[,] grid)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Define possible moves (up, down, left, right)
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1), // Up
            new Vector2Int(0, -1), // Down
            new Vector2Int(-1, 0), // Left
            new Vector2Int(1, 0), // Right
            new Vector2Int(-1, 1), // Up-Left
            new Vector2Int(1, 1), // Up-Right
            new Vector2Int(-1, -1), // Down-Left
            new Vector2Int(1, -1), // Down-Right
        };

        foreach (var move in directions)
        {
            Vector2Int neigh_node = new Vector2Int(position.xCoordinate + move.xCoordinate, position.yCoordinate + move.yCoordinate);

            // Check if the neighbor is within grid bounds
            if (neigh_node.xCoordinate >= 0 && neigh_node.xCoordinate < grid.GetLength(0) && 
                neigh_node.yCoordinate >= 0 && neigh_node.yCoordinate < grid.GetLength(1))
            {
                neighbors.Add(neigh_node);
            }
        }

        return neighbors;
    }

    private float euc_dist(Vector2Int node1, Vector2Int node2)
    {
        return Mathf.Sqrt(Mathf.Pow(node2.xCoordinate - node1.xCoordinate, 2) + Mathf.Pow(node2.yCoordinate - node1.yCoordinate, 2));
    }


    bool implAStarAlgo(Vector2Int start, Vector2Int target, List<TileType>[,] grid)
    {
        // Initialize heap and visited lists
        List<Vector2Int> heap = new List<Vector2Int>();
        List<Vector2Int> visited = new List<Vector2Int>();

        // Add the start node to the heap
        heap.Add(start);

        // Init score maps
        Dictionary<Vector2Int, float> graph_scores = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> heuristic_scores = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> final_scores = new Dictionary<Vector2Int, float>();

        graph_scores[start] = 0;
        heuristic_scores[start] = euc_dist(start, target);
        final_scores[start] = graph_scores[start] + heuristic_scores[start];

        while (heap.Count > 0)
        {
            // Get the node in the heap with the lowest F score
            Vector2Int cur_node = heap.OrderBy(n => final_scores.ContainsKey(n) ? final_scores[n] : float.MaxValue).First();

            if (cur_node == target) // Path  found
                return true;

            heap.Remove(cur_node);
            visited.Add(cur_node);

            foreach (Vector2Int neigh_node in fetch_neighs(cur_node, grid))
            {   
                if (visited.Contains(neigh_node))
                    continue; // the neigh_node is already evaluated

                if (grid[neigh_node.xCoordinate, neigh_node.yCoordinate].Contains(TileType.WALL))
                    continue; // Do not consider walls

                // Additional checks for diagonally blocked paths
                Vector2Int direction = new Vector2Int(neigh_node.xCoordinate - cur_node.xCoordinate, neigh_node.yCoordinate - cur_node.yCoordinate);
                if (Mathf.Abs(direction.xCoordinate) == 1 && Mathf.Abs(direction.yCoordinate) == 1)
                {
                    Vector2Int side1 = new Vector2Int(cur_node.xCoordinate + direction.xCoordinate, cur_node.yCoordinate);
                    Vector2Int side2 = new Vector2Int(cur_node.xCoordinate, cur_node.yCoordinate + direction.yCoordinate);

                    if (grid[side1.xCoordinate, side1.yCoordinate].Contains(TileType.WALL) && 
                        grid[side2.xCoordinate, side2.yCoordinate].Contains(TileType.WALL))
                    {
                        continue; // Skip this neigh_node as it's diagonally blocked by wall corners
                    }
                }

                float cur_g_score = graph_scores[cur_node] + euc_dist(cur_node, neigh_node);

                if (!heap.Contains(neigh_node)) // move to new node
                    heap.Add(neigh_node);
                else if (cur_g_score >= graph_scores[neigh_node])
                    continue; // Not good path

                // save best path
                graph_scores[neigh_node] = cur_g_score;
                heuristic_scores[neigh_node] = euc_dist(neigh_node, target);
                final_scores[neigh_node] = graph_scores[neigh_node] + heuristic_scores[neigh_node];
            }
        }

        return false; // Failed to find a path
    }

    List<Vector2Int> fetchRemovableWalls(Vector2Int start, Vector2Int target, List<TileType>[,] grid)
    {
        var path = implAStarWithWallCosts(start, target, grid);

        // List to store the positions of walls that should be removed
        List<Vector2Int> wallsToRemove = new List<Vector2Int>();

        for (int i = 1; i < path.Count; i++)
        {
            Vector2Int cur_node = path[i - 1];
            Vector2Int next_node = path[i];

            // Check for a diagonal move
            if (Mathf.Abs(cur_node.xCoordinate - next_node.xCoordinate) == 1 && Mathf.Abs(cur_node.yCoordinate - next_node.yCoordinate) == 1)
            {
                Vector2Int side1 = new Vector2Int(cur_node.xCoordinate, next_node.yCoordinate);
                Vector2Int side2 = new Vector2Int(next_node.xCoordinate, cur_node.yCoordinate);

                // Check if either side1 or side2 is a wall and add it to the walls to remove
                float rand_num = Random.Range(0.0f, 1.0f);
                if (grid[side1.xCoordinate, side1.yCoordinate].Contains(TileType.WALL) && grid[side2.xCoordinate, side2.yCoordinate].Contains(TileType.WALL)){
                    if (rand_num <= 0.5f){
                        wallsToRemove.Add(side1);
                    }
                    else{
                        wallsToRemove.Add(side2);
                    }
                }
                
            }

            if (grid[cur_node.xCoordinate, cur_node.yCoordinate].Contains(TileType.WALL)){
                wallsToRemove.Add(cur_node);
            }
        }

        // Additional check for a wall directly in front of the target point
        if (path.Count > 1)
        {
            Vector2Int lastStep = path[path.Count - 2];
            Vector2Int directionToEnd = new Vector2Int(target.xCoordinate - lastStep.xCoordinate, target.yCoordinate - lastStep.yCoordinate);
            Vector2Int frontOfEnd = new Vector2Int(target.xCoordinate + directionToEnd.xCoordinate, target.yCoordinate + directionToEnd.yCoordinate);

            if (frontOfEnd.xCoordinate >= 0 && frontOfEnd.xCoordinate < grid.GetLength(0) && 
                frontOfEnd.yCoordinate >= 0 && frontOfEnd.yCoordinate < grid.GetLength(1) && 
                grid[frontOfEnd.xCoordinate, frontOfEnd.yCoordinate].Contains(TileType.WALL))
            {
                wallsToRemove.Add(frontOfEnd);
            }
        }

        Vector2Int lastPoint = path[path.Count - 1];
        if (grid[lastPoint.xCoordinate, lastPoint.yCoordinate].Contains(TileType.WALL))
        {
            wallsToRemove.Add(lastPoint);
        }

        return wallsToRemove.Distinct().ToList();
    }

    public class AStarNode : System.IComparable<AStarNode>
    {
        public Vector2Int Position;
        public float GraphCost;
        public float HeuristicCost;
        public AStarNode Parent;

        public float FCost { get { return GraphCost + HeuristicCost; } }

        public AStarNode(Vector2Int position, AStarNode parent, float graph_cost, float heur_cost)
        {
            Position = position;
            Parent = parent;
            GraphCost = graph_cost;
            HeuristicCost = heur_cost;
        }

        public int CompareTo(AStarNode other)
        {
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0)
            {
                compare = HeuristicCost.CompareTo(other.HeuristicCost); // Tie-breaker using HCost
            }
            return compare;
        }
    }

    private List<Vector2Int> recon_path(Dictionary<Vector2Int, AStarNode> from, AStarNode cur_pt)
    {
        List<Vector2Int> complete_path = new List<Vector2Int> { cur_pt.Position };
        while (from.ContainsKey(cur_pt.Position))
        {
            cur_pt = from[cur_pt.Position];
            complete_path.Add(cur_pt.Position);
        }
        complete_path.Reverse();
        return complete_path;
    }

    public class PriorityQueue<T> where T : System.IComparable<T>
    {
        private List<T> _heapElems;
        public int Count => _heapElems.Count;
        private IComparer<T> _compFunc;

        public PriorityQueue()
        {
            _heapElems = new List<T>();
            _compFunc = Comparer<T>.Default;
        }

        public PriorityQueue(System.Func<T, T, int> compareFunction)
        {
            _heapElems = new List<T>();
            _compFunc = Comparer<T>.Create((x, y) => compareFunction(x, y));
        }

        public void Enqueue(T item)
        {
            _heapElems.Add(item);
            _heapElems.Sort(_compFunc);
        }

        public T Dequeue()
        {
            if (_heapElems.Count == 0) 
                throw new System.InvalidOperationException("No element in Heap");
            T item = _heapElems[0];
            _heapElems.RemoveAt(0);
            return item;
        }
    }

    private int travel_cost(Vector2Int from, Vector2Int to, List<TileType>[,] grid)
    {
        int walkThroughCost = 10;
        int normalWalkCost = 1;

        return grid[to.xCoordinate, to.yCoordinate].Contains(TileType.WALL) ? walkThroughCost : normalWalkCost;
    }

    List<Vector2Int> implAStarWithWallCosts(Vector2Int start, Vector2Int end, List<TileType>[,] grid)
    {
        var heap = new PriorityQueue<AStarNode>();
        var visitedPos = new HashSet<Vector2Int>(); // To keep track of positions in heap
        var start_node = new AStarNode(start, null, 0, euc_dist(start, end));

        heap.Enqueue(start_node);
        visitedPos.Add(start_node.Position); // Add to HashSet

        var from = new Dictionary<Vector2Int, AStarNode>();
        var graph_scores = new Dictionary<Vector2Int, int>();
        graph_scores[start] = 0;

        while (heap.Count > 0)
        {
            var cur_node = heap.Dequeue();
            visitedPos.Remove(cur_node.Position); // Remove from HashSet

            if (cur_node.Position == end)
                return recon_path(from, cur_node);

            foreach (var neigh in fetch_neighs(cur_node.Position, grid))
            {
                int cur_g_score = graph_scores[cur_node.Position] + travel_cost(cur_node.Position, neigh, grid);

                if (!graph_scores.ContainsKey(neigh) || cur_g_score < graph_scores[neigh])
                {
                    graph_scores[neigh] = cur_g_score;
                    float heuristic_scores = euc_dist(neigh, end);
                    var neighNode = new AStarNode(neigh, cur_node, cur_g_score, heuristic_scores);

                    if (!visitedPos.Contains(neighNode.Position)) // Check in HashSet
                    {
                        heap.Enqueue(neighNode);
                        visitedPos.Add(neighNode.Position);
                        from[neigh] = cur_node;
                    }
                }
            }
        }

        return new List<Vector2Int>(); // No path found
    }

    void DrawDungeon(List<TileType>[,] solution)
    {
        GetComponent<Renderer>().material.color = Color.grey; // ground plane will be grey

        // place character at random position (wr, lr) in terms of grid coordinates (integers)
        // make sure that this random position is a FLOOR tile (not wall, drug, or virus)
        int wr = 0;
        int lr = 0;
        while (true) // try until a valid position is sampled
        {
            wr = Random.Range(1, width - 1);
            lr = Random.Range(1, length - 1);

            if (solution[wr, lr][0] == TileType.FLOOR)
            {
                float x = bounds.min[0] + (float)wr * (bounds.size[0] / (float)width);
                float z = bounds.min[2] + (float)lr * (bounds.size[2] / (float)length);
                // fps_player_obj = Instantiate(fps_prefab);
                fps_player_obj = fps_prefab;
                fps_player_obj.name = "PLAYER";
                // character is placed above the level so that in the beginning, he appears to fall down onto the maze
                fps_player_obj.transform.position = new Vector3(x + 0.5f, 0, z + 0.5f);
                // Print the player position
                //Debug.Log("Player position: " + fps_player_obj.transform.position);
                fps_player_obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                createdGameObjs.Add(fps_player_obj);
                ObjectData fpsPlayerData = new ObjectData("FPSPlayer", fps_player_obj.transform.position, fps_player_obj.transform.rotation, fps_player_obj.transform.localScale);
                objDetails.Add(fpsPlayerData); 
                break;
            }
        }

        // place an exit from the maze at location (wee, lee) in terms of grid coordinates (integers)
        // destroy the wall segment there - the grid will be used to place a house
        // the exist will be placed as far as away from the character (yet, with some randomness, so that it's not always located at the corners)
        int max_dist = -1;
        int wee = -1;
        int lee = -1;
        while (true) // try until a valid position is sampled
        {
            if (wee != -1)
                break;
            for (int we = 0; we < width; we++)
            {
                for (int le = 0; le < length; le++)
                {
                    // skip corners
                    if (we == 0 && le == 0)
                        continue;
                    if (we == 0 && le == length - 1)
                        continue;
                    if (we == width - 1 && le == 0)
                        continue;
                    if (we == width - 1 && le == length - 1)
                        continue;

                    if (we == 0 || le == 0 || wee == length - 1 || lee == length - 1)
                    {
                        // randomize selection
                        if (Random.Range(0.0f, 1.0f) < 0.1f)
                        {
                            int dist = System.Math.Abs(wr - we) + System.Math.Abs(lr - le);
                            if (dist > max_dist) // must be placed far away from the player
                            {
                                wee = we;
                                lee = le;
                                max_dist = dist;
                            }
                        }
                    }
                }
            }
        }


        // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION  ***
        // implement an algorithm that checks whether
        // all paths between the player at (wr,lr) and the exit (wee, lee)
        // are blocked by walls. i.e., there's no way to get to the exit!
        // if this is the case, you must guarantee that there is at least 
        // one accessible path (any path) from the initial player position to the exit
        // by removing a few wall blocks (removing all of them is not acceptable!)
        // this is done as a post-processing step after the CSP solution.
        // It might be case that some constraints might be violated by this
        // post-processing step - this is OK.
        
        /*** implement what is described above ! */
        Vector2Int player_station_pos = new Vector2Int(wr, lr);
        Vector2Int exit_pos = new Vector2Int(wee, lee);
        bool isPath = implAStarAlgo(player_station_pos, exit_pos, solution);
        if (!isPath)
        {
            while (!isPath)
            {
                List<Vector2Int> vector2IntWallsToRemove = fetchRemovableWalls(player_station_pos, exit_pos, solution);
                List<int[]> removableWalls = vector2IntWallsToRemove.Select(vec => new int[] { vec.xCoordinate, vec.yCoordinate }).ToList();

                foreach (var wallCoords in removableWalls)
                {
                    solution[wallCoords[0], wallCoords[1]] = new List<TileType> { TileType.FLOOR }; // Remove wall
                }
                isPath = implAStarAlgo(player_station_pos, exit_pos, solution);
            }
        }


        // the rest of the code creates the scenery based on the grid state 
        // you don't need to modify this code (unless you want to replace the virus
        // or other prefabs with something else you like)
        int w = 0;
        for (float x = bounds.min[0]; x < bounds.max[0]; x += bounds.size[0] / (float)width - 1e-6f, w++)
        {
            int l = 0;
            for (float z = bounds.min[2]; z < bounds.max[2]; z += bounds.size[2] / (float)length - 1e-6f, l++)
            {
                if ((w >= width) || (l >= width))
                    continue;

                float y = bounds.min[1];
                if ((w == wee) && (l == lee)) // this is the exit
                {
                    GameObject house = Instantiate(house_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    house.name = "HOUSE";
                    house.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                    if (l == 0)
                        house.transform.Rotate(0.0f, 270.0f, 0.0f);
                    else if (w == 0)
                        house.transform.Rotate(0.0f, 0.0f, 0.0f);
                    else if (l == length - 1)
                        house.transform.Rotate(0.0f, 90.0f, 0.0f);
                    else if (w == width - 1)
                        house.transform.Rotate(0.0f, 180.0f, 0.0f);

                    house.AddComponent<BoxCollider>();
                    house.GetComponent<BoxCollider>().isTrigger = true;
                    house.GetComponent<BoxCollider>().size = new Vector3(3.0f, 3.0f, 3.0f);
                    house.AddComponent<House>();
                    ObjectData houseData = new ObjectData("House", house.transform.position, house.transform.rotation, house.transform.localScale);
                    objDetails.Add(houseData);
                    createdGameObjs.Add(house);
                }
                else if (solution[w, l][0] == TileType.WALL)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "WALL";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + 0.5f, y + storey_height / 2.0f, z + 0.5f);
                    // cube.GetComponent<Renderer>().material.color = Color.red;
                    Material grassMaterial = Resources.Load<Material>("Stylize_Grass_diffuse");
                    cube.GetComponent<Renderer>().material = grassMaterial;
                    createdGameObjs.Add(cube);
                    ObjectData wallData = new ObjectData("Wall", cube.transform.position, cube.transform.rotation, cube.transform.localScale);
                    objDetails.Add(wallData);
                }
                else if (solution[w, l][0] == TileType.VIRUS)
                {
                    GameObject virus = Instantiate(virus_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    virus.name = "COVID";
                    virus.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);

                    virus.AddComponent<Virus>();
                    Virus virusScript = virus.GetComponent<Virus>();
                    virusScript.maxHeight = storey_height / 4.0f;
                    virusMaxHeight = storey_height / 4.0f;

                    virus.GetComponent<Rigidbody>().mass = 10000;
                    ObjectData virusData = new ObjectData("Virus", virus.transform.position, virus.transform.rotation, virus.transform.localScale);
                    objDetails.Add(virusData);
                    createdGameObjs.Add(virus);
                }
                else if (solution[w, l][0] == TileType.DRUG)
                {
                    // GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    // capsule.name = "DRUG";
                    // capsule.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    // capsule.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    // capsule.GetComponent<Renderer>().material.color = Color.green;
                    // capsule.AddComponent<Drug>();
                    // CapsuleCollider capsuleCollider = capsule.GetComponent<CapsuleCollider>();
                    // capsuleCollider.center = new Vector3(0, 0, 0);
                    // capsuleCollider.height = 1 / capsule.transform.localScale.y; // Adjusting for the local scale
                    // capsuleCollider.radius = 1 / capsule.transform.localScale.x;
                    // createdGameObjs.Add(capsule);
                    // ObjectData capsuleData = new ObjectData("Capsule", capsule.transform.position, capsule.transform.rotation, capsule.transform.localScale);
                    // objDetails.Add(capsuleData);

                    GameObject capsule = Instantiate(gem_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    capsule.name = "DRUG";
                    capsule.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    capsule.transform.position = new Vector3(x, y + 1.0f, z);
                    capsule.transform.rotation = Quaternion.Euler(-90f, 45f, 0f);

                    Rigidbody rb = capsule.AddComponent<Rigidbody>();
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    rb.useGravity = false;
                    
                    capsule.GetComponent<Renderer>().material.color = Color.green;
                    capsule.AddComponent<Drug>();
                    createdGameObjs.Add(capsule);
                    ObjectData capsuleData = new ObjectData("Capsule", capsule.transform.position, capsule.transform.rotation, capsule.transform.localScale);
                    objDetails.Add(capsuleData);
                }
                else if (solution[w, l][0] == TileType.WATER)
                {
                    GameObject water = Instantiate(water_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    water.name = "WATER";
                    water.transform.localScale = new Vector3(1.0f * bounds.size[0] / (float)width, 6.0f, 1.0f * bounds.size[2] / (float)length);
                    water.transform.position = new Vector3(x + 0.5f, y + 0.1f, z + 0.5f);
                    ObjectData waterData = new ObjectData("Water", water.transform.position, water.transform.rotation, water.transform.localScale);
                    objDetails.Add(waterData);
                    createdGameObjs.Add(water);

                    // GameObject water = Instantiate(water_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    // water.name = "WATER";
                    // water.transform.localScale = new Vector3( bounds.size[0] / (float)width, 1.0f, bounds.size[2] / (float)length);
                    // water.transform.position = new Vector3(x + 0.5f, y + 0.1f, z + 0.5f);
                    // ObjectData waterData = new ObjectData("Water", water.transform.position, water.transform.rotation, water.transform.localScale);
                    // objDetails.Add(waterData);
                    // createdGameObjs.Add(water);

                    // GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // cube.name = "WATER_BOX";
                    // cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height / 20.0f, bounds.size[2] / (float)length);
                    // cube.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                    // cube.GetComponent<Renderer>().material.color = Color.grey;
                    // cube.GetComponent<BoxCollider>().size = new Vector3(1.1f, 20.0f * storey_height, 1.1f);
                    // cube.GetComponent<BoxCollider>().isTrigger = true;
                    // cube.AddComponent<Water>();
                    // ObjectData WaterBoxData = new ObjectData("WaterBox", cube.transform.position, cube.transform.rotation, cube.transform.localScale);
                    // objDetails.Add(WaterBoxData);
                    // createdGameObjs.Add(cube);
                }
            }
        }
    }


    
    // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION JUST TO ADD SOUNDS ***
    // YOU MAY CHOOSE ANY SHORT SOUNDS (<2 sec) YOU WANT FOR A VIRUS HIT, A VIRUS INFECTION,
    // GETTING INTO THE WATER, AND REACHING THE EXIT
    // note: you may also change other scripts/functions to add sound functionality,
    // along with the functionality for the starting the level, or repeating it
    public void PlayAgain()
    {
        // Reload the current scene to start over with a new procedural generation
        play_again_canvas.enabled = false;
        StartCoroutine(PlayAgainCoroutine());
    }

    private IEnumerator PlayAgainCoroutine()
    {
        ResetLevel();
        yield return new WaitForSeconds(1.0f);
    }

    private void ResetLevel()
    {
        InitializeLevel("reset");
    }

    public void UpdateVirusReferencesToPlayer()
    {
        Virus[] viruses = FindObjectsOfType<Virus>();

        foreach (Virus virus in viruses)
        {
            virus.UpdatePlayerReference(fps_player_obj);
        }
    }



    public void TryLevelAgain()
    {
        try_again_canvas.enabled = false;
        StartCoroutine(TryAgainCoroutine());
        // recreateSameLevel();
    }

    private IEnumerator TryAgainCoroutine()
    {
        recreateSameLevel();
        yield return new WaitForSeconds(1.0f);
    }

    // public void SolveCanvas()
    // {
    //     solve_canvas.enabled = true;
    //     // StartCoroutine(TryAgainCoroutine());
    //     // recreateSameLevel();
    // }

    // public void CheckAnswer()
    // {
    //     int userAnswer;

    //     if (int.TryParse(answerInput.text, out userAnswer))
    //     {
    //         if (userAnswer == correctAnswer)
    //         {
    //             successText.text = "Success! You got it right!";
    //             PlayAgain();
    //         }
    //         else
    //         {
    //             successText.text = "Try again. Incorrect answer.";
    //         }
    //     }
    //     else
    //     {
    //         successText.text = "Invalid input. Please enter a number.";
    //     }
    // }

    private void recreateSameLevel(){
        InitializeLevel("tryAgain");
    }

    public void PlaySoundWithLimit(AudioClip clip, float duration)
    {
        source.PlayOneShot(clip);
        StartCoroutine(StopAudioAfterDuration(duration));
    }

    private IEnumerator StopAudioAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        source.Stop();
    }
    void Update()
    {
        if (player_health < 0.001f) // the player dies here
        {
            PlaySoundWithLimit(player_health_gone_sound, 1.5f);
            text_box.GetComponent<Text>().text = "Failed!";

            if (fps_player_obj != null)
            {
                GameObject grave = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grave.name = "GRAVE";
                grave.transform.localScale = new Vector3(bounds.size[0] / (float)width, 2.0f * storey_height, bounds.size[2] / (float)length);
                grave.transform.position = fps_player_obj.transform.position;
                grave.GetComponent<Renderer>().material.color = Color.black;
                createdGameObjs.Add(grave);
                if (fps_player_obj != null){
                    Camera playerCam = fps_player_obj.GetComponentInChildren<Camera>();
                    if (playerCam != null)
                    {
                        Object.Destroy(playerCam.gameObject);
                    }
                }
                Object.Destroy(fps_player_obj);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                try_again_canvas.enabled = true;                
            }

            return;
        }
        if (player_entered_house) // the player suceeds here, variable manipulated by House.cs
        {
            PlaySoundWithLimit(house_reached_sound, 1.5f);
            
            if (virus_landed_on_player_recently)
                text_box.GetComponent<Text>().text = "Washed it off at home! Success!!!";
            else{
                text_box.GetComponent<Text>().text = "Success!!!";
            }
            if (fps_player_obj != null){
                Camera playerCam = fps_player_obj.GetComponentInChildren<Camera>();
                if (playerCam != null)
                {
                    Object.Destroy(playerCam.gameObject);
                }
            }
            
            Object.Destroy(fps_player_obj);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            solve_canvas.enabled = true;
            return;
        }

        if (Time.time - timestamp_last_msg > 7.0f) // renew the msg by restating the initial goal
        {
            text_box.GetComponent<Text>().text = "Find your home!";            
        }

        // virus hits the players (boolean variable is manipulated by Virus.cs)
        // if (virus_landed_on_player_recently)
        // {
        //     PlaySoundWithLimit(virus_sound, 1.5f);
        //     float time_since_virus_landed = Time.time - timestamp_virus_landed;
        //     if (time_since_virus_landed > 5.0f)
        //     {
        //         player_health -= Random.Range(0.25f, 0.5f) * (float)num_virus_hit_concurrently;
        //         player_health = Mathf.Max(player_health, 0.0f);
        //         if (num_virus_hit_concurrently > 1)
        //             text_box.GetComponent<Text>().text = "Ouch! Infected by " + num_virus_hit_concurrently + " viruses";
        //         else
        //             text_box.GetComponent<Text>().text = "Ouch! Infected by a virus";
        //         timestamp_last_msg = Time.time;
        //         timestamp_virus_landed = float.MaxValue;
        //         virus_landed_on_player_recently = false;
        //         num_virus_hit_concurrently = 0;
        //     }
        //     else
        //     {
        //         if (num_virus_hit_concurrently == 1)
        //             text_box.GetComponent<Text>().text = "A virus landed on you. Infection in " + (5.0f - time_since_virus_landed).ToString("0.0") + " seconds. Find water or drug!";
        //         else
        //             text_box.GetComponent<Text>().text = num_virus_hit_concurrently + " viruses landed on you. Infection in " + (5.0f - time_since_virus_landed).ToString("0.0") + " seconds. Find water or drug!";
        //     }
        // }

        // drug picked by the player  (boolean variable is manipulated by Drug.cs)
        if (drug_landed_on_player_recently)
        {
            PlaySoundWithLimit(drug_sound, 1.5f);
            if (player_health < 0.999f || virus_landed_on_player_recently)
                text_box.GetComponent<Text>().text = "Phew! New drug helped!";
            else
                text_box.GetComponent<Text>().text = "No drug was needed!";
            timestamp_last_msg = Time.time;
            player_health += Random.Range(0.25f, 0.75f);
            player_health = Mathf.Min(player_health, 1.0f);
            drug_landed_on_player_recently = false;
            timestamp_virus_landed = float.MaxValue;
            virus_landed_on_player_recently = false;
            num_virus_hit_concurrently = 0;
        }

        // splashed on water  (boolean variable is manipulated by Water.cs)
        // if (player_is_on_water)
        // {
        //     PlaySoundWithLimit(water_sound, 1.5f);
        //     if (virus_landed_on_player_recently)
        //         text_box.GetComponent<Text>().text = "Phew! Washed it off!";
        //     timestamp_last_msg = Time.time;
        //     timestamp_virus_landed = float.MaxValue;
        //     virus_landed_on_player_recently = false;
        //     num_virus_hit_concurrently = 0;
        // }

        // update scroll bar (not a very conventional manner to create a health bar, but whatever)
        // scroll_bar.GetComponent<Scrollbar>().size = player_health;
        // if (player_health < 0.5f)
        // {
        //     ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
        //     cb.disabledColor = new Color(1.0f, 0.0f, 0.0f);
        //     scroll_bar.GetComponent<Scrollbar>().colors = cb;
        // }
        // else
        // {
        //     ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
        //     cb.disabledColor = new Color(0.0f, 1.0f, 0.25f);
        //     scroll_bar.GetComponent<Scrollbar>().colors = cb;
        // }

        
    }
}

   


    