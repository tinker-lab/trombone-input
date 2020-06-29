using System;
using MinVR;
using static UnityEngine.Input;
using static UnityEngine.KeyCode;
using VREventList = System.Collections.Generic.List<MinVR.VREvent>;

namespace CustomInput
{
    // In Unity Editor:
    //    From _MAIN.scene
    //      Left Click on recommendations, dropdown via screen to canvas coordinates (Unity Eventsystem)
    //    From Bindings.cs
    //      Right Click + Mouse Wheel   =>      Stylus Slider
    //      Backquote                   =>      Stylus Front Button
    //      Tab                         =>      Stylus Back Button
    //      7/8/9/0                     =>      Fast Switch Layouts
    //      Shift                       =>      Hold for Precise Emulation
    //      Return                      =>      Switch to/from Lobby
    //    From MainController.cs
    //      Backspace                   =>      Force Backspace
    //      Space                       =>      Force Space
    //    From FakeTrackingInput.cs
    //      Mouse Delta                 =>      Stylus XY Position Delta
    //      Left Control + Mouse Delta  =>      Stylus XZ Rotation Delta
    //      X/Y/Z + Mouse Delta         =>      Stylus Strictly X/Y/Z Rotation Delta
    //      Up/Down/Left/Right          =>      Main Camera Delta
    //
    // In XR Build:
    //    From StylusModelController.Raycast
    //      Right Trigger to select recommendations, dropdown via raycast from stylus tip
    //    From _MAIN.scene
    //      Head Delta                  =>      Main Camera Delta
    //      Hand Pos/Rot Delta          =>      Stylus Pos/Rot Delta
    //    From Bindings.cs 
    //     - <DOM> is dominant hand, set in Bindings.DOMINANT_HAND
    //     - Primary is Trackpad for HP WMR
    //     - Secondary is Joystick for HP WMR
    //      <DOM> Primary Y Position    =>      Stylus Slider
    //      <DOM> Secondary Button      =>      Stylus Front Button
    //      <DOM> Grip Button           =>      Stylus Back Button
    //      <DOM> Joystick Quadrant     =>      Fast Switch Layouts
    //      Return                      =>      Switch to/from Lobby
    public static class Bindings
    {
        public static readonly int _slider_max_value = 64;
        public static readonly UnityEngine.KeyCode[] _layout_switch_bindings
            = new UnityEngine.KeyCode[] { Alpha7, Alpha8, Alpha9, Alpha0 };

        public static bool _left_handed = false;
        public static string _dominant_hand
            => _left_handed ? "Left" : "Right"; // or "Left"
        public static string _trigger
            => $"XRI_{_dominant_hand}_TriggerButton";
        public static string _grip
            => $"XRI_{_dominant_hand}_GripButton";

        // Trackpad/Joystick labels based on HP WMR 1440^2
        public static string _trackpad_vertical
            => $"XRI_{_dominant_hand}_Primary2DAxis_Vertical";

        public static string _joystick_vertical
            => $"XRI_{_dominant_hand}_Secondary2DAxis_Vertical";
        public static string _joystick_horizontal
            => $"XRI_{_dominant_hand}_Secondary2DAxis_Horizontal";

        public static bool inputThisFrame
            => touchCount > 0
            || GetMouseButton(0)
            || emulatingSlide
            || precisionMode
            || emulatingFront
            || emulatingBack;

        // right mouse down
        public static bool beginEmulatedSlide
            => GetMouseButtonDown(1);

        // right mouse held and slide delta exists
        public static bool emulatingSlide
            => GetMouseButton(1) || GetAxis(_trackpad_vertical) != 0;

        // right mouse up
        public static bool endEmulatedSlide
            => GetMouseButtonUp(1);

        public static float emulatedSlideDelta
            => mouseScrollDelta.y * 2;

        public static int emulatedSlideValue
            => UnityEngine.Mathf.FloorToInt((1 + GetAxis(_trackpad_vertical)) * _slider_max_value / 2);

        // either shift key held
        public static bool precisionMode
            => GetKey(LeftShift) || GetKey(RightShift);

        public static bool emulatingFrontDown
            => GetKeyDown(BackQuote) || GetButtonDown(_trigger);

        public static bool emulatingFront
            => GetKey(BackQuote) || GetButton(_trigger);

        public static bool emulatingFrontUp
            => GetKeyUp(BackQuote) || GetButtonUp(_trigger);

        public static bool emulatingBackDown
            => GetKeyDown(Tab) || GetButtonDown(_grip);

        public static bool emulatingBack
            => GetKey(Tab) || GetButton(_grip);

        public static bool emulatingBackUp
            => GetKeyUp(Tab) || GetButtonUp(_grip);

        public static bool spaceDown
            => GetKeyDown(Space);

        public static bool backspaceDown
            => GetKeyDown(Backspace);

        public static bool advanceToMain
            => GetKeyDown(Return);

        public static bool returnToLobby
            => GetKeyDown(Return);

        public static int? emulatingLayoutSwitch
        {
            get
            {
                for (int i = 0; i < _layout_switch_bindings.Length; i++)
                {
                    if (GetKeyDown(_layout_switch_bindings[i]))
                    {
                        return i;
                    }
                }

                return joystickQuadrant - 1;
            }
        }

        public static int? joystickQuadrant
        {
            get
            {
                float vert = GetAxis(_joystick_vertical);
                float horz = GetAxis(_joystick_horizontal);
                if (vert > 0.3f)
                {
                    if (horz > 0.3f)
                    {
                        return 1;
                    }
                    if (horz < -0.3f)
                    {
                        return 2;
                    }
                    return null;
                }

                if (vert < -0.3f)
                {
                    if (horz > 0.3f)
                    {
                        return 4;
                    }
                    if (horz < -0.3f)
                    {
                        return 3;
                    }
                }

                return null;
            }
        }

        public static void InitializeMinVRLayoutSwitching(VRDevice server)
        {
            if (server.vrNodeType != VRDevice.VRNodeType.NetServer)
            {
                UnityEngine.Debug.LogWarning("Provided VRDevice to initialize for MinVR Layout switching does not have vrNodeType NetServer!");
            }
            foreach (var item in _layout_switch_bindings)
            {
                server.unityKeysToVREvents.Add(item.ToString());
            }
        }

        public static void AddMinVRLayoutSwitchingHandlers(Func<int, VRMain.OnVRButtonDownEventDelegate> LayoutHandlers)
        {
            for (int i = 0; i < _layout_switch_bindings.Length; i++)
            {
                var binding = _layout_switch_bindings[i];
                VRMain.Instance.AddOnVRButtonDownCallback($"Kbd{binding.ToString()}_Down", LayoutHandlers(i));
            }
        }


        // If emulatingFront or endEmulatedSlide when the layout accepts potentiometer input,
        // then it emulates the forward button down.
        // If emulatingBack then it emulates back button
        public static void CaptureEmulatedButtonInput(ref VREventList eventList, bool layoutUsesSlider)
        {
            if (emulatingFrontDown || (endEmulatedSlide && layoutUsesSlider))
            {
                eventList.Add(VREventFactory.MakeButtonDownEvent(VREventFactory._front_button_event_name));
            }

            if (emulatingFrontUp)
            {
                eventList.Add(VREventFactory.MakeButtonUpEvent(VREventFactory._front_button_event_name));
            }

            if (emulatingBackDown)
            {
                eventList.Add(VREventFactory.MakeButtonDownEvent(VREventFactory._back_button_event_name));
            }

            if (emulatingBackUp)
            {
                eventList.Add(VREventFactory.MakeButtonUpEvent(VREventFactory._back_button_event_name));
            }
        }


        // Starts, updates, and ends emulated slider input when appropriate
        // Also accounts for precision mode
        public static void CaptureEmulatedSliderInput(ref VREventList eventList, int slideStartValue, int? currentValue)
        {
            if (beginEmulatedSlide)
            {
                eventList.Add(VREventFactory.MakePotentiometerEvent(slideStartValue));
                return;
            }

            float delta = emulatedSlideDelta;
            if (!precisionMode)
            {
                delta *= 4;
            }

            int rawNext = UnityEngine.Mathf.RoundToInt(currentValue + delta ?? 0);
            int next = UnityEngine.Mathf.Clamp(rawNext, 0, _slider_max_value);

            if (delta == 0)
            {
                next = emulatedSlideValue;
            }

            if (emulatingSlide)
            {
                eventList.Add(VREventFactory.MakePotentiometerEvent(next));
            }
        }
    }

    public static class VREventFactory
    {
        public const string _potentiometer_event_name = "BlueStylusAnalog";
        public const string _front_button_event_name = "BlueStylusFrontBtn";
        public const string _back_button_event_name = "BlueStylusBackBtn";
        public const string _button_down_event_type = "ButtonDown";
        public const string _button_up_event_type = "ButtonUp";

        public static VREvent MakeButtonDownEvent(string name)
            => MakeEvent(name, _button_down_event_type);

        public static VREvent MakeButtonUpEvent(string name)
            => MakeEvent(name, _button_up_event_type);

        public static VREvent MakePotentiometerEvent(float analogValue)
            => MakeEvent(_potentiometer_event_name, "AnalogUpdate", analogValue);

        public static VREvent MakeEvent(string name, string type, float? analogValue = null)
        {
            VREvent e = new VREvent(name);
            e.AddData("EventType", type);

            if (analogValue.HasValue)
            {
                e.AddData("AnalogValue", analogValue.Value);
            }

            return e;
        }
    }
}