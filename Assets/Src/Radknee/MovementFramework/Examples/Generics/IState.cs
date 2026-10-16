namespace Radknee.MovementFramework
{
    public interface IState
    {
        void Start();
        void Process();
        void End();
        IState Switch();
    }
}