namespace DesignPatterns
{
    public enum NOTIFY_ACTION_TYPE
    {
        SET_LIFE,
        SET_MONEY,
        SET_WEAPON,
        SET_FOG,
        SET_STATE,
        START_GOAP,
        SET_INITIAL_WEAPON_BTTN
    }

    public interface IObserver
    {
        void OnNotify(NOTIFY_ACTION_TYPE actionType);
    }
}