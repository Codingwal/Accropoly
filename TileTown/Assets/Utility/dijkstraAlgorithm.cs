public static class MainClass
{
    public static void Main()
    {
        Point a = new("A");
        Point b = new("B");
        Point c = new("C");
        Point d = new("D");
        Point e = new("E");
        Point f = new("F");
        Point g = new("G");

        a.connections = new() {
            {c, 5},
            {e, 3}
        };
        b.connections = new() {
            {d, 6},
            {c, 3}
        };
        c.connections = new() {
            {a, 5},
            {b, 3},
            {d, 8},
            {e, 10}
        };
        d.connections = new() {
            {b, 6},
            {c, 8},
            {f, 7},
            {g, 9}
        };
        e.connections = new() {
            {a, 3},
            {c, 19},
            {g, 4}
        };
        f.connections = new() {
            {d, 7},
            {g, 4}
        };
        g.connections = new() {
            {d, 9},
            {e, 4},
            {f, 4}
        };

        List<Point> allPoints = new() { a, b, c, d, e, f, g };

        Pathfinder pathfinder = new Pathfinder(allPoints);
        Dictionary<Point, Point> pathfindingDataTable = pathfinder.CreatePathfindingDataTable(c);
        List<Point> bestRoute = pathfinder.CalculateBestRoute(c, b, pathfindingDataTable);

        foreach (Point point in bestRoute)
        {
            Console.WriteLine(point.name);
        }
    }
}
public class Pathfinder
{
    public List<Point> allPoints;

    public List<Point> visited = new();
    public List<Point> queue = new();

    public Pathfinder(List<Point> allPoints)
    {
        this.allPoints = allPoints;
    }

    public Dictionary<Point, Point> CreatePathfindingDataTable(Point startPoint)
    {
        visited = new();
        queue = new() { startPoint };

        // Reset the points
        foreach (Point point in allPoints)
        {
            point.Reset();
        }

        // Set the startpoint cost to 0
        startPoint.cost = 0;

        // DataTableCalculationLoop
        while (queue.Count > 0)
        {
            // Select point with lowest cost of queue
            Point currentPoint = new("placeholder");
            foreach (Point point in queue)
            {
                if (point.cost < currentPoint.cost)
                {
                    currentPoint = point;
                }
            }

            // Get all successors 
            List<Point> successors = currentPoint.connections.Keys.ToList();

            foreach (Point successor in successors)
            {
                // Skip if it already has been visited
                if (visited.Contains(successor))
                {
                    continue;
                }

                // Set their cost and precessor
                float newCost = currentPoint.cost +  currentPoint.connections[successor];
                if (newCost < successor.cost)
                {
                    successor.cost = newCost;
                    successor.predecessor = currentPoint;
                }

                // Add successors to queue
                queue.Add(successor);
            }

            // Add point to "visited"
            visited.Add(currentPoint);

            // Remove point from queue
            queue.Remove(currentPoint);
        }
        Dictionary<Point, Point> pathfindingDataTable = new();
        
        foreach (Point point in allPoints)
        {
            pathfindingDataTable.Add(point, point.predecessor!);
        }

        return pathfindingDataTable;
    }

    public List<Point> CalculateBestRoute(Point startPoint, Point target, Dictionary<Point, Point> pathfindingDataTable)
    {
        List<Point> bestRoute = new();

        Point currentPoint = target;
        while (currentPoint != startPoint)
        {
            bestRoute.Add(currentPoint);
            currentPoint = pathfindingDataTable[currentPoint];
        }
        bestRoute.Add(startPoint);

        bestRoute.Reverse();
        return bestRoute;
    }
}

public class Point
{
    public string name;
    public float cost;
    public Point? predecessor;
    public Dictionary<Point, float> connections;

    public Point(string name)
    {
        this.name = name;
        connections = new();
        Reset();
    }
    public Point(string name, Dictionary<Point, float> connections)
    {
        this.name = name;
        this.connections = connections;
        Reset();
    }
    public void Reset()
    {
        cost = float.PositiveInfinity;
    }

    public override string ToString()
    {
        return $"POINT {name}: [Cost: {cost}, Predecessor: {predecessor?.name}]";
    }
}