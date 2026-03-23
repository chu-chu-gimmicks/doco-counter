
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ChuChuGimmicks.DocoCounter
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Board : UdonSharpBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image[] icons;
        [SerializeField] private Sprite[] iconSprites;

        [SerializeField] private TMPro.TextMeshProUGUI[] nameTexts;
        [SerializeField] private TMPro.TextMeshProUGUI[] countTexts;
        [SerializeField] private TMPro.TextMeshProUGUI totalCountText;




        public void SetName(GameObject[] colliders)
        {
            if (!Utilities.IsValid(nameTexts) || nameTexts.Length == 0) { return; }
            if (!Utilities.IsValid(colliders) || colliders.Length == 0) { return; }

            for (int i = 0; i < nameTexts.Length; i++)
            {
                if (!Utilities.IsValid(nameTexts[i])) { continue; }

                if (i < colliders.Length)
                {
                    if (!Utilities.IsValid(colliders[i])) { continue; }
                    nameTexts[i].text = colliders[i].name;
                }
                else
                {
                    nameTexts[i].text = string.Empty;
                }
            }
        }


        public void UpdateUI(int[] counts, int totalCount)
        {
            if (!Utilities.IsValid(countTexts) || countTexts.Length == 0) { return; }
            if (!Utilities.IsValid(counts)     || counts.Length == 0)     { return; }

            for (int i = 0; i < countTexts.Length; i++)
            {
                if (!Utilities.IsValid(countTexts[i])) { return; }

                if (i >= 0 && i < counts.Length)
                {
                    if (!Utilities.IsValid(counts[i])) { continue; }

                    bool isLocalPlayerHere = counts[i] >= 1000;

                    int count = isLocalPlayerHere ? counts[i] % 1000 : counts[i];
                    countTexts[i].text = $"{count}";

                    int spriteIdx = isLocalPlayerHere ? 1 : 0;
                    if (icons[i].sprite != iconSprites[spriteIdx])
                    {
                        icons[i].sprite = iconSprites[spriteIdx];
                    }
                }
                else
                {
                    countTexts[i].text = string.Empty;
                }
            }

            totalCountText.text = $"{totalCount}";
        }
    }
}