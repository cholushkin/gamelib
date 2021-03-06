using System.Collections.Generic;
using Alg;
using UnityEngine;


namespace GameLib.Time
{
    public class TimeScaleStack : Singleton<TimeScaleStack>
    {
        Stack<float> _stack = new Stack<float>();
        List<int> _defferedPopings = new List<int>();

        public bool IsPaused
        {
            get { return _stack.Count > 0 ? Mathf.Approximately(_stack.Peek(), 0f) : Mathf.Approximately(UnityEngine.Time.timeScale, 0f); }
        }

        public int Push(float scale)
        {
            _stack.Push(scale);
            Apply(scale);
            return _stack.Count;
        }

        public bool IsEmpty()
        {
            return _stack.Count == 0;
        }

        public void Pop(int level)
        {
            if (level != _stack.Count)
            {
                Debug.LogWarning("TimeScaleStack: the pop level is not same stack top");
                _defferedPopings.Add(level);
            }
            else
            {
                _stack.Pop();
                PopDeffered();
                Apply(_stack.Count > 0 ? _stack.Peek() : 1f);
            }
        }

        private void Apply(float scale)
        {
            UnityEngine.Time.timeScale = scale;
        }

        private void PopDeffered()
        {
            bool poped = false;
            do
            {
                poped = false;
                for (int i = _defferedPopings.Count - 1; i > -1; i--)
                    if (_stack.Count == _defferedPopings[i])
                    {
                        _defferedPopings.RemoveAt(i);
                        _stack.Pop();
                        poped = true;
                        break;
                    }
            } while (poped);
        }
    }

}
