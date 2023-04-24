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
    AnnotationStats stats;

    [Header("Visualization")]
    [SerializeField]
    int activeSequenceIndex = 0;
    
    [SerializeField]
    List<AnomalySequencer> anomalySequencers = new List<AnomalySequencer>();
    
    float timer = 0f;
    bool doneWithSequences = false;

    Texture2D loadingTexture;
    VisualElement pictureElement;

    AnomalySequencer activeSequence;


    void Start()
    {
        UIDoc = GetComponent<UIDocument>();
        pictureElement = UIDoc.rootVisualElement.Q("image-element");

        if (ResetStatsOnStartup) stats.ResetStats();

        PictureButton normalButton = new PictureButton(AnAnnotationType.Normal, "bt-normal");
        PictureButton trashButton = new PictureButton(AnAnnotationType.Trash, "bt-trash");
        PictureButton bikeButton = new PictureButton(AnAnnotationType.Bike, "bt-bike");
        PictureButton jaywalkerButton = new PictureButton(AnAnnotationType.Jaywalker, "bt-jaywalker");

        normalButton.onPressedEvent += SortPicture;
        trashButton.onPressedEvent += SortPicture;
        bikeButton.onPressedEvent += SortPicture;
        jaywalkerButton.onPressedEvent += SortPicture;

        LoadImage(bad_load);

        loadingTexture = new Texture2D(bad_load.width, bad_load.height);

        for(int i = 1; i <= 34; i++)
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
        
        if(timer < Time.time && !doneWithSequences)
        {
            LoadImage(activeSequence.GetFromSequence(loadingTexture));
            timer = Time.time + videoPlayrate;
        }
        
    }

    void LoadImage(Texture2D texture)
    {
        pictureElement.style.backgroundImage = new StyleBackground(texture);
    }

    private void SortPicture(AnAnnotationType type)
    {
        stats.sequenceDatas.Add(activeSequence.GetSerialzedData(type));
        activeSequenceIndex++;
        if(activeSequenceIndex == anomalySequencers.Count)
        {
            doneWithSequences = true;
            LoadImage(done_anno);
        }
        else activeSequence = anomalySequencers[activeSequenceIndex];
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
    Bike,
    Jaywalker
}
