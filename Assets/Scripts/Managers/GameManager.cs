using System.Collections.Generic;
using DesignPatterns;
using GOAP;
using IA2;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameManager : MonoBehaviour , IObservable
    {
        private static GameManager _gameManagerInstance;
        private List<IObserver> _observers = new List<IObserver>();
        [SerializeField] private PlayerState playerState;

        #region InitialHudVariables

        [SerializeField] private GameObject playerGO;
        [SerializeField] private GameObject initialCameraGO;

        #endregion

        public GameManager GameManagerInstance => _gameManagerInstance;

        [SerializeField]private GameObject successFeedBuckGO;
    
        private void Awake()
        {
            SingletonInstance();
            SetInitialComponents();
        }
    

        private void SetInitialValuesHud()
        {
            foreach (var observer in _observers)
            {
                observer.OnNotify(NOTIFY_ACTION_TYPE.SET_LIFE);
                observer.OnNotify(NOTIFY_ACTION_TYPE.SET_MONEY);
                observer.OnNotify(NOTIFY_ACTION_TYPE.SET_WEAPON);
                observer.OnNotify((NOTIFY_ACTION_TYPE.SET_FOG));
            }
        }

        private void SetInitialComponents()
        {
            playerState = GetComponent<PlayerState>();
        }

        private void SingletonInstance()
        {
            if (_gameManagerInstance != null && _gameManagerInstance != this)
            {
                Destroy(this.gameObject);
            }
            else _gameManagerInstance = this;
        }

        public void UpdateLife(int lifeAmount)
        {
            playerState.PlayerLife = lifeAmount;
            foreach (var observer in _observers)
            {
                observer.OnNotify(NOTIFY_ACTION_TYPE.SET_LIFE);
            }
        }
        public int GetPlayerLife() => playerState.PlayerLife;
    
        public void UpdateMoney(float moneyAmount)
        {
            playerState.PlayerMoney = moneyAmount;
            foreach (var observer in _observers)
            {
                observer.OnNotify(NOTIFY_ACTION_TYPE.SET_MONEY);
            }
        }

        public void StartGoapBttn()
        {
            foreach (var observer in _observers)
            {
                observer.OnNotify(NOTIFY_ACTION_TYPE.SET_INITIAL_WEAPON_BTTN);
                observer.OnNotify(NOTIFY_ACTION_TYPE.START_GOAP);
            }
            SetInitialValuesHud();
            playerGO.SetActive(true);
            initialCameraGO.SetActive(false);
        }

        public float GetPlayerMoney() => playerState.PlayerMoney;

        public WeaponType GetPlayerWeaponInWorld() => playerState.WeaponType;

        public bool GetFog() => playerState.Fog;

        public State<ActionEntity> GetActualState => playerState.ActualState;

        public void UpdateActualState(State<ActionEntity> state)
        {
            playerState.ActualState = state;
            foreach (var observer in _observers)
            {
                observer.OnNotify(NOTIFY_ACTION_TYPE.SET_STATE);
            }
        }
        public void SuscribeObserver(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void SetInitialFogBttn(bool state) => playerState.Fog = state;
    
        public void SetInitialWeaponBttn(string weaponTypeStr) => playerState.WeaponType = weaponTypeStr == "Sword" ? WeaponType.Sword : WeaponType.Explosive;
    
        public void Success() => successFeedBuckGO.SetActive(true);

        public void RestartGOAP() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
