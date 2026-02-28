using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace dokoniru_counter
{
    public class Manager : UdonSharpBehaviour
    {
        [Header("設置したコライダーをD&D（使わない枠は削除）")]
        [SerializeField] private GameObject[] colliders;
        [Header("人数情報を表示させたいボードをD&D（使わない枠は削除）")]
        [SerializeField] private Board[] boards;

        // プレイヤーの情報を格納するための配列
        VRCPlayerApi[] players;

        private int[] counts;

        private const int maxCount = 80;

        private const int countInterval = 2;

        private const float offsetHeight = 0.1f;


        private void OnEnable()
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && colliders[i].activeSelf)
                {
                    colliders[i].SetActive(false);
                }
            }

            PrepareDisplay();
            StartCount();
        }


        private void PrepareDisplay()
        {
            for (int i = 0; i < boards.Length; i++)
            {
                if (boards[i] != null)
                {
                    boards[i].SetName(colliders);
                }
            }
        }


        private void StartCount()
        {
            if (players == null)
            {
                players = new VRCPlayerApi[maxCount];
            }

            if (counts == null)
            {
                counts = new int[colliders.Length];
            }

            UpdateCount();
        }


        public void UpdateCount()
        {
            // プレイヤーの情報を格納
            VRCPlayerApi.GetPlayers(players);
            // インスタンス内の全プレイヤーの数を数える
            int total = VRCPlayerApi.GetPlayerCount();

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == null)
                {
                    counts[i] = -1;
                    continue;
                }
                else
                {
                    int tmpCount = 0;

                    for (int j = 0; j < total; j++)
                    {
                        if (players[j] == null || !players[j].IsValid()) { continue; }

                        if (IsInCollider(colliders[i].transform, players[j]))
                        {
                            tmpCount++;
                        }
                    }

                    counts[i] = tmpCount;
                }
            }

            for (int i = 0; i < boards.Length; i++)
            {
                if (boards[i] != null)
                {
                    boards[i].UpdateDisplay(counts, total);
                }
            }

            SendCustomEventDelayedSeconds(nameof(UpdateCount), countInterval);
        }


        private bool IsInCollider(Transform collider, VRCPlayerApi player)
        {
            Vector3 colliderPos = collider.position;
            Quaternion colliderRot = collider.rotation;
            Vector3 colliderSize = collider.localScale;

            Vector3 playerPos = player.GetPosition();

            // ワールド空間のプレイヤー座標をコライダーのローカル空間に変換
            Vector3 localPlayerPos = Quaternion.Inverse(colliderRot) * (playerPos - colliderPos);
            //Debug.Log(localPlayerPos);

            // ローカル空間でAABBチェック（y座標は少し上を基準に）
            return Mathf.Abs(localPlayerPos.x) <= colliderSize.x / 2 &&
                   Mathf.Abs(localPlayerPos.y + offsetHeight) <= colliderSize.y / 2 &&
                   Mathf.Abs(localPlayerPos.z) <= colliderSize.z / 2;
        }
    }
}