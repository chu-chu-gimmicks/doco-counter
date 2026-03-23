
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ChuChuGimmicks.DocoCounter
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Manager : UdonSharpBehaviour
    {
        [SerializeField] private bool isDebug = false;
        private void ChuDebug(string message)
        {
            if (isDebug)
            {
                Debug.Log(message);
            }
        }


        [SerializeField] private GameObject[] colliders;
        [SerializeField] private Board[] boards;

        private const float COUNT_INTERVAL_SECONDS = 2.0f;
        private const int MAX_COUNT = 100;
        private const float HEIGHT_OFFSET = 0.1f;

        private bool isInitialized = false;
        private bool isPending = false;

        private int[] counts;
        private int totalCount = 0;

        // コライダーのワールド座標系からローカル座標系への変換用行列
        private Matrix4x4[] colliderMatrices;
        // コライダーの中心座標（球を用いた大まかな距離計算用）
        private Vector3[] colliderPositions;
        // コライダーを覆う球の半径の2乗（球を用いた大まかな距離計算用）
        private float[] colliderRadiusSq;

        VRCPlayerApi[] players = new VRCPlayerApi[MAX_COUNT];
        private Vector3[] playerPositions = new Vector3[MAX_COUNT];




        private void OnEnable()
        {
            if (isPending) { return; }
            isPending = true;

            if (!Utilities.IsValid(colliders) || colliders.Length == 0) { return; }
            if (!Utilities.IsValid(boards) || boards.Length == 0) { return; }

            if (!isInitialized)
            {
                InitializeOnce();
                isInitialized = true;
            }

            CountPeriodically();
        }


        private void InitializeOnce()
        {
            if (isInitialized) { return; }

            counts = new int[colliders.Length];

            int colCount = colliders.Length;
            colliderMatrices = new Matrix4x4[colCount];
            colliderPositions = new Vector3[colCount];
            colliderRadiusSq = new float[colCount];
            for (int i = 0; i < colCount; i++)
            {
                if (!Utilities.IsValid(colliders[i])) { continue; }

                Transform colTransform = colliders[i].transform;
                colliderMatrices[i] = colTransform.worldToLocalMatrix;
                colliderPositions[i] = colTransform.position;
                // ピタゴラスの定理により、コライダーを覆う球の半径の2乗を算出
                Vector3 scale = colTransform.localScale;
                colliderRadiusSq[i] = (scale.x * scale.x + scale.y * scale.y + scale.z * scale.z) * 0.25f;
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                if (isDebug) { break; }
                if (!Utilities.IsValid(colliders[i])) { continue; }
                if (!colliders[i].activeSelf) { continue; }

                colliders[i].SetActive(false);
            }

            InitializeBoard();
        }


        private void InitializeBoard()
        {
            for (int i = 0; i < boards.Length; i++)
            {
                if (!Utilities.IsValid(boards[i])) { continue; }
                boards[i].SetName(colliders);
            }
        }


        public void CountPeriodically()
        {
            // 非アクティブ時はカウント処理を行わず、次回分の予約もしない
            if (this.gameObject.activeInHierarchy)
            {
                SendCustomEventDelayedSeconds(nameof(CountPeriodically), COUNT_INTERVAL_SECONDS);
            }
            else
            {
                isPending = false;
                return;
            }

            // プレイヤー情報を更新
            VRCPlayerApi.GetPlayers(players);
            totalCount = VRCPlayerApi.GetPlayerCount();
            for (int i = 0; i < totalCount; i++)
            {
                if (!Utilities.IsValid(players[i]) || !players[i].IsValid()) { continue; }
                Vector3 pos = players[i].GetPosition();
                pos.y += HEIGHT_OFFSET;
                playerPositions[i] = pos;
            }

            // 各コライダー内のプレイヤー数をカウント
            for (int i = 0; i < counts.Length; i++)
            {
                counts[i] = GetPlayerCount(i);
            }

            // UIを更新
            UpdateBoard(counts, totalCount);
        }


        private int GetPlayerCount(int cIdx)
        {
            if (!Utilities.IsValid(colliders[cIdx])) { return -1; }

            int count = 0;

            for (int pIdx = 0; pIdx < totalCount; pIdx++)
            {
                if (!Utilities.IsValid(players[pIdx]) || !players[pIdx].IsValid()) { continue; }

                if (IsInCollider(cIdx, pIdx))
                {
                    count++;
                    if (players[pIdx].isLocal)
                    {
                        count += 1000;
                    }
                }
            }

            return count;
        }


        private bool IsInCollider(int colIndex, int playerIndex)
        {
            Vector3 playerPos = playerPositions[playerIndex];

            // コライダーを覆う球を用いた大まかな判定
            Vector3 colPos = colliderPositions[colIndex];
            Vector3 delta = playerPos - colPos;
            if (delta.sqrMagnitude > colliderRadiusSq[colIndex])
            {
                ChuDebug("OUTSIDE SPHERE");
                return false;
            }

            // プレイヤーのワールド座標をコライダーのローカル座標に変換することによる厳密な判定
            Vector3 localPos = colliderMatrices[colIndex].MultiplyPoint3x4(playerPos);
            if (Mathf.Abs(localPos.x) >= 0.5f ||
                Mathf.Abs(localPos.y) >= 0.5f ||
                Mathf.Abs(localPos.z) >= 0.5f)
            {
                ChuDebug("OUTSIDE COLLIDER");
                return false;
            }

            ChuDebug("INSIDE COLLIDER");
            return true;
        }


        private void UpdateBoard(int[] counts, int totalCount)
        {
            for (int i = 0; i < boards.Length; i++)
            {
                if (!Utilities.IsValid(boards[i])) { continue; }
                boards[i].UpdateUI(counts, totalCount);
            }
        }
    }
}
