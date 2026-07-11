using Alchemy.Editor;
using Alchemy.Editor.Drawers;
using UnityEditor;
using UnityEngine.UIElements;

[CustomAttributeDrawer(typeof(RequiredAttribute))]
public sealed class RequiredDrawer : TrackSerializedObjectAttributeDrawer
{
    private Label _label;
    private string _originalText;

    public override void OnCreateElement()
    {
        if (SerializedProperty.propertyType != SerializedPropertyType.ObjectReference)
            return;

        base.OnCreateElement();

        TargetElement.schedule.Execute(() =>
        {
            _label = TargetElement.Q<Label>();

            if (_label != null)
            {
                _originalText = _label.text;
                UpdateLabel();
            }
        });
    }

    protected override void OnInspectorChanged()
    {
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (_label == null)
            return;

        bool missing = SerializedProperty.objectReferenceValue == null;

        _label.text = missing
            ? $"⚠ {_originalText}"
            : _originalText;
    }
}