using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;

public class AnomalySorter : MonoBehaviour
{
    [Header("Input dataset folder here")]
    public string folderPath = "C:/Users/morte/Documents/Test";
    public bool doWriteImages = true;

    [Header("To make work")]
    public AnnotationStats firstSortStats;
    public AnnotationStats specificSortStats;
    public bool includeDupes = false;
    public string dirPath;

    int dataWriteCount = 0;
    bool currentlyWritingData = false;
    string csvString = "";

    FolderNumberCounter jaywalkCount = new FolderNumberCounter();
    FolderNumberCounter sidewalkCount = new FolderNumberCounter();
    FolderNumberCounter outLaneCount = new FolderNumberCounter();

    [SerializeField]
    List<SerialzedSequenceStructureData> datas = new List<SerialzedSequenceStructureData>();

    private void Start()
    {
        dirPath = folderPath + "/Sorted/anomalyData_" + DateTime.Now.ToString("yyyy/MM/dd HH.mm.ss") + "/";
        EnsureFolderExists(dirPath);

        for (int i = 0; i < firstSortStats.sequenceDatas.Count; i++)
        {
            if(!(firstSortStats.sequenceDatas[i].AnnotationType == AnAnnotationType.Trash))
            {
                SerialzedSequenceStructureData data = new SerialzedSequenceStructureData(firstSortStats.sequenceDatas[i]);
                data.sortedFolder = firstSortStats.sequenceDatas[i].AnnotationType + "_";
                if (firstSortStats.sequenceDatas[i].AnnotationType == AnAnnotationType.Jaywalker) data.sortedFolder += jaywalkCount.GetAndAddToCount();
                if (firstSortStats.sequenceDatas[i].AnnotationType == AnAnnotationType.BikeSidewalk) data.sortedFolder += sidewalkCount.GetAndAddToCount();
                if (firstSortStats.sequenceDatas[i].AnnotationType == AnAnnotationType.BikeOutOfLane) data.sortedFolder += outLaneCount.GetAndAddToCount();
                datas.Add(data);
            }
        }

        datas = datas.OrderBy(x => x.sortedFolder).ToList();

        if (!includeDupes)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                if(specificSortStats.sequenceDatas[i].AnnotationType == AnAnnotationType.Better)
                {
                    datas[i - 1].isDupe = true;
                }
                else if (specificSortStats.sequenceDatas[i].AnnotationType == AnAnnotationType.Worse)
                {
                    datas[i].isDupe = true;
                }
            }
        }

        
    }

    private void Update()
    {
        if (dataWriteCount < datas.Count && !currentlyWritingData)
        {
            if (!datas[dataWriteCount].isDupe)
            {
                csvString += datas[dataWriteCount].GetCSVData();
                if (dataWriteCount != datas.Count - 1) csvString += "\n";

                if(doWriteImages) StartCoroutine(SaveSequenceToJPG(datas[dataWriteCount]));
            }
            dataWriteCount++;
            Debug.Log("Writing data: " + dataWriteCount + "/" + datas.Count);
        }
        else if(dataWriteCount == datas.Count)
        {
            File.WriteAllText(dirPath + "data.csv", csvString);
        }
    }

    IEnumerator SaveSequenceToJPG(SerialzedSequenceStructureData sequenceData)
    {
        currentlyWritingData = true;
        string FilePath = dirPath + sequenceData.sortedFolder + "/";
        EnsureFolderExists(FilePath);
        //Debug.Log("Writing to path: " + FilePath);
        for (int i = sequenceData.StartFrame; i <= sequenceData.EndFrame; i++)
        {
            //Os
            string Os = "";
            for (int j = i.ToString().Length; j < 5; j++)
            {
                Os += "0";
            }

            //FilePath
            string fileName = Os + i + ".jpg";
            string targetPath = folderPath + "/" + sequenceData.datasetFolder + "/" + fileName;


            //Writing
            byte[] bytes = File.ReadAllBytes(targetPath);
            File.WriteAllBytes(FilePath + fileName, bytes);
        }
        currentlyWritingData = false;
        yield return 0;
    }

    protected void EnsureFolderExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}

public class FolderNumberCounter
{
    int count = 0;
    public string GetAndAddToCount()
    {
        string returnString = "";
        count++;
        if (count < 10) returnString += "0";
        return returnString + count.ToString();
    }
}

[Serializable]
public class SerialzedSequenceStructureData
{
    public string datasetFolder;
    public string sortedFolder;
    public int StartFrame;
    public int EndFrame;
    public AnAnnotationType AnnotationType;
    public bool isDupe = false;

    public SerialzedSequenceStructureData(string df, int start, int end, AnAnnotationType type)
    {
        datasetFolder = df;
        StartFrame = start;
        EndFrame = end;
        AnnotationType = type;
    }

    public SerialzedSequenceStructureData(SerialzedSequenceData data)
    {
        datasetFolder = data.Path;
        StartFrame = data.StartFrame;
        EndFrame = data.EndFrame;
        AnnotationType = data.AnnotationType;
    }

    public string GetCSVData()
    {
        return datasetFolder + "," + sortedFolder + "," + StartFrame + "," + EndFrame + "," + AnnotationType;
    }
}