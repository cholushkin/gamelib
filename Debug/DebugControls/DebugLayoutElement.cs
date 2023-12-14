using GameLib.Alg;
using UnityEngine;
using UnityEngine.Assertions;
    
namespace GameLib.Dbg
{
    // base class for all widgets
    public abstract class DebugLayoutElement : MonoBehaviour
    {
        public Direction2D.RelativeDirection Side;
        public Vector2Int Size;
        public Vector2 Position { get; set; }
        protected string _controlStateRepresentation;
        private const string DebugLayoutElementKeyPrefix = "DbgLayout.";

        public virtual void Update()
        {

        }
        
        public virtual void InitializeState()
        {
            Assert.IsTrue(Direction2D.GetConnectionsCount(Side) < 2, "Only one direction supported");
        }

        public virtual void RestoreFromState()
        {

        }

        public virtual void SaveToState()
        {

        }   

        public void LoadState()
        {
            _controlStateRepresentation = PlayerPrefs.GetString(GetUniqueControlStateName());
        }

        public void SaveState()
        {
            if(string.IsNullOrEmpty(_controlStateRepresentation))
                return;
            Debug.Log($"Saving {GetUniqueControlStateName()} control state");
            PlayerPrefs.SetString(GetUniqueControlStateName(), _controlStateRepresentation);

        }

        protected string GetUniqueControlStateName()
        {
            return DebugLayoutElementKeyPrefix + transform.GetDebugName(false, true);
        }

        public virtual void OnClick()
        {

        }

        public abstract string GetPrefabBasedOnName();
    }
}
