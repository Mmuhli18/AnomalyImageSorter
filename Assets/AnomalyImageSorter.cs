using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.IO;

public class AnomalyImageSorter : MonoBehaviour
{
    [Header("Settings")]
    public float videoPlayrate = 0.2f;
    public string folderPath = "C:/Users/morte/Documents/Test";
    public bool ResetStatsOnStartup = true;

    [Header("To make work")]
    [SerializeField]
    public static UIDocument UIDoc;
    public Texture2D bad_load;
    public Texture2D done_anno;
    [SerializeField]
    protected AnnotationStats stats;

    [Header("Visualization")]
    [SerializeField]
    protected int activeSequenceIndex = 0;
    
    [SerializeField]
    protected List<AnomalySequencer> anomalySequencers = new List<AnomalySequencer>();
    
    float timer = 0f;
    bool doneWithSequences = false;

    protected Texture2D loadingTexture;
    protected VisualElement pictureElement;

    protected AnomalySequencer activeSequence;


    protected string dirPath;


    void Start()
    {
        UIDoc = GetComponent<UIDocument>();

        if (ResetStatsOnStartup) stats.ResetStats();

        PictureButton normalButton = new PictureButton(AnAnnotationType.Normal, "bt-normal");
        PictureButton trashButton = new PictureButton(AnAnnotationType.Trash, "bt-trash");
        PictureButton bikeOutOfLaneButton = new PictureButton(AnAnnotationType.BikeOutOfLane, "bt-bike");
        PictureButton bikeSideWalkButton = new PictureButton(AnAnnotationType.BikeSidewalk, "bt-bike-sidewalk");
        PictureButton jaywalkerButton = new PictureButton(AnAnnotationType.Jaywalker, "bt-jaywalker");

        normalButton.onPressedEvent += SortPicture;
        trashButton.onPressedEvent += SortPicture;
        bikeOutOfLaneButton.onPressedEvent += SortPicture;
        bikeSideWalkButton.onPressedEvent += SortPicture;
        jaywalkerButton.onPressedEvent += SortPicture;

        LoadImage(bad_load);
        loadingTexture = new Texture2D(bad_load.width, bad_load.height);
        dirPath = folderPath + "/Sorted/anomalyData_" + DateTime.Now.ToString("yyyy/MM/dd HH.mm.ss") + "/";
        EnsureFolderExists(dirPath);
        

        for (int i = 1; i <= 34; i++)
        {
            string folderNumber = i.ToString();
            string Os = "";
            for (int j = folderNumber.Length; j < 3; j++)
            {
                Os += "0";
            }

            ImageFolder folder = new ImageFolder(folderPath, "Test" + Os + folderNumber);
            anomalySequencers.AddRange(folder.sequencers);
        }
        

        activeSequence = anomalySequencers[activeSequenceIndex];
        
    }

    private void Update()
    {
        
        if(timer < Time.time && !doneWithSequences && activeSequence != null)
        {
            LoadImage(activeSequence.GetFromSequence(loadingTexture));
            timer = Time.time + videoPlayrate;
        }
        
    }

    protected void LoadImage(Texture2D texture)
    {
        if (UIDoc == null) UIDoc = GetComponent<UIDocument>();
        if(pictureElement == null) pictureElement = UIDoc.rootVisualElement.Q("image-element");
        pictureElement.style.backgroundImage = new StyleBackground(texture);
    }

    protected void SortPicture(AnAnnotationType type)
    {
        SerialzedSequenceData data = activeSequence.GetSerialzedData(type);
        stats.sequenceDatas.Add(data);
        activeSequenceIndex++;
        if(activeSequenceIndex == anomalySequencers.Count)
        {
            doneWithSequences = true;
            LoadImage(done_anno);
        }
        else activeSequence = anomalySequencers[activeSequenceIndex];
        SaveDataToCSV();

        if (type == AnAnnotationType.Jaywalker || 
            type == AnAnnotationType.BikeOutOfLane || 
            type == AnAnnotationType.BikeSidewalk) 
                SaveSequenceToJPG(data);

    }

    void SaveDataToCSV()
    {
        string FilePath = dirPath + "data.csv";
        File.WriteAllText(FilePath, stats.GetDataAsCSV());
    }

    void SaveSequenceToJPG(SerialzedSequenceData sequenceData)
    {
        string FilePath = dirPath + sequenceData.AnnotationType.ToString() + "_" + stats.GetAndAddToCountOfType(sequenceData.AnnotationType) + "/";
        EnsureFolderExists(FilePath);
        Debug.Log("Writing to path: " + FilePath);
        for(int i = sequenceData.StartFrame; i <= sequenceData.EndFrame; i++)
        {
            //Os
            string Os = "";
            for (int j = i.ToString().Length; j < 5; j++)
            {
                Os += "0";
            }

            //FilePath
            string fileName = Os + i + ".jpg";
            string targetPath = folderPath + "/" + sequenceData.Path + "/" + fileName;
            

            //Writing
            byte[] bytes = File.ReadAllBytes(targetPath);
            File.WriteAllBytes(FilePath + fileName, bytes);
        }
        
    }

    protected void EnsureFolderExists(string path)
    {
        if (!Directory.Exists(path))
        {
            if (true) { Debug.Log("Creating directory: " + path); }
            Directory.CreateDirectory(path);
        }
    }
}

public class PictureButton
{
    public AnAnnotationType type;
    public Button button;
    public Action<AnAnnotationType> onPressedEvent;
    public PictureButton(AnAnnotationType t, string name)
    {
        button = AnomalyImageSorter.UIDoc.rootVisualElement.Q<Button>(name);
        type = t;
        button.RegisterCallback<MouseUpEvent>(x => onPressedEvent.Invoke(type));
    }
}

[Serializable]
public class ImageFolder
{
    public string name;
    public string path;
    public List<AnomalySequencer> sequencers = new List<AnomalySequencer>();

    [SerializeField]
    List<ImageData> imageDatas = new List<ImageData>();

    public ImageFolder(string p, string name)
    {
        path = p + "/" + name + "/";

        string fileData = File.ReadAllText(path + name + "_gt.txt");
        string[] lines = fileData.Split("\n");

        for (int i = 0; i < lines.Length; i++)
        {
            string[] splitLine = (lines[i].Trim()).Split(' ');
            ImageData data = new ImageData();
            string numberString = splitLine[0].Split('.')[0];
            if (numberString != "")
            {
                data.imageNumber = int.Parse(splitLine[0].Split('.')[0]);
                data.sequenceNumber = int.Parse(splitLine[1]);
                imageDatas.Add(data);
            }
        }

        int currentSequence = imageDatas[0].sequenceNumber;
        int sequenceStart = imageDatas[0].imageNumber;
        for (int i = 0; i < imageDatas.Count; i++)
        {
            if (!(imageDatas[i].sequenceNumber == currentSequence))
            {
                sequencers.Add(new AnomalySequencer(sequenceStart, imageDatas[i - 1].imageNumber, path));
                currentSequence = imageDatas[i].sequenceNumber;
                sequenceStart = imageDatas[i].imageNumber;
            }
        }
        sequencers.Add(new AnomalySequencer(sequenceStart, imageDatas[imageDatas.Count - 1].imageNumber, path));
    }
}

[Serializable]
public class ImageData
{
    public int imageNumber;
    public int sequenceNumber;
}

public enum AnAnnotationType
{
    Normal,
    Trash,
    BikeOutOfLane,
    BikeSidewalk,
    Jaywalker,
    New,
    Better
}
