using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Plugins.Alg;
using CGTespy.UI;
using GameLib;
using ResourcesHelper;
using UnityEngine;
using UnityEngine.Assertions;

public class DebugLayout : MonoBehaviour
{
    public GameObject PrefabLayout;

    public PrefabHolder WidgetsPrefabs;

    [Range(0, 100)]
    public float Gap;

    private GameObject _debugLayout;

    void Awake()
    {
        _debugLayout = Instantiate(PrefabLayout, transform.parent);
        _debugLayout.gameObject.name = PrefabLayout.name;
    }

    void Start()
    {
        PrepareLayouts();
        DestroyImmediate(gameObject);
    }

    private void PrepareLayouts()
    {
        foreach (Transform screenTransform in transform)
        {
            if (!screenTransform.gameObject.activeSelf)
            {
                var layout = GetLayotInGUI(screenTransform);
                DestroyImmediate(layout.gameObject);
                continue;
            }

            foreach (Transform frameTransform in screenTransform)
            {
                if (!frameTransform.gameObject.activeSelf)
                {
                    continue;
                }

                Dictionary<Direction2D.RelativeDirection, float> stackPointers = new Dictionary<Direction2D.RelativeDirection, float>();
                var frame = GetFrameInGUI(frameTransform);
                var frameWidth = frame.rect.width;
                var frameHeight = frame.rect.height;

                var elements = frameTransform.gameObject.GetComponents<DebugLayoutElement>();

                Dictionary<Direction2D.RelativeDirection, float> poleSize = new Dictionary<Direction2D.RelativeDirection, float>();
                foreach (var debugLayoutElement in elements)
                {
                    if (!debugLayoutElement.enabled)
                        continue;
                    if (debugLayoutElement.Side == Direction2D.RelativeDirection.Left ||
                        debugLayoutElement.Side == Direction2D.RelativeDirection.Right ||
                        debugLayoutElement.Side == Direction2D.RelativeDirection.Up ||
                        debugLayoutElement.Side == Direction2D.RelativeDirection.Down ||
                        debugLayoutElement.Side == Direction2D.RelativeDirection.Center
                    )
                    {
                        var isVerticalPole = debugLayoutElement.Side == Direction2D.RelativeDirection.Left ||
                                             debugLayoutElement.Side == Direction2D.RelativeDirection.Right ||
                                             debugLayoutElement.Side == Direction2D.RelativeDirection.Center;

                        if (!poleSize.ContainsKey(debugLayoutElement.Side))
                            poleSize[debugLayoutElement.Side] = 0;

                        poleSize[debugLayoutElement.Side] +=
                            isVerticalPole ? debugLayoutElement.Size.y + Gap : debugLayoutElement.Size.x + Gap;
                    }
                }

                foreach (var key in poleSize.Keys.ToList())
                {
                    var isVerticalPole =
                        key == Direction2D.RelativeDirection.Left ||
                        key == Direction2D.RelativeDirection.Right ||
                        key == Direction2D.RelativeDirection.Center;
                    poleSize[key] -= Gap;
                    poleSize[key] /= isVerticalPole ? frameHeight : frameWidth;
                }

                foreach (var debugLayoutElement in elements)
                {
                    if (!debugLayoutElement.enabled)
                        continue;

                    if (!stackPointers.TryGetValue(debugLayoutElement.Side, out var stackPointer))
                    {
                        stackPointer = GetInitialStackPointer(debugLayoutElement.Side, poleSize);
                    }

                    // create new element
                    Assert.IsNotNull(frame);
                    var newElement = InstantiateWidget(debugLayoutElement, frame);
                    var newElementTransform = newElement.transform as RectTransform;
                    newElementTransform.SetPivot(Direction2D.ToTextAnchor(debugLayoutElement.Side));


                    var elementSizeNormalized = new Vector2(
                        debugLayoutElement.Size.x / frameWidth,
                        debugLayoutElement.Size.y / frameHeight);
                    var gapSizeNormalized = new Vector2(Gap / frame.rect.width, Gap / frame.rect.height);

                    stackPointer = MoveStackPointer(
                        stackPointer,
                        debugLayoutElement.Side,
                        elementSizeNormalized,
                        gapSizeNormalized);
                    stackPointers[debugLayoutElement.Side] = stackPointer;

                    SetAnchors(newElementTransform, stackPointer, elementSizeNormalized, gapSizeNormalized, debugLayoutElement.Side);

                    newElementTransform.offsetMin = Vector2.zero;
                    newElementTransform.offsetMax = Vector2.zero;

                    // initialize
                    var dbgLayoutElement = newElement.GetComponent<DebugLayoutElement>();
                    Assert.IsNotNull(dbgLayoutElement);
                    dbgLayoutElement.InitializeState();
                    dbgLayoutElement.LoadState();
                    dbgLayoutElement.RestoreFromState();
                }
            }
        }
    }

    private void SetAnchors(RectTransform rectTransform, float stackPointer, Vector2 elementSizeNormalized, Vector2 gapSizeNormalized, Direction2D.RelativeDirection side)
    {
        switch (side)
        {
            // move down:
            case Direction2D.RelativeDirection.LeftUp:
            case Direction2D.RelativeDirection.Left:
                rectTransform.anchorMin = new Vector2(gapSizeNormalized.x, stackPointer);
                break;
            case Direction2D.RelativeDirection.UpRight:
            case Direction2D.RelativeDirection.Right:
                rectTransform.anchorMin = new Vector2(1f - gapSizeNormalized.x - elementSizeNormalized.x, stackPointer);
                break;
            case Direction2D.RelativeDirection.Center:
                rectTransform.anchorMin = new Vector2(0.5f - elementSizeNormalized.x * 0.5f, stackPointer);
                break;

            // move up:
            case Direction2D.RelativeDirection.DownLeft:
                rectTransform.anchorMin = new Vector2(gapSizeNormalized.x, stackPointer - elementSizeNormalized.y);
                break;

            case Direction2D.RelativeDirection.RightDown:
                rectTransform.anchorMin = new Vector2(1f - gapSizeNormalized.x - elementSizeNormalized.x, stackPointer - elementSizeNormalized.y);
                break;


            // move right:
            case Direction2D.RelativeDirection.Down:
                rectTransform.anchorMin = new Vector2(stackPointer - elementSizeNormalized.x, gapSizeNormalized.y);
                break;
            case Direction2D.RelativeDirection.Up:
                rectTransform.anchorMin = new Vector2(stackPointer - elementSizeNormalized.x, 1f - gapSizeNormalized.y - elementSizeNormalized.y);
                break;

            default:
                throw new NotImplementedException();
        }
        rectTransform.anchorMax = rectTransform.anchorMin + elementSizeNormalized;
    }

    private float GetInitialStackPointer(Direction2D.RelativeDirection side, Dictionary<Direction2D.RelativeDirection, float> poleSize)
    {
        switch (side)
        {
            // move down:
            case Direction2D.RelativeDirection.LeftUp:
            case Direction2D.RelativeDirection.UpRight:
                return 1.0f;

            // move up:
            case Direction2D.RelativeDirection.DownLeft:
            case Direction2D.RelativeDirection.RightDown:
                return 0f;

            // vertical pole, move down:
            case Direction2D.RelativeDirection.Left:
            case Direction2D.RelativeDirection.Right:
            case Direction2D.RelativeDirection.Center:
                return (1f - poleSize[side]) * 0.5f + poleSize[side];

            // horizontal pole, move right:
            case Direction2D.RelativeDirection.Up:
            case Direction2D.RelativeDirection.Down:
                return (1f - poleSize[side]) * 0.5f;

            default:
                throw new NotImplementedException(side.ToString());
        }
    }

    private float MoveStackPointer(float pointer, Direction2D.RelativeDirection side, Vector2 elementSizeNormalized, Vector2 gapSizeNormalized)
    {
        switch (side)
        {
            // move down:
            case Direction2D.RelativeDirection.LeftUp:
            case Direction2D.RelativeDirection.UpRight:
            case Direction2D.RelativeDirection.Right:
            case Direction2D.RelativeDirection.Left:
            case Direction2D.RelativeDirection.Center:
                return pointer - gapSizeNormalized.y - elementSizeNormalized.y;

            // move up:
            case Direction2D.RelativeDirection.DownLeft:
            case Direction2D.RelativeDirection.RightDown:
                return pointer + gapSizeNormalized.y + elementSizeNormalized.y;

            // move right:
            case Direction2D.RelativeDirection.Up:
            case Direction2D.RelativeDirection.Down:
                return pointer + gapSizeNormalized.x + elementSizeNormalized.x;

            default:
                throw new NotImplementedException();
        }
    }

    private GameObject InstantiateWidget(DebugLayoutElement elem, RectTransform parent)
    {
        GameObject gObj = null;
        var prefab = (GameObject)WidgetsPrefabs.Prefabs[elem.GetPrefabBasedOnName()];
        Assert.IsNotNull(prefab, $"Can't find prefab {elem.GetPrefabBasedOnName()}");
        gObj = Instantiate(prefab, parent);

        gObj.CopyComponent(elem);
        gObj.name = elem.GetType().Name;
        return gObj;
    }

    private RectTransform GetLayotInGUI(Transform markupLayout)
    {
        var layoutName = markupLayout.name;
        var guiLayout = _debugLayout.transform.Find(layoutName);
        Assert.IsNotNull(guiLayout);
        return guiLayout as RectTransform;
    }

    private RectTransform GetFrameInGUI(Transform markupFrame)
    {
        var layoutName = markupFrame.parent.name;
        var frameName = markupFrame.gameObject.name;

        var guiLayout = _debugLayout.transform.Find(layoutName);
        Assert.IsNotNull(guiLayout, $"Can't find {layoutName}");

        var guiFrame = guiLayout.Find(frameName);
        Assert.IsNotNull(guiFrame, $"Can't find {frameName}");

        return guiFrame as RectTransform;
    }

}
