using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.IO;

public class SpecificAnomalySorter : AnomalyImageSorter
{
    SerialzedSequenceData lastSequencer;
    Label dupeLabel;

    private void Start()
    {
        UIDoc = GetComponent<UIDocument>();

        if (ResetStatsOnStartup) stats.ResetStats();

        dupeLabel = UIDoc.rootVisualElement.Q<Label>("l-dupe");
        dupeLabel.style.display = DisplayStyle.None;

        PictureButton betterButton = new PictureButton(AnAnnotationType.Better, "bt-better");
        PictureButton newButton = new PictureButton(AnAnnotationType.New, "bt-new");
        PictureButton worseButton = new PictureButton(AnAnnotationType.Worse, "bt-worse");
        Button backButton = UIDoc.rootVisualElement.Q<Button>("bt-back");

        backButton.RegisterCallback<MouseUpEvent>(x => GoBack());
        betterButton.onPressedEvent += SortPicture;
        newButton.onPressedEvent += SortPicture;
        worseButton.onPressedEvent += SortPicture;

        LoadImage(bad_load);
        loadingTexture = new Texture2D(bad_load.width, bad_load.height);
        dirPath = folderPath + "/SpecifiedData_" + DateTime.Now.ToString("yyyy/MM/dd HH.mm.ss") + ".csv";

        string[] folders = Directory.GetDirectories(folderPath);
        for (int i = 0; i < folders.Length; i++)
        {
            string[] files = Directory.GetFiles(folders[i]);

            string firstFileNumber = Path.GetFileNameWithoutExtension(files[0]);

            string lastFileNumber = Path.GetFileNameWithoutExtension(files[files.Length - 1]);

            string anomalyName = folders[i].Split(Path.DirectorySeparatorChar)[folders[i].Split(Path.DirectorySeparatorChar).Length - 1];

            anomalySequencers.Add(new AnomalySequencer(int.Parse(firstFileNumber), int.Parse(lastFileNumber) - 1, folders[i] + "/", anomalyName));
        }
        activeSequence = anomalySequencers[activeSequenceIndex];
        newSequenceEvent += CheckForDupeSequence;
        lastSequencer = activeSequence.GetSerialzedData(AnAnnotationType.BikeOutOfLane);
    }

    void GoBack()
    {
        if(activeSequenceIndex > 0)
        {
            activeSequenceIndex--;
            stats.sequenceDatas.RemoveAt(stats.sequenceDatas.Count - 1);
            activeSequence = anomalySequencers[activeSequenceIndex];
        }
    }

    void CheckForDupeSequence(SerialzedSequenceData data)
    {
        if(activeSequenceIndex > 1)
        {
            bool inRange = false;
            if (data.StartFrame > lastSequencer.StartFrame && data.StartFrame < lastSequencer.EndFrame) inRange = true;
            if (data.EndFrame > lastSequencer.StartFrame && data.EndFrame < lastSequencer.EndFrame) inRange = true;
            if (inRange) dupeLabel.style.display = DisplayStyle.Flex;

            else dupeLabel.style.display = DisplayStyle.None;

            lastSequencer = data;
        }
    }
}
