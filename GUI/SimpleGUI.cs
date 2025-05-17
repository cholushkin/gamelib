using System;
using System.Collections.Generic;
using System.Linq;
using GameLib;
using GameLib.Log;
using UnityEngine;
using UnityEngine.Assertions;
using VitalRouter;

namespace GameGUI
{
    [ScriptExecutionOrder(-8)]
    public class SimpleGUI : MonoBehaviour
    {
        public interface IInitialize
        {
            void Initialize();
        }
        public string StartingScreenName;
        public Transform ScreensRoot;
        public GUIScreenBase[] Screens;
        public LogChecker Log;
        

        private readonly Stack<GUIScreenBase> _screenStack = new Stack<GUIScreenBase>(); // note: _screenStack is a logical representation, and the hierarchy is current state of the screens


        void Awake()
        {
            if (ScreensRoot == null)
                ScreensRoot = transform;

            foreach (var init in GetComponentsInChildren<IInitialize>(true))
                init.Initialize();

            // disable screens 
            foreach (Transform child in ScreensRoot)
                child.gameObject.SetActive(false);

            Router.Default.Subscribe<DebugWidgetSimpleGUI.EventRetrieveSimpleGUIInstance>(Handle);
        }

        void Start()
        {
            // activating default screen
            if (string.IsNullOrEmpty(StartingScreenName))
                return;
            if (Log.Normal())
                Debug.LogFormat("Activating starting screen {0}", StartingScreenName);
            PushScreen(StartingScreenName);
        }


        // return current active screen
        public GUIScreenBase GetCurrentScreen()
        {
            if (_screenStack.Count == 0)
                return null;
            return _screenStack.Peek();
        }

        // todo:
        // public int PopScreenUntil(string untilScreen)


        public int PopAll()
        {
            var count = _screenStack.Count;
            while (_screenStack.Count > 0)
            {
                var popped = _screenStack.Pop();
                popped.DisappearForced();

            }
            return count;
        }

        // pops current screen and returns it
        public GUIScreenBase PopScreen(string expectedScreenToPop = null)
        {
            var screenOnTop = GetCurrentScreen();
            GUIScreenBase popped;
            if (screenOnTop == null)
            {
                Debug.LogError($"Nothing to pop {expectedScreenToPop}");
                return null;
            }

            if (expectedScreenToPop != null && (expectedScreenToPop != screenOnTop.name))
            {
                Debug.LogError($"Expecting to pop screen '{expectedScreenToPop}', but got '{screenOnTop.name}'");
                return null;
            }

            popped = _screenStack.Pop();
            screenOnTop.StartDisappearAnimation();
            
            Assert.IsTrue(popped == screenOnTop);
            return popped;
        }

        internal void OnScreenPopped(GUIScreenBase screen)
        {
            if (screen.IsModal)
                ModalNotify(screen, false);

            screen.OnPopped();
            GetCurrentScreen()?.OnRestore();

            // hide or die
            if (screen.IsDestroyOnPop)
                Destroy(screen.gameObject);
            else
                screen.gameObject.SetActive(false);
        }

        public void PushScreen<T>(string screenName, Action<T> passOptionsCallback) where T : GUIScreenBase
        {
            var screenTr = ObtainScreen(screenName);
            var screenCasted = screenTr.gameObject.GetComponent<T>();

            passOptionsCallback(screenCasted);

            PushScreenInternal(screenCasted);
        }
                                                            
        public void PushScreen(string screenName)
        {
            // obtain screen
            var screenTransform = ObtainScreen(screenName);
            Assert.IsNotNull(screenTransform, "SimpleGUI: couldn't obtain screen: " + screenName);
            var screen = screenTransform.gameObject.GetComponent<GUIScreenBase>();
            PushScreenInternal(screen);
        }

        private void PushScreenInternal(GUIScreenBase screen)
        {
            screen.gameObject.SetActive(true);
            screen.OnPushed();

            // push it on the top of the tree
            screen.transform.SetAsLastSibling();
            screen.StartAppearAnimation();
            _screenStack.Push(screen);

            // notify all active screens in case if current screen is a modal screen
            if (screen.IsModal)
                ModalNotify(screen, true);
        }

        private void ModalNotify(GUIScreenBase screen, bool isUnderModal)
        {
            // note: we rely on visual instead of logic( the stack) because blocking and unblocking other screens 
            // happens according to visual of the screen, while they are still on the screen
            var foundSelf = false;
            for (int i = ScreensRoot.childCount - 1; i >= 0; --i)
            {
                var s = ScreensRoot.GetChild(i).gameObject;
                var baseScreen = s.GetComponent<GUIScreenBase>();
                if (baseScreen == screen) // starting from the screen that call ModalNotify
                {
                    foundSelf = true;
                    continue;
                }
                if (!foundSelf) // skip screens that are above 
                    continue;
                if (!s.activeSelf) // skip disabled (disabled == dead)
                    continue;

                baseScreen.OnBecomeUnderModal(isUnderModal); // notify
                if (baseScreen.IsModal) // below the first modal is already being controlled by that modal, so stop notifying
                    return;
            }
        }

        private Transform ObtainScreen(string screenName)
        {
            // try to reuse screen, find screen in children of current SimpleGUI
            var screenTransform = ScreensRoot.Find(screenName);

            // create new by name
            if (screenTransform == null)
            {
                var screenPrefab = Screens.FirstOrDefault(n => screenName == n.name);
                screenPrefab.gameObject.SetActive(false);
                Assert.IsNotNull(screenPrefab, "SimpleGUI: can't find screen named " + screenName);
                screenTransform = Instantiate(screenPrefab.transform, ScreensRoot);
                screenTransform.name = screenName;
            }
            else
            {
                Assert.IsTrue(screenTransform.GetComponent<GUIScreenBase>() != null, "");
                Assert.IsFalse(screenTransform.GetComponent<GUIScreenBase>().IsInTransaction, "");
                Assert.IsFalse(screenTransform.gameObject.activeSelf);
            }
            return screenTransform;
        }

        [ContextMenu("DbgPrintStack")]
        void DbgPrintStack()
        {
            Debug.Log(">>>>> _screenStack");
            foreach (var guiScreenBase in _screenStack)
            {
                Debug.LogFormat("  >Screen name:{0}, IsModal:{1}, IsInputEnabled:{2}, IsInTransaction:{3}, inputEnabledRefs:{4}",
                    guiScreenBase.name,
                    guiScreenBase.IsModal,
                    guiScreenBase.IsInputEnabled,
                    guiScreenBase.IsInTransaction,
                    guiScreenBase.GetRefsCount());
            }
        }

        public string DbgGetStackString()
        {
            var str = $"GUI stack[{_screenStack.Count}]\n";
            foreach (var guiScreenBase in _screenStack)
                str += $"*{guiScreenBase.name}, modal:{guiScreenBase.IsModal}, input:{guiScreenBase.IsInputEnabled}, inTransaction:{guiScreenBase.IsInTransaction}, inputEnabledRefs:{guiScreenBase.GetRefsCount()}\n";
            return str;
        }

        private void Handle(DebugWidgetSimpleGUI.EventRetrieveSimpleGUIInstance eventRetrieveSimpleGUIInstance, PublishContext context)
        {
            eventRetrieveSimpleGUIInstance.Instance = this;
        }
    }
}