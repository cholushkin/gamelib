using System;
using UnityEngine;

public class DebugActivator : MonoBehaviour
{
	// todo: implement other activation strategy
	[Flags]
	public enum ActivationStrategy
	{
		NoTriggering = 0,
		KeyboardTextTyping = 1, // type "dbgconsole"
		KeyboardTilde = 2,
		FourTouchInCorner = 4,
		ProcessShortcuts = 8
	}

	public class EventDebugOverlayActivate
	{
		public int LayoutIndex = -1; // -1 = not specified. Open the recent 
		public int OverlayIndex = -1; // -1 = not specified. Open the recent 
	}

	public string TriggerName;

	public ActivationStrategy ActivateOnStrat;


	void Update()
	{
		if (ActivateOnStrat == ActivationStrategy.NoTriggering)
			return;

		if (IsStrategySet(ActivationStrategy.KeyboardTilde))
		{
			if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				Trigger();
				return;
			}
		}

		if (IsStrategySet(ActivationStrategy.FourTouchInCorner))
		{
			const int touchCount = 4;
			const int threshold = 100;

			if (Input.touchCount == touchCount)
			{
				Touch[] touches = Input.touches;

				if (touches.Length == touchCount)
				{
					var cnt = 0;
					for (var i = 0; i < touchCount; ++i)
						if (touches[i].position is { x: < threshold, y: < threshold })
							cnt++;
					if (cnt == touchCount)
					{
						Trigger();
						return;
					}
				}
			}
		}

		if (IsStrategySet(ActivationStrategy.ProcessShortcuts))
		{
			void HandleDigitInput(int key)
			{
				if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
					Trigger(-1, key);
				else
					Trigger(key, -1);
			}

			if (Input.GetKeyDown(KeyCode.Alpha0))
				HandleDigitInput(0);
			else if (Input.GetKeyDown(KeyCode.Alpha1))
				HandleDigitInput(1);
			else if (Input.GetKeyDown(KeyCode.Alpha2))
				HandleDigitInput(2);
			else if (Input.GetKeyDown(KeyCode.Alpha3))
				HandleDigitInput(3);
			else if (Input.GetKeyDown(KeyCode.Alpha4))
				HandleDigitInput(4);
			else if (Input.GetKeyDown(KeyCode.Alpha5))
				HandleDigitInput(5);
			else if (Input.GetKeyDown(KeyCode.Alpha6))
				HandleDigitInput(6);
			else if (Input.GetKeyDown(KeyCode.Alpha7))
				HandleDigitInput(7);
			else if (Input.GetKeyDown(KeyCode.Alpha8))
				HandleDigitInput(8);
			else if (Input.GetKeyDown(KeyCode.Alpha9))
				HandleDigitInput(9);
		}
	}


	void Trigger(int layoutIndex = -1, int overlayIndex = -1)
	{
		Debug.Log($"Trigger debug overlays {layoutIndex} {overlayIndex}");
		GlobalEventAggregator.Publish(new EventDebugOverlayActivate());
	}


	bool IsStrategySet(ActivationStrategy strat)
	{
		return (ActivateOnStrat & strat) != 0;
	}
}
