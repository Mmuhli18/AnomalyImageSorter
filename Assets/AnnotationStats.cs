using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu()]
public class AnnotationStats : ScriptableObject
{
    public List<SerialzedSequenceData> sequenceDatas = new List<SerialzedSequenceData>();
    public int JaywalkerCount = 0;
    public int BikeOnSidewalkCount = 0;
    public int BikeOutOfLaneCount = 0;

    public void ResetStats()
    {
        sequenceDatas = new List<SerialzedSequenceData>();
        JaywalkerCount = 0;
        BikeOnSidewalkCount = 0;
        BikeOutOfLaneCount = 0;
    }

    public int GetAndAddToCountOfType(AnAnnotationType type)
    {
        switch (type)
        {
            case AnAnnotationType.Jaywalker:
                JaywalkerCount++;
                return JaywalkerCount;
            case AnAnnotationType.BikeSidewalk:
                BikeOnSidewalkCount++;
                return BikeOnSidewalkCount;
            case AnAnnotationType.BikeOutOfLane:
                BikeOutOfLaneCount++;
                return BikeOutOfLaneCount;
        }
        return 0;
    }

    public string GetDataAsCSV()
    {
        string dataString = "";
        for(int i = 0; i < sequenceDatas.Count; i++)
        {
            dataString += sequenceDatas[i].Path + "," + sequenceDatas[i].StartFrame + ","
                + sequenceDatas[i].EndFrame + "," + sequenceDatas[i].AnnotationType;
            if(i != sequenceDatas.Count - 1) dataString += "\n";
        }
        return dataString;
    }
}