using System.Collections.Generic;
using System.Linq;

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
        // Setup temporary lists
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
                float newCost = currentPoint.cost + currentPoint.connections[successor];
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

        // Build a pathfinding data table with the calculated information and return it
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

        // Start at the target and move according to the predecessors to the start point
        Point currentPoint = target;
        while (currentPoint != startPoint)
        {
            bestRoute.Add(currentPoint);
            currentPoint = pathfindingDataTable[currentPoint];
        }

        // Start point isn't included, so add it manually
        bestRoute.Add(startPoint);

        // Reverse the route as the algorithm scans it backwards
        bestRoute.Reverse();

        return bestRoute;
    }
}

public class Point
{
    public string name;
    public float cost;
    public Point predecessor;
    public Dictionary<Point, float> connections;

    public Point(string name)
    {
        this.name = name;
        connections = new();
        Reset();
    }
    public void Reset()
    {
        cost = float.PositiveInfinity;
    }
}
