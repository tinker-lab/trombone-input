using UnityEngine;

namespace CustomInput
{
    namespace Layout
    {
        public class TwoRotationABCDE : StylusBinnedABCDE
        {
            public override bool usesSlider => false;

            protected override int? InnerIndex(InputData data, int parentSize)
                => Utils.Static.NormalizedIntoIndex(1 - data.normalizedAngles.x, parentSize);
        }
    }
}