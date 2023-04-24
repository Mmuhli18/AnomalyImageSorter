using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[Serializable]
public class AnomalySequencer
{
    [SerializeField]
    int StartNumber;
    [SerializeField]
    int EndNumber;
    int currentIndex = 0;
    [SerializeField]
    string path;

    public AnomalySequencer(int start, int end, string p)
    {
        path = p;
        StartNumber = start;
        EndNumber = end;
    }

    public Texture2D GetSetTexture(int i, Texture2D texture)
    {
        string number = (StartNumber + i).ToString();
        string Os = "";
        for(int j = number.Length; j < 5; j++)
        {
            Os += "0";
        }
        byte[] bytes = File.ReadAllBytes(path + Os + number + ".jpg");
        ImageConversion.LoadImage(texture, bytes);
        return texture;
    }

    public Texture2D GetFromSequence(Texture2D texture)
    {
        if (currentIndex > (EndNumber - StartNumber)) currentIndex = 0;
        else currentIndex++;
        return GetSetTexture(currentIndex, texture);
    }

    public SerialzedSequenceData GetSerialzedData(AnAnnotationType type)
    {
        SerialzedSequenceData data = new SerialzedSequenceData();
        data.StartFrame = StartNumber;
        data.EndFrame = EndNumber;
        data.AnnotationType = type;
        data.Path = path;
        return data;
    }
}

[Serializable]
public class SerialzedSequenceData
{
    public int StartFrame;
    public int EndFrame;
    public AnAnnotationType AnnotationType;
    public string Path;
}
