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
    public SortingType sortingType = SortingType.AnomalyData;

    [Header("To make work")]
    public AnnotationStats firstSortStats;
    public AnnotationStats specificSortStats;
    public bool includeDupes = false;
    public string dirPath;

    
    int dataWriteCount = 0;
    bool currentlyWritingData = false;
    //
    //For anomaly sorting
    [Header("Anomaly Sorting")]
    string csvString = "";

    FolderNumberCounter jaywalkCount = new FolderNumberCounter();
    FolderNumberCounter sidewalkCount = new FolderNumberCounter();
    FolderNumberCounter outLaneCount = new FolderNumberCounter();

    [SerializeField]
    List<SerialzedSequenceStructureData> datas = new List<SerialzedSequenceStructureData>();

    //
    //For normal sorting
    [Header("Normal Sorting")]
    public List<DataFolder> dataFolders = new List<DataFolder>();
    public int Normal_FolderFileIndex = 1;

    private void Start()
    {
        if (sortingType == SortingType.AnomalyData)
            StartupIfAnomaly();
        else
            StartupIfNormal();
    }

    void StartupIfAnomaly()
    {
        dirPath = folderPath + "/Sorted/anomalyData_" + DateTime.Now.ToString("yyyy/MM/dd HH.mm.ss") + "/";
        EnsureFolderExists(dirPath);

        for (int i = 0; i < firstSortStats.sequenceDatas.Count; i++)
        {
            if (!(firstSortStats.sequenceDatas[i].AnnotationType == AnAnnotationType.Trash))
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
                if (specificSortStats.sequenceDatas[i].AnnotationType == AnAnnotationType.Better)
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

    void StartupIfNormal()
    {
        dirPath = folderPath + "/Sorted/normalData_" + DateTime.Now.ToString("yyyy/MM/dd HH.mm.ss") + "/";
        EnsureFolderExists(dirPath);

        List<string> folderNames = new List<string>();
        for(int i = 1; i < 35; i++)
        {
            string folder = "Test";
            if (i < 10) folder += "0";
            folder += "0" + i;
            folderNames.Add(folder);
        }

        for(int i = 0; i < folderNames.Count; i++)
        {
            List<SerialzedSequenceData> data = new List<SerialzedSequenceData>();
            for(int j = 0; j < firstSortStats.sequenceDatas.Count; j++)
            {
                if(firstSortStats.sequenceDatas[j].Path == folderNames[i])
                {
                    data.Add(firstSortStats.sequenceDatas[j]);
                }
            }
            dataFolders.Add(new DataFolder(folderNames[i], data, Directory.GetFiles(folderPath + "/" + folderNames[i], "*.jpg").Length));
        }

        Normal_FolderFileIndex = dataFolders[dataWriteCount].firstFile;
        EnsureFolderExists(dirPath + "/" + dataFolders[dataWriteCount].name);
        Debug.Log("Writing data folder: " + (dataWriteCount + 1) + "/" + dataFolders.Count);
    }

    private void Update()
    {
        if (sortingType == SortingType.AnomalyData)
            UpdateForAnomaly();
        else 
            UpdateForNormal();
    }

    void UpdateForAnomaly()
    {
        if (dataWriteCount < datas.Count && !currentlyWritingData)
        {
            if (!datas[dataWriteCount].isDupe)
            {
                csvString += datas[dataWriteCount].GetCSVData();
                if (dataWriteCount != datas.Count - 1) csvString += "\n";

                if (doWriteImages) StartCoroutine(SaveSequenceToJPG(datas[dataWriteCount]));
            }
            dataWriteCount++;
            Debug.Log("Writing data: " + dataWriteCount + "/" + datas.Count);
        }
        else if (dataWriteCount == datas.Count)
        {
            File.WriteAllText(dirPath + "data.csv", csvString);
        }
    }

    void UpdateForNormal()
    {
        if (dataWriteCount < dataFolders.Count)
        {
            SaveNormalsInFolder(dataFolders[dataWriteCount]);
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

    void SaveNormalsInFolder(DataFolder folder)
    {
        int i = Normal_FolderFileIndex;
        if (i <= folder.lastFile)
        {
            if (!folder.CheckIfForbidden(i))
            {
                //Os
                string Os = "";
                for (int j = i.ToString().Length; j < 5; j++)
                {
                    Os += "0";
                }

                //FilePath
                string fileName = Os + i + ".jpg";
                string targetPath = folderPath + "/" + folder.name + "/" + fileName;

                //Writing
                byte[] bytes = File.ReadAllBytes(targetPath);
                File.WriteAllBytes(dirPath + "/" + folder.name + "/" + fileName, bytes);
            }
            Normal_FolderFileIndex++;
            if (Normal_FolderFileIndex % 500 == 0) Debug.Log("Copying images: " + Normal_FolderFileIndex + "/" + folder.lastFile);
        }
        else
        {
            dataWriteCount++;
            Normal_FolderFileIndex = dataFolders[dataWriteCount].firstFile;
            EnsureFolderExists(dirPath + "/" + dataFolders[dataWriteCount].name);
            Debug.Log("Writing data folder: " + (dataWriteCount + 1) + "/" + dataFolders.Count);
        }
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
public class DataFolder
{
    public List<ForbiddenFiles> forbiddenFiles = new List<ForbiddenFiles>();
    public string name;
    public int firstFile = 1;
    public int lastFile = 5000;
    public DataFolder(string n, List<SerialzedSequenceData> data, int fileCount)
    {
        for(int i = 0; i < data.Count; i++)
        {
            forbiddenFiles.Add(new ForbiddenFiles(data[i].StartFrame, data[i].EndFrame));
        }
        name = n;
        lastFile = fileCount;
    }

    public bool CheckIfForbidden(int number)
    {
        for(int i = 0; i < forbiddenFiles.Count; i++)
        {
            if (forbiddenFiles[i].CheckIfForbidden(number)) return true;
        }
        return false;
    }

    [Serializable]
    public class ForbiddenFiles
    {
        [SerializeField] int start;
        [SerializeField] int stop;

        public ForbiddenFiles(int start, int stop)
        {
            this.start = start;
            this.stop = stop;
        }

        public bool CheckIfForbidden(int number)
        {
            if (number >= start && number <= stop) return true;
            return false;
        }
    }
}

public enum SortingType
{
    AnomalyData,
    NormalData
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