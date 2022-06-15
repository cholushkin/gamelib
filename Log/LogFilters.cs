using UnityEngine;

namespace GameLib.Log
{
    [CreateAssetMenu(fileName = "LogFilters", menuName = "Create LogFilters", order = 1)]
    public class LogFilters : ScriptableObject
    {
        public string[] PassFilters;
        public string[] RejectFilters;
    }
}