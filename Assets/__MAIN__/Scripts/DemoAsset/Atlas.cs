using UnityEngine;

namespace COL.UnityGameWheels.Demo
{
    public class Atlas : ScriptableObject
    {
        public Sprite[] Sprites;

        public Sprite GetSprite(string name)
        {
            foreach (var sprite in Sprites)
            {
                if (sprite.name == name)
                {
                    return sprite;
                }
            }

            return null;
        }
    }
}