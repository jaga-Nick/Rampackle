using UnityEngine;
using InGame.Model;
using InGame.View;

namespace InGame.Presenter
{
    public class PlayerPresenter: MonoBehaviour
    {
            // --- Inspectorで設定する項目 ---
        [Header("車の性能")]
        [SerializeField] private float _acceleration = 500f;
        [SerializeField] private float _maxSpeed = 50f;
        [SerializeField] private float _turnStrength = 180f;
        [SerializeField] private float _driftMultiplier = 1.5f;

        [Header("参照")]
        // ViewコンポーネントをInspectorからアタッチする
        [SerializeField] private PlayerView _view;

        // --- 内部で管理するインスタンス ---
        private PlayerModel _model;
        private Rigidbody _rigidbody; // Viewから取得したRigidbodyをキャッシュする

        void Awake()
        {
            // 1. Modelを生成し、Inspectorで設定した性能値を渡す
            _model = new PlayerModel(_acceleration, _maxSpeed, _turnStrength, _driftMultiplier);

            // 2. ViewからRigidbodyを取得してキャッシュしておく
            //    (FixedUpdateの度にGetComponentするのは非効率なため)
            if (_view != null)
            {
                _rigidbody = _view.GetRigidbody();
            }
            else
            {
                Debug.LogError("CarViewがアタッチされていません！");
            }
        }

        void Update()
        {
            // ユーザー入力を毎フレーム検知する
            // バックは無し
            float throttleInput = Input.GetKey(KeyCode.W) ? 1f : 0f;

            // A/Dキーで左右の入力を取得
            float steerInput = 0f;
            if (Input.GetKey(KeyCode.A))
            {
                steerInput = -1f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                steerInput = 1f;
            }

            // Shiftキーでドリフト状態を検知
            bool isDrifting = Input.GetKey(KeyCode.LeftShift);

            // 検知した入力値をModelにセットする
            _model.SetThrottleInput(throttleInput);
            _model.SetSteerInput(steerInput);
            _model.SetDriftInput(isDrifting);
        }

        void FixedUpdate()
        {
            // 物理演算の前に実行される
            if (_model == null || _view == null || _rigidbody == null) return;

            // 1. 現在の速度を計算してModelに渡す
            //    (車のローカル座標系での前方速度を取得)
            float currentSpeed = Vector3.Dot(_rigidbody.linearVelocity, _view.transform.forward);

            // 2. Modelに力の計算を依頼する
            _model.CalculateMovement(currentSpeed);

            // 3. Modelから計算結果を取得する
            float force = _model.GetForwardForce();
            float torque = _model.GetTurnTorque();

            // 4. Viewに物理的な挙動を依頼する
            _view.ApplyMovement(force, torque);
        }
    }
}