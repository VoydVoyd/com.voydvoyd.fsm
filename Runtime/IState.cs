namespace VoydVoyd.FSM
{
    public interface IState
    {
        void Tick(object param = null);

        void OnEnter();

        void OnExit();
    }

    public interface IFixedTick
    {
        void FixedTick();
    }

}