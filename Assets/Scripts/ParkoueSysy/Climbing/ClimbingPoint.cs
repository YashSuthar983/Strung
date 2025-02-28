using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ClimbingPoint : MonoBehaviour
{
    public List<Neighbour> neighbours;
    public bool MountClimbToChrouchPoint;
    private void Awake()
    {
        var toWayClimbNeighbour = neighbours.Where(n=>n.isPointTwoWay);
        foreach(var neighbour in toWayClimbNeighbour)
        {
            neighbour.ClimbingPoint?.CreatePointConnection(this, -neighbour.pointDirection, neighbour.connectionType, neighbour.isPointTwoWay);
        }
    }
    public void CreatePointConnection(ClimbingPoint climbingPoint,Vector2 pointDirection,ConnectionType connectionType,bool isPointTwoWay)
    {
        var neighbour = new Neighbour()
        {
            ClimbingPoint = climbingPoint,
            pointDirection = pointDirection,
            connectionType = connectionType,
            isPointTwoWay = isPointTwoWay,
            matchPos = climbingPoint.transform.position


        };
        neighbours.Add(neighbour);
    }

    public Neighbour GetNeighbour(Vector2 climbDir)
    {
        Neighbour neighbour=null;

        if(climbDir.y!=0)
        {
            neighbour=neighbours.FirstOrDefault(n=> n.pointDirection.y==climbDir.y);
        }
        if (climbDir.x!= 0 )
        {
            neighbour = neighbours.FirstOrDefault(n => n.pointDirection.x == climbDir.x);
        }
        if(neighbour==null)
        {
            Debug.Log("No Neighbour");
        }
        return neighbour;
    }
    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, transform.right, Color.red);
        foreach(var neighbour in neighbours)
        {
            if (neighbour.ClimbingPoint==null) continue;
            Debug.DrawLine(transform.position,neighbour.ClimbingPoint.transform.position,(neighbour.isPointTwoWay)?Color.green:Color.red);
        }
    }
}

[System.Serializable]
public class Neighbour
{
    public ClimbingPoint ClimbingPoint;
    public Vector2 pointDirection;
    public ConnectionType connectionType;
    public bool isPointTwoWay=true;
    public Vector3 matchPos;

}

public enum ConnectionType { Jump,Move}
