/*
Radial Layout Group by Just a Pixel (Danny Goodayle) - http://www.justapixel.co.uk
Copyright (c) 2015

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

      actual website the code is found is http://www.justapixel.co.uk/2015/09/14/radial-layouts-nice-and-simple-in-unity3ds-ui-system/
      GitHub Repo: https://gist.github.com/DGoodayle/aa62a344aa83e5342175


      Goodayle, D [Just a Pixel] (2015) Radial Layout Group [Source code]. http://www.justapixel.co.uk
*/
// For our Arc-type, we used Danny Goodayle's radial layout group, with the
// necessary changes made to accommodate to our design needs.

// Created by: Danny Goodayle
// Modified by: Zahara M. Spilka
// Date Created: 2015
// Date Modified Updated: 07/29/2020

namespace UnityEngine.UI
{
  //This adds the layout to the layout group.
    [AddComponentMenu("Layout/Circular Layout Group", 155)]


    public class CircularLayout : LayoutGroup
    {
      // This section sets up user/proctor display aspects of the layout.
      // fDistance is the distance between bins, the value constraint by Unity’s
      // limits. Likewise, the user/proctor can set the layout's minimum angle,
      // maximum angle, and starting angle as any value between 0 and 360. The
      // user/proctor can also set the layout to be the only viable one, however
      // this feature is obsolete due to the nature of our interface's design. I
      // left it in the file, in the event that this not obsolete when it comes
      // to the CalculateCircular function assigning the letter bin's positions
      // around the circular layout.


        public float fDistance;
        [Range(0f, 360f)]
        public float MinAngle, MaxAngle, StartAngle;
        public bool OnlyLayoutVisible = false;

        // The following functions, depending on their name, call the
        // CaclualeCirlcle function

        protected override void OnEnable()
        {
            base.OnEnable();
            CalculateCircular();
        }
        public override void SetLayoutHorizontal()
        {
        }
        public override void SetLayoutVertical()
        {
        }
        public override void CalculateLayoutInputHorizontal()
        {
            CalculateCircular();
        }
        public override void CalculateLayoutInputVertical()
        {
            CalculateCircular();

        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            CalculateCircular();
        }
#endif

// When the file calls this function, it checks if the layout has any children,
// then assigns the number of children to the integer variable,
// ChildrenToFormat. Next, the function sets the angle offset value; the layout
// uses the angle offset value to evenly spaced in the bins. Then, the function
// sets the initial value of the current angle, fAngle, equaled to the start
// angle. Next the function looks for the virtual stylus and assigns its
// position as described below. Lastly, the function runs through the bins,
// assigning their positions and rotation values along the layout.

        void CalculateCircular()
        {
            m_Tracker.Clear();
            if (transform.childCount == 0)
                return;
            int ChildrenToFormat = 0;
            // if (OnlyLayoutVisible)
            // {
            //     for (int i = 0; i < transform.childCount; i++)
            //     {
            //         RectTransform child = (RectTransform)transform.GetChild(i);
            //         if ((child != null) && child.gameObject.activeSelf)
            //             ++ChildrenToFormat;
            //     }
            // }
            // else
            // {
                ChildrenToFormat = transform.childCount;
            // }

            float fOffsetAngle = (MaxAngle - MinAngle) / (transform.childCount - 1);

            float fAngle = StartAngle;


            for (int i = 0; i < transform.childCount; i++)
            {
              // Because the first child of the layout is a virtual stylus, the
              // function finds and assigns the game object to this child. Then,
              // the function sets the child's position to be the center of the
              // circular layout.

                if (i == 0)
                {
                    // Debug.LogWarning("Child is Stylus");
                    GameObject child = GameObject.FindGameObjectWithTag("CircularStylus");
                    child.transform.position = transform.position;
                }
                else
                {
                    RectTransform child = (RectTransform)transform.GetChild(i);
                    // if ((child != null) && (!OnlyLayoutVisible || child.gameObject.activeSelf))
                    if ((child != null) && (child.gameObject.activeSelf))
                    {
                        // Adding the elements to the tracker stops the user
                        // from modifying their positions via the editor.


                        // This also sets the bins up so that they are at an
                        // angle around the layout. For example, on the middle
                        // bin, the function sets the z rotation so that it
                        // appears horizontal along the z axis, making it
                        // parallel with the virtual stylus’s starting position.

                        m_Tracker.Add(this, child,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.Pivot);
                        Vector3 vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
                        child.localPosition = vPos * fDistance;
                        Quaternion vRot = Quaternion.Euler(0, 0, fAngle - 90);
                        child.localRotation = vRot;

                        //Force objects to be center aligned, this can be
                        // changed however I'd suggest you keep all of the
                        // objects with the same anchor points.
                        child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);
                        fAngle += fOffsetAngle;
                    }
                }
            }
        }
    }
}
