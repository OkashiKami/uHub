using System;
using UnityEngine;
using UnityEngine.Events;

namespace uHub.Utils
{
    using uHub.Entity;

    [Serializable]
    public class Health
    {
        public float curHealth = 100;
        public float maxHealth = 100;
        public float regenerateSpeed = 0.1f;

        internal void Init(Player parent)
        {
            this.parent = parent;
            foreach (Collider c in parent.GetComponentsInChildren<Collider>())
            {
                Collider pc = parent.GetComponent<Collider>();
                if (c != pc)
                {
                    c.enabled = false;
                }
                pc.enabled = true;
            }
        }

        public bool Damage20;
        public UnityEvent onHeal;
        public UnityEvent onDamage;

        public void Update()
        {
            // if (curHealth / maxHealth < 0.75f) curHealth += .05f * regenerateSpeed * Time.deltaTime;
            if (curHealth > maxHealth) curHealth = maxHealth;
            if (curHealth < 0) curHealth = 0;
            if (Damage20)
            {
                DoDamage(20);
                Damage20 = false;
            }
        }

        public UnityEvent onDeath;
        internal Player parent;

        public void DoHeal(int amount)
        {
            if (curHealth < maxHealth)
            {
                onHeal?.Invoke();
                curHealth += amount;
            }
        }
        public void DoDamage(int amount)
        {
            if (curHealth > 0)
            {
                onDamage?.Invoke();
                curHealth -= amount;
            }
            else DoDeath();
            if (curHealth <= 0) DoDeath();
        }
        public void DoDeath()
        {
            foreach (Collider c in parent.GetComponentsInChildren<Collider>())
            {
                Collider pc = parent.GetComponent<Collider>();
                if (c != pc)
                {
                    c.enabled = true;
                }
                pc.enabled = true;
            }
            parent.GetComponent<Animator>().enabled = false;
            onDeath?.Invoke();
        }
    }
}