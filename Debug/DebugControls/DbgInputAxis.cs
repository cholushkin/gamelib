using CGTespy.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace GameLib.Dbg
{
    public class DbgInputAxis : DebugLayoutElement
    {
        public string AxisName1;
        public string AxisName2;

        protected Text _textAxis1;
        protected Text _textAxis2;

        protected Image _imagePointerAxis;
        protected Image _imagePointerRaw;
        protected Image _imagePointerCircled;


        public override string GetPrefabBasedOnName()
        {
            return "DbgInputAxis";
        }

        public override void InitializeState()
        {
            base.InitializeState();

            _textAxis1 = transform.Find("TextAxis1").GetComponent<Text>();
            _textAxis2 = transform.Find("TextAxis2").GetComponent<Text>();
            _imagePointerAxis = transform.Find("ImagePointerAxis").GetComponent<Image>();
            _imagePointerRaw = transform.Find("ImagePointerRaw").GetComponent<Image>();
            _imagePointerCircled = transform.Find("ImagePointerCircled").GetComponent<Image>();

            Assert.IsNotNull(_textAxis1);
            Assert.IsNotNull(_textAxis2);
            Assert.IsNotNull(_imagePointerAxis);
            Assert.IsNotNull(_imagePointerRaw);
            Assert.IsNotNull(_imagePointerCircled);
        }

        public override void Update()
        {
            base.Update();
            _textAxis1.text = $"{AxisName1}:\n{Input.GetAxis(AxisName1):0.00}";
            _textAxis2.text = $"{AxisName2}:\n{Input.GetAxis(AxisName2):0.00}";

            var w = GetComponent<RectTransform>().Width() / 2;
            var h = GetComponent<RectTransform>().Height() / 2;

            _imagePointerAxis.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                Input.GetAxis(AxisName1) * w,
                Input.GetAxis(AxisName2) * h);

            _imagePointerRaw.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                Input.GetAxisRaw(AxisName1) * w,
                Input.GetAxisRaw(AxisName2) * h);


            var xf = Input.GetAxis(AxisName1);
            var yf = Input.GetAxis(AxisName2);
            var normDirection = new Vector2(xf, yf).normalized;
            var maxx = normDirection
                .x; // projection of x component to horizontal axis (maximum value with a sign for curent direction)
            var maxy = normDirection.y;

            _imagePointerCircled.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                maxx * Mathf.Abs(xf) * w, // Mathf.Abs(xf) here is percent of propagation on current axis
                maxy * Mathf.Abs(yf) * h);
        }
    }
}