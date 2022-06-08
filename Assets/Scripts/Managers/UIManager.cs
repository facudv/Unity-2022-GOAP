using DesignPatterns;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class UIManager : MonoBehaviour, IObserver
    {
        [SerializeField] private GameObject initialSetWorldParamsHud;
        [SerializeField] private GameObject gameplayGoapHud;
    
        [SerializeField] private Dropdown dropDownWeaponInitialSet;

        [SerializeField] private Text lifeTxt;
        [SerializeField] private Text moneyTxt;
        [SerializeField] private Text weaponTxt;
        [SerializeField] private Text fogTxt;
        [SerializeField] private Text actualStateTxt;
    
        private string _initialLifeTxt = "Life : ";
        private string _initialMoneyTxt = "Money : ";
        private string _initialWaponTxt = "Weapon : ";
        private string _initialFogTxt = "Fog : ";
        private string _initialActualStateTxt = "ActualState :";
    
    
        [SerializeField] private GameManager gameManager;

        public void OnNotify(NOTIFY_ACTION_TYPE actionType)
        {
            switch (actionType)
            {
                case NOTIFY_ACTION_TYPE.SET_LIFE : UpdateLifeOnCanvas(); break;
                case NOTIFY_ACTION_TYPE.SET_MONEY: UpdateMoneyOnCanvas(); break;
                case NOTIFY_ACTION_TYPE.SET_WEAPON: UpdateWeaponOnCanvas(); break;
                case NOTIFY_ACTION_TYPE.SET_FOG: UpdateFogOnCanvas(); break;
                case NOTIFY_ACTION_TYPE.SET_STATE: UpdateStateOnCanvas(); break;
                case NOTIFY_ACTION_TYPE.START_GOAP: StartGoapHud(); break;
                case NOTIFY_ACTION_TYPE.SET_INITIAL_WEAPON_BTTN: SetWeaponWorld(); break;
            }
        }

        private void StartGoapHud()
        {
            initialSetWorldParamsHud.SetActive(false);
            gameplayGoapHud.SetActive(true);
        }

        private void Awake()
        {
            gameManager.GameManagerInstance.SuscribeObserver(this);
        }
    

        private void UpdateLifeOnCanvas() => lifeTxt.text = _initialLifeTxt + gameManager.GameManagerInstance.GetPlayerLife();
        private void UpdateMoneyOnCanvas()=> moneyTxt.text = _initialMoneyTxt + gameManager.GameManagerInstance.GetPlayerMoney();
        private void UpdateWeaponOnCanvas()=> weaponTxt.text = _initialWaponTxt + gameManager.GameManagerInstance.GetPlayerWeaponInWorld();
        private void UpdateFogOnCanvas() => fogTxt.text = _initialFogTxt + gameManager.GameManagerInstance.GetFog();
        private void UpdateStateOnCanvas() => actualStateTxt.text = _initialActualStateTxt + gameManager.GameManagerInstance.GetActualState.Name;

        private void SetWeaponWorld() => gameManager.GameManagerInstance.SetInitialWeaponBttn(dropDownWeaponInitialSet.captionText.text);
    }
}
