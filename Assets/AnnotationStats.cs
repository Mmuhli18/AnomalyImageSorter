using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu()]
public class AnnotationStats : ScriptableObject
{
    public AnnotationCounter Normal = new AnnotationCounter(AnAnnotationType.Normal);
    public AnnotationCounter Trash = new AnnotationCounter(AnAnnotationType.Trash);
    public AnnotationCounter Bike = new AnnotationCounter(AnAnnotationType.Bike);
    public AnnotationCounter Jaywalker = new AnnotationCounter(AnAnnotationType.Jaywalker);
    public List<SerialzedSequenceData> sequenceDatas = new List<SerialzedSequenceData>();

    public int GetAndAddToCount(AnAnnotationType type)
    {
        int count = 0;
        switch (type)
        {
            case AnAnnotationType.Normal:
                count = Normal.count;
                Normal.count++;
                break;
            case AnAnnotationType.Trash:
                count = Trash.count;
                Trash.count++;
                break;
            case AnAnnotationType.Bike:
                count = Bike.count;
                Bike.count++;
                break;
            case AnAnnotationType.Jaywalker:
                count = Jaywalker.count;
                Jaywalker.count++;
                break;
        }
        
        return count;
    }

    public void ResetStats()
    {
        Normal = new AnnotationCounter(AnAnnotationType.Normal);
        Trash = new AnnotationCounter(AnAnnotationType.Trash);
        Bike = new AnnotationCounter(AnAnnotationType.Bike);
        Jaywalker = new AnnotationCounter(AnAnnotationType.Jaywalker);
        sequenceDatas = new List<SerialzedSequenceData>();
}
}

[Serializable]
public class AnnotationCounter
{
    public int count;
    public AnAnnotationType type { private set; get; }

    public AnnotationCounter(AnAnnotationType t)
    {
        count = 0;
        type = t;
    }
}