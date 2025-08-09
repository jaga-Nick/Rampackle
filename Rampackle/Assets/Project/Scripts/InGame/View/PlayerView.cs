using UnityEngine;

namespace InGame.View
{
    public class PlayerView: MonoBehaviour
    {
        private Rigidbody _rigidbody;

        // Presenterが初期設定時にRigidbodyを取得するために使用
        public Rigidbody GetRigidbody()
        {
            // _rigidbodyがnullの場合のみ取得処理を行い、負荷を減らす
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }
            return _rigidbody;
        }

        /// <summary>
        /// Presenterからの指示で車に物理的な力を加える
        /// </summary>
        /// <param name="force">前進させる力</param>
        /// <param name="torque">回転させる力（トルク）</param>
        public void ApplyMovement(float force, float torque)
        {
            // 自身の前方に力を加える
            GetRigidbody().AddRelativeForce(Vector3.forward * force);

            // Y軸周りにトルクを加えて回転させる
            GetRigidbody().AddRelativeTorque(Vector3.up * torque);
        }
    }
}