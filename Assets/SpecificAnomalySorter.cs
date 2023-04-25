using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.IO;

public class SpecificAnomalySorter : AnomalyImageSorter
{
    
    private void Start()
    {
        UIDoc = GetComponent<UIDocument>();

        if (ResetStatsOnStartup) stats.ResetStats();

        PictureButton betterButton = new PictureButton(AnAnnotationType.Better, "bt-better");
        PictureButton newButton = new PictureButton(AnAnnotationType.New, "bt-new");
        PictureButton trashButton = new PictureButton(AnAnnotationType.Trash, "bt-trash");

        betterButton.onPressedEvent += SortPicture;
        newButton.onPressedEvent += SortPicture;
        trashButton.onPressedEvent += SortPicture;

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
    }
}
