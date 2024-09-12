using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class NameGenerator
    {
        public static string GenerateName()
        {
            string[] adjectives = new string[] { "Brave", "Clever", "Daring", "Eager", "Fearless", "Gallant", "Humble", "Intrepid", "Jolly", "Kind", "Loyal", "Mighty", "Noble", "Optimistic", "Proud", "Quiet", "Resolute", "Steadfast", "True", "Unwavering", "Valiant", "Wise", "Xenial", "Youthful", "Zealous",
                "Adventurous", "Bold", "Courageous", "Determined", "Energetic", "Fierce", "Generous", "Heroic", "Inventive", "Joyful", "Keen", "Luminous", "Majestic", "Nimble", "Observant", "Persistent", "Quick", "Radiant", "Strong", "Tenacious", "Unique", "Vibrant", "Witty", "Xtraordinary", "Youthful", "Zealous" };
            
            string[] nouns = new string[] { "Aardvark", "Badger", "Cheetah", "Dolphin", "Elephant", "Falcon", "Giraffe", "Hawk", "Iguana", "Jaguar", "Kangaroo", "Lion", "Mongoose", "Narwhal", "Ocelot", "Penguin", "Quokka", "Raccoon", "Sloth", "Tiger", "Uakari", "Vulture", "Walrus", "Xerus", "Yak", "Zebra",
                "Antelope", "Beaver", "Cobra", "Dragon", "Eagle", "Frog", "Gazelle", "Hedgehog", "Ibex", "Jackal", "Koala", "Lynx", "Marmot", "Newt", "Octopus", "Panda", "Quail", "Raven", "Salamander", "Tortoise", "Urchin", "Viper", "Wombat", "Xenops", "Yak", "Zebu" };

            string adjective = adjectives[Random.Range(0, adjectives.Length)];
            string noun = nouns[Random.Range(0, nouns.Length)];
            
            return $"{adjective}_{noun}";
        }
    }
}
