using GOAP;
using IA2;
using UnityEngine;

namespace Managers
{
    public enum WeaponType
    {
        Sword,
        Explosive,
    };

    public class PlayerState : MonoBehaviour
    {
        private int _playerLife;
        [SerializeField, Range(0, 100)] private int maxPlayerLife;
    
        private float _playerMoney;

        [SerializeField] private WeaponType weaponType;
        private WeaponType _weaponType;
        
        private bool _fog;
    
        private State<ActionEntity> _actualState;
    
        public int PlayerLife
        {
            get => _playerLife;
            set => _playerLife = value + _playerLife > maxPlayerLife ? _playerLife = maxPlayerLife : _playerLife += value;
        }

        public float PlayerMoney
        {
            get => _playerMoney;
            set => _playerMoney += value;
        }

        public WeaponType WeaponType
        {
            get => _weaponType;
            set => _weaponType = value;
        }

        public bool Fog
        {
            get => _fog;
            set => _fog = value;
        }
 
        public State<ActionEntity> ActualState
        {
            get => _actualState;
            set => _actualState = value;
        }
        private void Awake()
        {
            PlayerLife = maxPlayerLife;
            WeaponType = weaponType;
        }
    
    }
}