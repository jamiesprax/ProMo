using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spraxware.Util {
    public class Util {

        public static float SmoothStart(float t) {
            return t * t;
        }

        public static float SmoothStart2(float t) {
            return t * t * t;
        }
        public static float SmoothStart3(float t) {
            return t * t * t * t;
        }

        public static float SmoothStop(float t) {
            return 1 - SmoothStart(1 - t);
        }
    }
}
