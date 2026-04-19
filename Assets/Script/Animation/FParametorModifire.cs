using UnityEngine;
using System;

namespace AnimationParametorObserver
{
    public class SpeedModify : AnimationParametorModify
    {
        private Rigidbody rb;

        public SpeedModify(Rigidbody rb)
        {
            this.rb = rb;
        }

        public float GetFloatParametor()
        {
            Vector3 v = rb.linearVelocity;
            v.y = 0f;

            return v.magnitude;
        }
    }

    public class CrochModify : AnimationParametorModify
    {
        private BoxCollider col;

        // 立ち状態の高さ
        private float standHeight;

        // しゃがみ状態の高さ
        private float crouchHeight;

        public CrochModify(BoxCollider col, float standHeight, float crouchHeight)
        {
            this.col = col;
            this.standHeight = standHeight;
            this.crouchHeight = crouchHeight;
        }

        public float GetFloatParametor()
        {
            if (col == null) return 0f;

            float currentHeight = col.bounds.size.y;

            float range = standHeight - crouchHeight;
            if (range <= 0f) return 0f;

            float t = (currentHeight - crouchHeight) / range;

            return Mathf.Clamp01(t);
        }
    }
}