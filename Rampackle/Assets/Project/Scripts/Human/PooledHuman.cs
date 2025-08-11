using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

/// <summary>
/// プールされたオブジェクトが、自身の元のプレハブを記憶するためのコンポーネント
/// </summary>
public class PooledHuman : MonoBehaviour
{
    // フィールドは外部から直接アクセスできないようにprivateにする
    private GameObject _prefabOrigin;

    private Rigidbody _rb;

    /// <summary>
    /// 元のプレハブを設定する（Setter）
    /// </summary>
    public void SetPrefabOrigin(GameObject prefab)
    {
        _prefabOrigin = prefab;
    }

    /// <summary>
    /// 元のプレハブを取得する（Getter）
    /// </summary>
    public GameObject GetPrefabOrigin()
    {
        return _prefabOrigin;
    }

    public void FlyAway()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = false; // Bật lại vật lý
        _rb.useGravity = true; // Đảm bảo có trọng lực
        _rb.constraints = RigidbodyConstraints.None; // Gỡ bỏ mọi ràng buộc

        // 
        Vector3 launchForce = new Vector3(
            UnityEngine.Random.Range(-2f, 2f),  // Một chút lực ngang
            UnityEngine.Random.Range(4f, 8f), // Lực hướng lên trời mạnh
            UnityEngine.Random.Range(-2f, 2f)   // Một chút lực ngang
        );

        _rb.AddForce(launchForce, ForceMode.Impulse);

        // 
        Vector3 flipTorque = new Vector3(
            UnityEngine.Random.Range(10f, 20f),  // Xoay quanh trục X
            UnityEngine.Random.Range(-6f, 6f),  // Một chút xoay ngang
            UnityEngine.Random.Range(-20f, -30f) // Xoay mạnh để giật ngửa về sau
        );

        _rb.AddTorque(flipTorque, ForceMode.Impulse);
    }

    public async UniTask ReleaseHuman()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(2f));
        HumanManager.Instance.ReturnHuman(this.gameObject);
    }
        
}