using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ChuChuGimmicks.DokoCounter
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Manager : UdonSharpBehaviour
    {
        [SerializeField] private GameObject[] colliders;
        [SerializeField] private Board[] boards;

        private const int MAX_COUNT = 100;
        private const float INTERVAL = 2.0f;
        private const float HEIGHT_OFFSET = 0.1f;

        VRCPlayerApi[] players = new VRCPlayerApi[MAX_COUNT];
        private int[] counts = null;
        private bool isPending = false;




        private void OnEnable()
        {
            if (isPending) { return; }
            isPending = true;

            for (int i = 0; i < colliders.Length; i++)
            {
                if (!Utilities.IsValid(colliders[i])) { continue; }
                if (!colliders[i].activeSelf) { continue; }

                colliders[i].SetActive(false);
            }

            PrepareBoard();
            StartCount();
        }


        private void PrepareBoard()
        {
            for (int i = 0; i < boards.Length; i++)
            {
                if (!Utilities.IsValid(boards[i])) { continue; }
                boards[i].SetName(colliders);
            }
        }


        private void StartCount()
        {
            if (!Utilities.IsValid(counts))
            {
                counts = new int[colliders.Length];
            }

            UpdateCount();
        }


        public void UpdateCount()
        {
            if (this.gameObject.activeInHierarchy)
            {
                // 次の更新を予約
                SendCustomEventDelayedSeconds(nameof(UpdateCount), INTERVAL);
            }
            else
            {
                isPending = false;
                return;
            }

            // プレイヤーの情報を格納
            VRCPlayerApi.GetPlayers(players);
            // プレイヤー数を格納
            int playerCount = VRCPlayerApi.GetPlayerCount();

            for (int i = 0; i < colliders.Length; i++)
            {
                if (!Utilities.IsValid(colliders[i]))
                {
                    counts[i] = -1;
                    continue;
                }
                else
                {
                    int count = 0;

                    for (int j = 0; j < playerCount; j++)
                    {
                        if (!players[j].IsValid()) { continue; }

                        if (IsInCollider(colliders[i].transform, players[j]))
                        {
                            count++;
                        }
                    }

                    counts[i] = count;
                }
            }

            // UIを更新
            for (int i = 0; i < boards.Length; i++)
            {
                if (!Utilities.IsValid(boards[i])) { continue; }
                boards[i].UpdateUI(counts, playerCount);
            }
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
                   Mathf.Abs(localPlayerPos.y + HEIGHT_OFFSET) <= colliderSize.y / 2 &&
                   Mathf.Abs(localPlayerPos.z) <= colliderSize.z / 2;
        }
    }
}