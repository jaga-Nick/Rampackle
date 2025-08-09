// Unityの機能に依存しない、純粋なロジッククラス

namespace InGame.Model
{
    public class PlayerModel
    {
        // --- 車の基本性能 ---
        private readonly float _acceleration;
        private readonly float _maxSpeed;
        private readonly float _turnStrength;
        private readonly float _driftMultiplier;
    
        // --- 現在の入力状態 ---
        private float _throttleInput;
        private float _steerInput;
        private bool _isDrifting;
    
        // --- 計算結果 ---
        private float _currentForwardForce;
        private float _currentTurnTorque;
    
        /// <summary>
        /// コンストラクタで車の性能を初期化
        /// </summary>
        public PlayerModel(float acceleration, float maxSpeed, float turnStrength, float driftMultiplier)
        {
            _acceleration = acceleration;
            _maxSpeed = maxSpeed;
            _turnStrength = turnStrength;
            _driftMultiplier = driftMultiplier;
        }
    
        // --- 入力値を設定するアクセサメソッド ---
    
        public void SetThrottleInput(float input)
        {
            _throttleInput = input;
        }
    
        public void SetSteerInput(float input)
        {
            _steerInput = input;
        }
    
        public void SetDriftInput(bool isDrifting)
        {
            _isDrifting = isDrifting;
        }
    
        // --- 計算ロジック ---
    
        /// <summary>
        /// 現在の速度と入力値に基づいて、適用すべき力とトルクを計算する
        /// </summary>
        /// <param name="currentSpeed">現在の車の前方速度</param>
        public void CalculateMovement(float currentSpeed)
        {
            // アクセル入力があり、かつ最高速に達していない場合のみ力を計算
            if (currentSpeed < _maxSpeed)
            {
                _currentForwardForce = _throttleInput * _acceleration;
            }
            else
            {
                _currentForwardForce = 0f;
            }
    
            // ドリフト中は旋回能力を向上させる
            float turnMultiplier = _isDrifting ? _driftMultiplier : 1f;
            _currentTurnTorque = _steerInput * _turnStrength * turnMultiplier;
        }
    
        // --- 計算結果を取得するアクセサメソッド ---
    
        public float GetForwardForce()
        {
            return _currentForwardForce;
        }
    
        public float GetTurnTorque()
        {
            return _currentTurnTorque;
        }
    }
}
