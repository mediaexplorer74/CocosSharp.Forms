using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CocosSharp;

namespace tests
{
    public class ZoomFlipXRightOver : CCTransitionZoomFlipX
    {
        public ZoomFlipXRightOver (float t, CCScene s) : base(t, s, CCTransitionOrientation.RightOver)
        { }

    }
}
