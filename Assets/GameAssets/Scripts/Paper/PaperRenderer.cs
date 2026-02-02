using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum OverrideType
{
    Text,
}
public class Override
{
    public string targetName;
    public OverrideType overrideType;
    public string stringOverride;

    public Override(string targetName, OverrideType overrideType, string stringOverride = "")
    {
        this.targetName = targetName;
        this.overrideType = overrideType;
        this.stringOverride = stringOverride;
    }
}
public class PaperRenderer : MonoBehaviour
{
    public static PaperRenderer active;

    [Serializable]
    public class OverrideEntry
    {
        public string name;
        public Transform objectRefrence;
    }
    [Serializable]
    public class Paper
    {
        public string name;
        public GameObject paperObject;
        public Vector2Int paperResolution;
        public List<OverrideEntry> overrideEntryList;   
        public OverrideEntry FindOverrideEntry(string name)
        {
            foreach (OverrideEntry entry in overrideEntryList)
            {
                if (entry.name.ToLower() == name.ToLower())
                {
                    return entry;
                }
            }

            Debug.LogWarning("Did not find override entry with name: " + name);
            return null;
        }
    }

    //Variables
    [SerializeField]
    private Camera renderCamera;
    [SerializeField]
    public List<Paper> paperList;

    private void Start()
    {
        active = this;
    }

    public Texture2D RenderPaper(string paperName,List<Override> overrideList)
    {
        Paper paper = GetPaper(paperName);
        if (paper == null)
        {
            Debug.LogWarning("No Paper");
            return null;
        }

        //Handle overrides
        foreach (Override overrideInfo in overrideList)
        {
            OverrideEntry overrideEntry = paper.FindOverrideEntry(overrideInfo.targetName);
            if (overrideEntry != null)
            {
                if (overrideInfo.overrideType == OverrideType.Text)
                    overrideEntry.objectRefrence.GetComponent<TMP_Text>().text = overrideInfo.stringOverride;
            }
            else
            {
                Debug.LogWarning("Override skipped: " + overrideInfo.targetName);
            }
        }

        //Render
        paper.paperObject.SetActive(true);

        RenderTexture renderTexture = new RenderTexture(paper.paperResolution.x, paper.paperResolution.y,16);
        //renderTexture.Create();

        renderCamera.targetTexture = renderTexture;
        renderCamera.forceIntoRenderTexture = true;
        RenderTexture.active = renderTexture;
        renderCamera.Render();

        Texture2D texture = new Texture2D(paper.paperResolution.x, paper.paperResolution.y);
        texture.ReadPixels(new Rect(0, 0, paper.paperResolution.x, paper.paperResolution.y), 0, 0);
        texture.Apply();

        paper.paperObject.SetActive(false);

        return texture;
    }

    private Paper GetPaper(string paperName)
    {
        foreach (Paper paper in paperList)
        {
            if(paper.name.ToLower() == paperName.ToLower())
            {
                return paper;
            }
        }

        Debug.LogWarning("Did not find paper with name: " + paperName);
        return null;
    }
}
