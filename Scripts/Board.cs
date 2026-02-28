
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace dokoniru_counter
{
    public class Board : UdonSharpBehaviour
    {
        [SerializeField] private int max = 0;
        [SerializeField] private TextMeshProUGUI tName;
        [SerializeField] private TextMeshProUGUI tCount;
        [SerializeField] private TextMeshProUGUI tTotal;


        public void SetName(GameObject[] colliders)
        {
            if (tName == null) { return; }

            tName.text = string.Empty;
            tCount.text = string.Empty;

            for (int i = 0; i < max; i++)
            {
                if (i < colliders.Length && colliders[i] != null)
                {
                    tName.text += $"{colliders[i].name}　\n";
                }
                else
                {
                    tName.text += "　\n";
                }
            }
        }


        public void UpdateDisplay(int[] counts, int countOfAllPlayers)
        {
            if (tCount == null) { return; }

            tCount.text = string.Empty;

            for (int i = 0; i < max; i++)
            {
                if (i < counts.Length && counts[i] >= 0)
                {
                    tCount.text += $"　{counts[i]}\n";
                }
                else
                {
                    tCount.text += "　\n";
                }
            }
            tTotal.text = $"{countOfAllPlayers}";
        }
    }
}