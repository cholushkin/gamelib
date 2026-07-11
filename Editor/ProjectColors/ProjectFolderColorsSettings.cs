using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLib
{
    [CreateAssetMenu(
        fileName = "ProjectFolderColors",
        menuName = "GameLib/Project Folder Colors")]
    public class ProjectFolderColorsSettings : ScriptableObject
    {
        public List<Rule> Rules = new();

        [Serializable]
        public class Rule
        {
            [Tooltip("Apply this color to folders whose name starts with this text.")]
            public string Wildcard;

            public Color Color = Color.white;
        }
    }
}