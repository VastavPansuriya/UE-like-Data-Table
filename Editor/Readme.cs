using System;
using UnityEngine;

public class Readme : ScriptableObject
{
    public const string title = "Data Table";
    public Section[] sections; 
    public bool loadedLayout;

    [Serializable]
    public class Section
    {
        public string heading;
        [TextArea(2,10)]
        public string[] text;
        public string linkText;
        public string url;
    }
}
