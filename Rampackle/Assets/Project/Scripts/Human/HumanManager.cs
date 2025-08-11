using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HumanManager : MonoBehaviour
{
    // シングルトンインスタンス：どこからでもアクセスできるようにする
    public static HumanManager Instance { get; private set; }

    [Header("プレイヤーオブジェクト")]
    [SerializeField, Tooltip("Humanをスポーンさせる中心となるプレイヤー")]
    private Transform playerTransform;

    [Header("プールするHumanのプレハブ配列")]
    [SerializeField] private GameObject[] humanPrefabs;

    [Header("プール設定")]
    [SerializeField, Tooltip("種類ごとに最初に生成しておく数")]
    private int initialPoolSize = 10;

    [Header("スポーン設定")]
    [SerializeField, Tooltip("Humanをスポーンさせる間隔（秒）")]
    private float spawnInterval = 1.0f; // 動作確認しやすいように少し短くしました

    [SerializeField, Tooltip("プレイヤーからの最小スポーン距離")]
    private float spawnMinRadius = 20f;
    [SerializeField, Tooltip("プレイヤーからの最大スポーン距離")]
    private float spawnMaxRadius = 40f;

    // オブジェクトプールの実体
    private Dictionary<GameObject, Queue<GameObject>> _pool = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // プレイヤーが設定されていなければ、タグで探す（任意）
        if (playerTransform == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("プレイヤーが見つかりません。HumanManagerにPlayerTransformを設定してください。");
            }
        }

        InitializePool();
    }

    private void Start()
    {
        // プレイヤーが設定されている場合のみスポーンループを開始
        if (playerTransform != null)
        {
            SpawnHumansLoop().Forget();
        }
    }

    /// <summary>
    /// ゲーム開始時にプールを初期化する
    /// </summary>
    private void InitializePool()
    {
        foreach (var prefab in humanPrefabs)
        {
            var objectQueue = new Queue<GameObject>();
            for (int i = 0; i < initialPoolSize; i++)
            {
                var newHuman = Instantiate(prefab);
                newHuman.AddComponent<PooledHuman>().SetPrefabOrigin(prefab);
                newHuman.SetActive(false);
                objectQueue.Enqueue(newHuman);
            }
            _pool.Add(prefab, objectQueue);
        }
    }

    /// <summary>
    /// 定期的にHumanをスポーンさせる非同期ループ
    /// </summary>
    private async UniTask SpawnHumansLoop()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        while (true)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval), cancellationToken: ct);
            if (ct.IsCancellationRequested) break;

            SpawnHuman(); // スポーン処理を呼び出す
        }
    }

    /// <summary>
    /// プレイヤーの周囲にHumanを一体スポーンさせる
    /// </summary>
    private void SpawnHuman()
    {
        // プレイヤーが設定されていなければ何もしない
        if (playerTransform == null) return;

        // ランダムな方向と距離を決める
        float randomAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad; // 360度のランダムな角度（ラジアン）
        float randomRadius = UnityEngine.Random.Range(spawnMinRadius, spawnMaxRadius); // minとmaxの間のランダムな距離

        // 方向と距離から、プレイヤーを中心とした座標を計算する
        // X座標 = cos(角度) * 半径
        // Z座標 = sin(角度) * 半径
        Vector3 spawnOffset = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle)) * randomRadius;
        Vector3 spawnPosition = playerTransform.position + spawnOffset;

        // Y座標はプレイヤーと同じ高さにする（必要に応じて変更）
        spawnPosition.y = playerTransform.position.y;

        // プールからHumanを取得して配置する
        // ここでは向きはランダムにせず、デフォルトの向き（Quaternion.identity）とする
        // ランダムにしたい場合は Quaternion.Euler(0, Random.Range(0, 360), 0) などを使用
        GetHuman(spawnPosition, Quaternion.identity);
    }

    /// <summary>
    /// プールからランダムな種類のHumanを一つ取り出し、指定した位置・回転で配置する
    /// </summary>
    public GameObject GetHuman(Vector3 position, Quaternion rotation)
    {
        if (humanPrefabs.Length == 0)
        {
            Debug.LogWarning("HumanManagerにプレハブが登録されていません。");
            return null;
        }

        GameObject selectedPrefab = humanPrefabs[UnityEngine.Random.Range(0, humanPrefabs.Length)];

        if (_pool.TryGetValue(selectedPrefab, out var queue))
        {
            GameObject humanToSpawn;
            if (queue.Count > 0)
            {
                humanToSpawn = queue.Dequeue();
            }
            else
            {
                humanToSpawn = Instantiate(selectedPrefab);
                humanToSpawn.AddComponent<PooledHuman>().SetPrefabOrigin(selectedPrefab);
            }

            humanToSpawn.transform.position = position;
            humanToSpawn.transform.rotation = rotation;
            humanToSpawn.SetActive(true);

            return humanToSpawn;
        }

        Debug.LogError($"プレハブ {selectedPrefab.name} のプールが見つかりません。");
        return null;
    }

    /// <summary>
    /// 使用済みのHumanをプールに戻す
    /// </summary>
    public void ReturnHuman(GameObject human)
    {
        var pooledHuman = human.GetComponent<PooledHuman>();
        if (pooledHuman == null || pooledHuman.GetPrefabOrigin() == null)
        {
            Debug.LogWarning($"プール管理外のオブジェクト {human.name} が返却されようとしました。Destroyします。");
            Destroy(human);
            return;
        }

        if (_pool.TryGetValue(pooledHuman.GetPrefabOrigin(), out var queue))
        {
            human.SetActive(false);
            queue.Enqueue(human);
        }
        else
        {
            Debug.LogError($"プレハブ {pooledHuman.GetPrefabOrigin().name} のプールが見つかりません。オブジェクトをDestroyします。");
            Destroy(human);
        }
    }
}