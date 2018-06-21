using System;
using UnityEngine;

namespace uHub.Entity
{
    using uHub.Utils;

    [Serializable]
    public class Player : MonoBehaviour
    {
        public Health HealthModule = new Health();

        private void Awake()
        {
            HealthModule.Init(this);
        }
        void Update()
        {
            HealthModule.Update();
        }
    }
}