﻿using UnityEngine;
using System.Collections.Generic;

namespace MinVR
{
    /**
	 * Genertates fake VREvents for non-Unity built-in inputs (like the events from Optitrak or the buttons on the stylus).
	 * This is only for use during debugging on your laptop. Make sure to not to use the script when you deploy your app!
	 * 
	 * To make debugging VR apps easier, you can use the mouse and keyboard to create 'fake' input for two trackers.
	 * "Press the '1' or '2' key to switch between controlling tracker1 or tracker2. Move the mouse around the screen
	 * to move the 3D position of that tracker within a plane parallel to the screen.  Hold down 'left shift' while
	 * moving the mouse vertically to change the 3D depth. Hold 'x', 'y', or 'z' while moving the mouse horizontally
	 * to rotate the tracker around the X, Y, or Z axis. 
	 */
    public class FakeTrackingInput : MonoBehaviour, VREventGenerator
    {
        [Tooltip("Fake head tracking with arrow keys. 'up' moves forward, 'down' moves backward, 'left' rotates left, 'right' rotates right.")]
        public string fakeHeadTrackerEvent = "Head_Move";

        public Vector3 initialHeadPos = new Vector3(0, 1, -2);
        private Vector3 headTrackerPos;

        public Vector3 initialHeadRot = new Vector3();
        private Quaternion headTrackerRot;

        [Tooltip("The name of the VREvent generated by the first fake tracker.")]
        public string fakeTracker1Event = "LHand_Move";

        public Vector3 initialTracker1Pos = new Vector3(0, 0, 0);
        private Vector3 tracker1Pos;
        public Vector3 initialTracker1Rot = new Vector3();
        private Quaternion tracker1Rot;

        [Tooltip("The name of the VREvent generated by the second fake tracker.")]
        public string fakeTracker2Event = "RHand_Move";

        public Vector3 initialTracker2Pos = new Vector3(0, 0, 0);
        private Vector3 tracker2Pos;
        public Vector3 initialTracker2Rot = new Vector3();
        private Quaternion tracker2Rot;

        private int curTracker = 0;
        private float lastx = float.NaN;
        private float lasty = float.NaN;


        void Start()
        {
            VRMain.Instance.AddEventGenerator(this);

            headTrackerPos = initialHeadPos;
            headTrackerRot = Quaternion.Euler(initialHeadRot);
            tracker1Pos = initialTracker1Pos;
            tracker1Rot = Quaternion.Euler(initialTracker1Rot);
            tracker2Pos = initialTracker2Pos;
            tracker2Rot = Quaternion.Euler(initialTracker1Rot);
        }

        public void AddEventsSinceLastFrame(ref List<VREvent> eventList)
        {
            AddHeadTrackerEvent(ref eventList);
            AddTrackerEvents(ref eventList);
        }

        private void AddHeadTrackerEvent(ref List<VREvent> eventList)
        {
            // if (Input.GetKey(KeyCode.LeftShift))
            // {
            //     if (Input.GetKey(KeyCode.DownArrow))
            //     {
            //         headTrackerRot *= Quaternion.AngleAxis(-1.0f, new Vector3(1f, 0f, 0f));
            //     }
            //     else if (Input.GetKey(KeyCode.UpArrow))
            //     {
            //         headTrackerRot *= Quaternion.AngleAxis(1.0f, new Vector3(1f, 0f, 0f));
            //     }
            // }
            // else 
            if (Input.GetKey(KeyCode.UpArrow))
            {
                headTrackerPos += 0.1f * Camera.main.transform.forward;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                headTrackerPos -= 0.1f * Camera.main.transform.forward;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                headTrackerRot *= Quaternion.AngleAxis(-1.0f, new Vector3(0f, 1f, 0f));
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                headTrackerRot *= Quaternion.AngleAxis(1.0f, new Vector3(0f, 1f, 0f));
            }

            Matrix4x4 m3 = Matrix4x4.TRS(headTrackerPos, headTrackerRot, Vector3.one);
            float[] d3 = VRConvert.ToFloatArray(m3);
            VREvent e = new VREvent(fakeHeadTrackerEvent);
            e.AddData("EventType", "TrackerMove");
            e.AddData("Transform", d3);
            eventList.Add(e);
        }


        private void AddTrackerEvents(ref List<VREvent> eventList)
        {
            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;
            // first time through
            if (float.IsNaN(lastx))
            {
                lastx = x;
                lasty = y;
                return;
            }

            if (Input.GetKeyDown("1"))
            {
                curTracker = 0;
            }
            else if (Input.GetKeyDown("2"))
            {
                curTracker = 1;
            }

            if (Input.GetKey("x"))
            {
                float angle = 0.1f * (x - lastx);
                if (curTracker == 0)
                {
                    tracker1Rot = Quaternion.AngleAxis(angle, new Vector3(1f, 0f, 0f)) * tracker1Rot;
                }
                else if (curTracker == 1)
                {
                    tracker2Rot = Quaternion.AngleAxis(angle, new Vector3(1f, 0f, 0f)) * tracker2Rot;
                }
            }
            else if (Input.GetKey("y"))
            {
                float angle = 0.1f * (x - lastx);
                if (curTracker == 0)
                {
                    tracker1Rot = Quaternion.AngleAxis(angle, new Vector3(0f, 1f, 0f)) * tracker1Rot;
                }
                else if (curTracker == 1)
                {
                    tracker2Rot = Quaternion.AngleAxis(angle, new Vector3(0f, 1f, 0f)) * tracker2Rot;
                }
            }
            else if (Input.GetKey("z"))
            {
                float angle = 0.1f * (x - lastx);
                if (curTracker == 0)
                {
                    tracker1Rot = Quaternion.AngleAxis(angle, new Vector3(0f, 0f, 1f)) * tracker1Rot;
                }
                else if (curTracker == 1)
                {
                    tracker2Rot = Quaternion.AngleAxis(angle, new Vector3(0f, 0f, 1f)) * tracker2Rot;
                }
            }
            else if (Input.GetKey("left shift"))
            {
                float depth = 0.005f * (y - lasty);
                if (curTracker == 0)
                {
                    tracker1Pos += depth * Camera.main.transform.forward;
                }
                else if (curTracker == 1)
                {
                    tracker2Pos += depth * Camera.main.transform.forward;
                }
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                float angle = 0.1f * (x - lastx);
                if (curTracker == 0)
                {
                    tracker1Rot = Quaternion.AngleAxis(angle, new Vector3(0f, 0f, 1f)) * tracker1Rot;
                }
                else if (curTracker == 1)
                {
                    tracker2Rot = Quaternion.AngleAxis(angle, new Vector3(0f, 0f, 1f)) * tracker2Rot;
                }

                angle = 0.1f * (y - lasty);
                if (curTracker == 0)
                {
                    tracker1Rot = Quaternion.AngleAxis(angle, new Vector3(1f, 0f, 0f)) * tracker1Rot;
                }
                else if (curTracker == 1)
                {
                    tracker2Rot = Quaternion.AngleAxis(angle, new Vector3(1f, 0f, 0f)) * tracker2Rot;
                }
            }
            else
            {
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0f));
                Plane p = new Plane();
                float dist = 0.0f;
                if (curTracker == 0)
                {
                    p.SetNormalAndPosition(-Camera.main.transform.forward, tracker1Pos);
                    if (p.Raycast(ray, out dist))
                    {
                        tracker1Pos = ray.GetPoint(dist);
                    }
                }
                else if (curTracker == 1)
                {
                    p.SetNormalAndPosition(-Camera.main.transform.forward, tracker2Pos);
                    if (p.Raycast(ray, out dist))
                    {
                        tracker2Pos = ray.GetPoint(dist);
                    }
                }

            }

            // for fake traker 1
            Matrix4x4 m1 = Matrix4x4.TRS(tracker1Pos, tracker1Rot, Vector3.one);
            float[] d1 = VRConvert.ToFloatArray(m1);
            VREvent e1 = new VREvent(fakeTracker1Event);
            e1.AddData("EventType", "TrackerMove");
            e1.AddData("Transform", d1);
            eventList.Add(e1);

            // for fake traker 2
            Matrix4x4 m2 = Matrix4x4.TRS(tracker2Pos, tracker2Rot, Vector3.one);
            float[] d2 = VRConvert.ToFloatArray(m2);
            VREvent e2 = new VREvent(fakeTracker2Event);
            e2.AddData("EventType", "TrackerMove");
            e2.AddData("Transform", d2);
            eventList.Add(e2);

            // 
            this.lastx = x;
            this.lasty = y;
        }

    }
}
// namespace MinVR
