using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Data
{
    public string protocol;
    public string data;

    public Data()
    {
        protocol = "";
        data = "";
    }

    public Data(string protocol, string data)
    {
        this.protocol = protocol;
        this.data = data;
    }
}

[System.Serializable]
public class ActiveData
{
    public bool p1Active;
    public bool p2Active;
    public bool p3Active;
    public bool p4Active;

    public ActiveData(bool p1, bool p2, bool p3, bool p4)
    {
        p1Active = p1;
        p2Active = p2;
        p3Active = p3;
        p4Active = p4;
    }
}

[System.Serializable]
public class PositionData
{
    public Vector3 p1Position;
    public Vector3 p2Position;
    public Vector3 p3Position;
    public Vector3 p4Position;

    public PositionData(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        p1Position = p1;
        p2Position = p2;
        p3Position = p3;
        p4Position = p4;
    }
}

[System.Serializable]
public class MoveData
{
    public Vector3 movePos;

    public MoveData(Vector3 movePos)
    {
        this.movePos = movePos;
    }
}

[System.Serializable]
public class RotationData
{
    public Quaternion p1Rotation;
    public Quaternion p2Rotation;
    public Quaternion p3Rotation;
    public Quaternion p4Rotation;

    public RotationData(Quaternion p1, Quaternion p2, Quaternion p3, Quaternion p4)
    {
        p1Rotation = p1;
        p2Rotation = p2;
        p3Rotation = p3;
        p4Rotation = p4;
    }
}